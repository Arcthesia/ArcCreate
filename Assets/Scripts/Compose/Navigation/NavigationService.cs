using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace ArcCreate.Compose.Navigation
{
    [EditorScope("Navigation")]
    public class NavigationService : MonoBehaviour, INavigationService
    {
        private readonly List<IAction> allActions = new List<IAction>();
        private readonly List<Keybind> keybinds = new List<Keybind>();

        // Last action in the list is considered top-priority, and only it will have sub-actions processed.
        // A stack was not used because lower-priority actions might exit early.
        private readonly List<EditorAction> actionsInProgress = new List<EditorAction>();

        public void StartAction(EditorAction action)
        {
            CancelOngoingKeybinds();
            ExecuteActionTask(action).Forget();
        }

        public List<IAction> GetContextMenuEntries(bool calledByAction = false)
        {
            if (calledByAction)
            {
                EditorAction caller = actionsInProgress[actionsInProgress.Count - 1];
                actionsInProgress.RemoveAt(actionsInProgress.Count - 1);
                List<IAction> result = allActions
                    .Where(action => ShouldExecute(action))
                    .ToList();
                actionsInProgress.Add(caller);
                return result;
            }

            return allActions.Where(ShouldExecute).ToList();
        }

        public bool ShouldExecute(IAction action)
        {
            EditorAction currentAction = null;
            if (actionsInProgress.Count != 0)
            {
                currentAction = actionsInProgress[actionsInProgress.Count - 1];
            }

            if (action is EditorAction editorAction)
            {
                bool whitelisted = true;
                if (currentAction != null && !currentAction.Whitelist.Contains(editorAction.Scope.Type))
                {
                    whitelisted = false;
                }

                return whitelisted && editorAction.CheckRequirement();
            }
            else if (action is SubAction subAction)
            {
                if (currentAction == null)
                {
                    return false;
                }

                return currentAction.SubActions.Contains(subAction);
            }

            return false;
        }

        [EditorAction("Cancel", false, "<esc>")]
        private void CancelOngoingKeybinds()
        {
            foreach (var keybind in keybinds)
            {
                keybind.Reset();
            }
        }

        private void Awake()
        {
            RegisterMethods();
        }

        private void RegisterMethods()
        {
            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsDefined(typeof(EditorScopeAttribute)));

            keybinds.Clear();
            foreach (Type type in types)
            {
                string scopeId = type.GetCustomAttribute<EditorScopeAttribute>().Id ?? type.Name;

                object instance;
                if (type.IsSubclassOf(typeof(Component)))
                {
                    instance = FindObjectOfType(type);
                }
                else
                {
                    instance = Activator.CreateInstance(type);
                }

                if (instance == null)
                {
                    throw new Exception($"Can not get an instance for type {type}");
                }

                foreach (MethodInfo method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                {
                    EditorActionAttribute editorAction = method.GetCustomAttribute(typeof(EditorActionAttribute)) as EditorActionAttribute;
                    if (editorAction == null)
                    {
                        continue;
                    }

                    ContextRequirementAttribute[] contextRequirements = method.GetCustomAttributes(typeof(ContextRequirementAttribute)) as ContextRequirementAttribute[];
                    SubActionAttribute[] subActions = method.GetCustomAttributes(typeof(SubActionAttribute)) as SubActionAttribute[];
                    WhitelistScopesAttribute whitelist = method.GetCustomAttribute(typeof(WhitelistScopesAttribute)) as WhitelistScopesAttribute;

                    // Resolve subactions
                    List<SubAction> subActionInstances = new List<SubAction>();
                    if (subActions != null)
                    {
                        foreach (SubActionAttribute s in subActions)
                        {
                            SubAction subAction = new SubAction(s.Id, editorAction.Id, s.ShouldDisplayOnContextMenu);
                            foreach (string hotkey in s.DefaultHotkeys)
                            {
                                if (KeybindUtils.TryParseKeybind(hotkey, subAction, out Keybind keybind, out string reason))
                                {
                                    keybinds.Add(keybind);
                                }
                                else
                                {
                                    Debug.LogWarning(reason);
                                }
                            }

                            subActionInstances.Add(subAction);
                            allActions.Add(subAction);
                        }
                    }

                    EditorAction action = new EditorAction(
                        id: editorAction.Id ?? method.Name,
                        shouldDisplayOnContextMenu: editorAction.ShouldDisplayOnContextMenu,
                        contextRequirements: contextRequirements?.Cast<IContextRequirement>().ToList() ?? new List<IContextRequirement>(),
                        whitelist: whitelist?.Scopes.ToList() ?? new List<Type>(),
                        scope: new EditorScope(type, scopeId, instance),
                        method: method,
                        subActions: subActionInstances);

                    action.Whitelist.Add(GetType());

                    // Resolve keybinds
                    foreach (string hotkey in editorAction.DefaultHotkeys)
                    {
                        if (KeybindUtils.TryParseKeybind(hotkey, action, out Keybind keybind, out string reason))
                        {
                            keybinds.Add(keybind);
                        }
                        else
                        {
                            Debug.LogWarning(reason);
                        }
                    }

                    allActions.Add(action);
                }
            }
        }

        private async UniTask ExecuteActionTask(EditorAction action)
        {
            actionsInProgress.Add(action);

            // This causes boxing but what else can you do
            object obj = action.Method.Invoke(action.Scope.Instance, action.ParamsToPass);
            if (obj is UniTask task)
            {
                await task;
            }

            actionsInProgress.Remove(action);
        }
    }
}