using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArcCreate.Utility;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcCreate.Compose.Navigation
{
    public class NavigationService : MonoBehaviour, INavigationService
    {
        private readonly List<EditorAction> allActions = new List<EditorAction>();
        private readonly List<Keybind> actionKeybinds = new List<Keybind>();
        private readonly UnorderedList<Keybind> keybindsInProgress = default;
        private readonly UnorderedList<Keybind> subActionsKeybindsInProgress = default;

        // Last action in the list is considered top-priority, and only it will have sub-actions processed.
        // A stack was not used because lower-priority actions might exit early.
        private readonly List<EditorAction> actionsInProgress = new List<EditorAction>();

        public void StartAction(EditorAction action)
        {
            subActionsKeybindsInProgress.Clear();
            ExecuteActionTask(action).Forget();
        }

        public IEnumerable<EditorAction> GetCurrentlyAvailableActions()
        {
            return allActions.Where(action => action.CheckRequirement());
        }

        private void Awake()
        {
            RegisterMethods();
        }

        private void Update()
        {
            CheckKeybinds();
        }

        private void RegisterMethods()
        {
            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsDefined(typeof(EditorScopeAttribute)));

            actionKeybinds.Clear();
            foreach (Type type in types)
            {
                string scopeName = type.GetCustomAttribute<EditorScopeAttribute>().DisplayName;
                object instance = Activator.CreateInstance(type);

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
                    List<Keybind> subActionKeybinds = new List<Keybind>();
                    if (subActions != null)
                    {
                        foreach (SubActionAttribute s in subActions)
                        {
                            SubAction subAction = new SubAction(s.DisplayName, s.DisplayName != null);
                            foreach (string hotkey in s.DefaultHotkeys)
                            {
                                if (KeybindUtils.TryParseKeybind(hotkey, out Keybind keybind, out string reason))
                                {
                                    keybind.Action = subAction;
                                    subActionKeybinds.Add(keybind);
                                }
                                else
                                {
                                    Debug.LogWarning(reason);
                                }
                            }
                        }
                    }

                    EditorAction action = new EditorAction(
                        displayName: editorAction.DisplayName,
                        shouldDisplayOnContextMenu: editorAction.DisplayName != null,
                        contextRequirements: contextRequirements.Cast<IContextRequirement>().ToList(),
                        whitelist: whitelist.Scopes.ToList(),
                        scopeInstance: instance,
                        method: method);

                    // Resolve keybinds
                    foreach (string hotkey in editorAction.DefaultHotkeys)
                    {
                        if (KeybindUtils.TryParseKeybind(hotkey, out Keybind keybind, out string reason))
                        {
                            keybind.Action = action;
                            actionKeybinds.Add(keybind);
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

        private void CheckKeybinds()
        {
            Keyboard keyboard = Keyboard.current;
            bool actionExecuted = false;

            if (!keyboard.anyKey.isPressed)
            {
                return;
            }

            EditorAction currentAction = null;
            if (actionsInProgress.Count > 0)
            {
                currentAction = actionsInProgress[actionsInProgress.Count - 1];
            }

            // please help
            // Check for action keybinds
            if (keybindsInProgress.Count == 0)
            {
                // Check all keybinds
                foreach (Keybind keybind in actionKeybinds)
                {
                    EditorAction action = keybind.Action as EditorAction;
                    bool passWhitelist =
                        currentAction == null || action == null ||
                        currentAction.Whitelist.Contains(action.ScopeInstance);

                    if (!passWhitelist)
                    {
                        continue;
                    }

                    KeybindResponse response = keybind.CheckKeystroke(keyboard);
                    if (response == KeybindResponse.Complete)
                    {
                        bool requirementFulfilled = action?.CheckRequirement() ?? true;
                        if (requirementFulfilled)
                        {
                            keybind.Action.Execute();
                            actionExecuted = true;
                        }
                    }
                    else if (response == KeybindResponse.Incomplete)
                    {
                        keybindsInProgress.Add(keybind);
                    }
                }
            }
            else
            {
                // Check keybinds already in progress
                for (int i = keybindsInProgress.Count - 1; i >= 0; i--)
                {
                    Keybind keybind = keybindsInProgress[i];
                    EditorAction action = keybind.Action as EditorAction;
                    bool passWhitelist =
                        currentAction == null || action == null ||
                        currentAction.Whitelist.Contains(action.ScopeInstance);

                    if (!passWhitelist)
                    {
                        continue;
                    }

                    KeybindResponse response = keybind.CheckKeystroke(keyboard);
                    if (response == KeybindResponse.Complete)
                    {
                        bool requirementFulfilled = action?.CheckRequirement() ?? true;
                        if (requirementFulfilled)
                        {
                            keybind.Action.Execute();
                            actionExecuted = true;
                        }
                        else
                        {
                            keybindsInProgress.RemoveAt(i);
                        }
                    }
                    else if (response == KeybindResponse.Invalid)
                    {
                        keybindsInProgress.RemoveAt(i);
                    }
                }
            }

            if (actionExecuted)
            {
                keybindsInProgress.Clear();
            }

            // i really mean it please help
            if (currentAction != null)
            {
                // Check sub-action keybinds of currently active action
                bool subActionExecuted = false;
                if (subActionsKeybindsInProgress.Count == 0)
                {
                    // Check all sub-action keybinds
                    foreach (Keybind keybind in currentAction.SubActionsKeybinds)
                    {
                        SubAction action = keybind.Action as SubAction;
                        KeybindResponse response = keybind.CheckKeystroke(keyboard);
                        if (response == KeybindResponse.Complete)
                        {
                            keybind.Action.Execute();
                            subActionExecuted = true;
                        }
                        else if (response == KeybindResponse.Incomplete)
                        {
                            subActionsKeybindsInProgress.Add(keybind);
                        }
                    }
                }
                else
                {
                    // Check sub-action keybinds in progress
                    for (int i = subActionsKeybindsInProgress.Count - 1; i >= 0; i--)
                    {
                        Keybind keybind = subActionsKeybindsInProgress[i];
                        SubAction action = keybind.Action as SubAction;
                        KeybindResponse response = keybind.CheckKeystroke(keyboard);
                        if (response == KeybindResponse.Complete)
                        {
                            keybind.Action.Execute();
                            subActionExecuted = true;
                        }
                        else if (response == KeybindResponse.Invalid)
                        {
                            subActionsKeybindsInProgress.RemoveAt(i);
                        }
                    }
                }

                if (subActionExecuted)
                {
                    subActionsKeybindsInProgress.Clear();
                }
            }
        }

        private async UniTask ExecuteActionTask(EditorAction action)
        {
            actionsInProgress.Remove(action);

            // This causes boxing but what else can you do
            object obj = action.Method.Invoke(action.ScopeInstance, action.ParamsToPass);
            if (obj is UniTask task)
            {
                await task;
            }

            actionsInProgress.Remove(action);
        }
    }
}