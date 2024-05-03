using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ArcCreate.Compose.Components;
using ArcCreate.Utility;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YamlDotNet.RepresentationModel;

namespace ArcCreate.Compose.Navigation
{
    [EditorScope("Navigation")]
    public class NavigationService : MonoBehaviour, INavigationService
    {
        private readonly List<IAction> allActions = new List<IAction>();
        private readonly List<Keybind> keybinds = new List<Keybind>();
        private readonly Dictionary<Type, object> instances = new Dictionary<Type, object>();
        private readonly List<Keybind> activatingKeybinds = new List<Keybind>();
        private readonly List<Keybind> inProgressKeybinds = new List<Keybind>();

        // Last action in the list is considered top-priority, and only it will have sub-actions processed.
        // A stack was not used because lower-priority actions might exit early.
        private readonly List<EditorAction> actionsInProgress = new List<EditorAction>();
        private readonly Dictionary<IContextRequirement, bool> contextRequirementStates = new Dictionary<IContextRequirement, bool>();
        private readonly List<IAction> cachedActionList = new List<IAction>();
        private readonly List<Keybind> keybindsToDisplayHint = new List<Keybind>();
        private readonly List<Keybind> cachedKeybindList = new List<Keybind>();
        private List<IContextRequirement> contextRequirements = new List<IContextRequirement>();

        [SerializeField] private KeybindHintList keybindHintList;

        public string ConfigFilePath
        {
            get
            {
                string path = Path.Combine(Application.streamingAssetsPath, Values.KeybindSettingsFileName + ".yaml");
                if (!File.Exists(path))
                {
                    path = Path.Combine(Application.streamingAssetsPath, Values.KeybindSettingsFileName + ".yml");
                }

                return path;
            }
        }

        public void ReloadHotkeys()
        {
            allActions.Clear();
            keybinds.Clear();
            activatingKeybinds.Clear();
            inProgressKeybinds.Clear();
            contextRequirementStates.Clear();
            keybindsToDisplayHint.Clear();

            Dictionary<string, List<string>> keybindOverrides = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> keybindActions = new Dictionary<string, List<string>>();

            string configPath = ConfigFilePath;
            if (File.Exists(configPath))
            {
                using (FileStream stream = File.OpenRead(configPath))
                {
                    YamlStream yaml = new YamlStream();
                    yaml.Load(new StreamReader(stream));

                    if (yaml.Documents.Count >= 1)
                    {
                        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

                        foreach (KeyValuePair<YamlNode, YamlNode> child in mapping.Children)
                        {
                            string nodeKey = (child.Key as YamlScalarNode).Value;
                            YamlNode val = child.Value;
                            if (!(val is YamlMappingNode valueNode))
                            {
                                continue;
                            }

                            if (nodeKey == "Override")
                            {
                                YamlExtractor.ExtractListsTo(keybindOverrides, valueNode, "");
                            }
                            else if (nodeKey == "Action")
                            {
                                YamlExtractor.ExtractListsTo(keybindActions, valueNode, "");
                            }
                        }
                    }
                }
            }

            RegisterMethods(keybindOverrides);
            RegisterCompositeActions(keybindActions);
        }

        public void StartAction(EditorAction action)
        {
            ExecuteActionTask(action).Forget();
        }

        public void StartActionsInSequence(List<IAction> actions)
        {
            ExecuteActionListTask(actions).Forget();
        }

        public void StartAction(string fullPath)
        {
            foreach (IAction action in allActions)
            {
                if (action.FullPath == fullPath)
                {
                    action.Execute();
                    return;
                }
            }

            Services.Macros.RunMacro(fullPath);
        }

        public List<IAction> GetExecutableActions(bool calledByAction = false, Func<IAction, bool> predicate = null)
        {
            if (calledByAction)
            {
                EditorAction caller = actionsInProgress[actionsInProgress.Count - 1];
                actionsInProgress.RemoveAt(actionsInProgress.Count - 1);

                cachedActionList.Clear();
                foreach (var a in allActions)
                {
                    if (predicate.Invoke(a) & ShouldExecute(a))
                    {
                        cachedActionList.Add(a);
                    }
                }

                actionsInProgress.Add(caller);
                return cachedActionList;
            }

            cachedActionList.Clear();
            foreach (var a in allActions)
            {
                if (predicate.Invoke(a) & ShouldExecute(a))
                {
                    cachedActionList.Add(a);
                }
            }

            return cachedActionList;
        }

        public List<Keybind> GetKeybindsToDisplay()
        {
            cachedKeybindList.Clear();
            foreach (var k in keybindsToDisplayHint)
            {
                if (ShouldExecute(k.Action))
                {
                    cachedKeybindList.Add(k);
                }
            }

            return cachedKeybindList;
        }

        public void RefreshKeybindHint()
        {
            keybindHintList.RebuildList();
        }

        public bool ShouldExecute(IAction action)
        {
            if (EventSystem.current.currentSelectedGameObject != null &&
                (EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null
              || EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null))
            {
                return false;
            }

            if (Dialog.IsAnyOpen)
            {
                return false;
            }

            EditorAction currentAction = null;
            if (actionsInProgress.Count != 0)
            {
                currentAction = actionsInProgress[actionsInProgress.Count - 1];
            }

            switch (action)
            {
                case EditorAction editorAction:
                    bool whitelisted = true;
                    if (currentAction != null
                    && !currentAction.WhitelistAll
                    && !currentAction.Whitelist.Contains(editorAction.Scope.Type))
                    {
                        whitelisted = false;
                    }

                    return whitelisted && editorAction.CheckRequirement();
                case SubAction subAction:
                    if (currentAction == null)
                    {
                        return false;
                    }

                    return !subAction.ForceDisabled && currentAction.SubActions.Contains(subAction);
                case CompositeAction compositeAction:
                    return true;
                case MacroAction macroAction:
                    return true;
            }

            return false;
        }

        [EditorAction("Cancel", false, "<esc>")]
        [KeybindHint(Exclude = true)]
        private void CancelOngoingKeybinds()
        {
            foreach (var keybind in keybinds)
            {
                keybind.Reset();
            }
        }

        private void Awake()
        {
            ReloadHotkeys();
        }

        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject != null &&
                (EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null
              || EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null))
            {
                keybindHintList.EnableDisplay = false;
            }
            else
            {
                keybindHintList.EnableDisplay = true;
            }

            CheckKeybind();
            CheckContextRequirementChanges();
        }

        private void OnDestroy()
        {
            keybinds.Clear();
        }

        private void RegisterMethods(Dictionary<string, List<string>> keybindOverrides)
        {
            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsDefined(typeof(EditorScopeAttribute)));

            foreach (Type type in types)
            {
                string scopeId = type.GetCustomAttribute<EditorScopeAttribute>().Id ?? type.Name;

                object instance;
                if (instances.TryGetValue(type, out object typeInstance))
                {
                    instance = typeInstance;
                }
                else if (type.IsSubclassOf(typeof(Component)))
                {
                    instance = FindComponent(type);
                    instances.Add(type, instance);
                }
                else
                {
                    instance = Activator.CreateInstance(type);
                    instances.Add(type, instance);
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

                    string actionId = editorAction.Id ?? method.Name;
                    ContextRequirementAttribute[] contextRequirements = method.GetCustomAttributes(typeof(ContextRequirementAttribute)) as ContextRequirementAttribute[];
                    SubActionAttribute[] subActions = method.GetCustomAttributes(typeof(SubActionAttribute)) as SubActionAttribute[];
                    WhitelistScopesAttribute whitelist = method.GetCustomAttribute(typeof(WhitelistScopesAttribute)) as WhitelistScopesAttribute;
                    KeybindHintAttribute[] hint = method.GetCustomAttributes(typeof(KeybindHintAttribute)) as KeybindHintAttribute[];
                    KeybindHintAttribute editorHint = hint.FirstOrDefault(h => string.IsNullOrEmpty(h.SubActionId));

                    // Resolve subactions
                    List<SubAction> subActionInstances = new List<SubAction>();
                    if (subActions != null)
                    {
                        foreach (SubActionAttribute s in subActions)
                        {
                            SubAction subAction = new SubAction(s.Id, scopeId, actionId, s.ShouldDisplayOnContextMenu);
                            KeybindHintAttribute subHint = hint.FirstOrDefault(h => h.SubActionId == s.Id);

                            IEnumerable<string> keybindStrings = s.DefaultHotkeys;
                            if (keybindOverrides.TryGetValue(subAction.FullPath, out List<string> keybindOverride))
                            {
                                keybindStrings = keybindOverride;
                                Debug.Log(I18n.S("Compose.Navigation.KeybindOverride", subAction.FullPath));
                            }

                            bool subfirst = true;
                            foreach (string keybindString in keybindStrings)
                            {
                                if (string.IsNullOrEmpty(keybindString))
                                {
                                    continue;
                                }

                                if (KeybindUtils.TryParseKeybind(keybindString, subAction, out Keybind keybind, out string reason))
                                {
                                    keybinds.Add(keybind);
                                    if (subfirst && !(subHint?.Exclude ?? false) && !(editorHint?.Exclude ?? false))
                                    {
                                        keybind.Priority = subHint?.Priority ?? 0;
                                        keybindsToDisplayHint.Add(keybind);
                                        subfirst = false;
                                    }
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

                    var contextRequirementList = contextRequirements?.Cast<IContextRequirement>().ToList() ?? new List<IContextRequirement>();
                    foreach (var req in contextRequirementList)
                    {
                        if (!contextRequirementStates.ContainsKey(req))
                        {
                            contextRequirementStates.Add(req, false);
                        }
                    }

                    EditorAction action = new EditorAction(
                        id: actionId,
                        shouldDisplayOnContextMenu: editorAction.ShouldDisplayOnContextMenu,
                        contextRequirements: contextRequirementList,
                        whitelist: whitelist?.Scopes.ToList() ?? new List<Type>(),
                        whitelistAll: whitelist?.All ?? false,
                        scope: new EditorScope(type, scopeId, instance),
                        method: method,
                        subActions: subActionInstances);

                    action.Whitelist.Add(GetType());

                    // Resolve keybinds
                    IEnumerable<string> actionKeybindStrings = editorAction.DefaultHotkeys;
                    if (keybindOverrides.TryGetValue(action.FullPath, out List<string> actionKeybindOverride))
                    {
                        actionKeybindStrings = actionKeybindOverride;
                        Debug.Log(I18n.S("Compose.Navigation.KeybindOverride", action.FullPath));
                    }

                    bool first = true;
                    foreach (string keybindString in actionKeybindStrings)
                    {
                        if (string.IsNullOrEmpty(keybindString))
                        {
                            continue;
                        }

                        if (KeybindUtils.TryParseKeybind(keybindString, action, out Keybind keybind, out string reason))
                        {
                            keybind.Priority = editorHint?.Priority ?? 0;
                            keybinds.Add(keybind);
                            if (first && !(editorHint?.Exclude ?? false))
                            {
                                keybindsToDisplayHint.Add(keybind);
                                first = false;
                            }
                        }
                        else
                        {
                            Debug.LogWarning(reason);
                        }
                    }

                    allActions.Add(action);
                }

                keybindsToDisplayHint.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                contextRequirements = contextRequirementStates.Keys.ToList();
            }
        }

        private void RegisterCompositeActions(Dictionary<string, List<string>> keybindActions)
        {
            foreach (KeyValuePair<string, List<string>> pair in keybindActions)
            {
                string keybindString = pair.Key;
                List<string> actionPaths = pair.Value;

                List<IAction> actions = new List<IAction>();
                foreach (string actionPath in actionPaths)
                {
                    bool found = false;
                    foreach (IAction registeredAction in allActions)
                    {
                        if (registeredAction.FullPath == actionPath)
                        {
                            actions.Add(registeredAction);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        actions.Add(new MacroAction(actionPath));
                    }
                }

                CompositeAction action = new CompositeAction(keybindString, actions);
                if (string.IsNullOrEmpty(keybindString))
                {
                    continue;
                }

                if (KeybindUtils.TryParseKeybind(keybindString, action, out Keybind keybind, out string reason))
                {
                    keybinds.Add(keybind);
                }
                else
                {
                    Debug.LogWarning(reason);
                }
            }
        }

        private void CheckKeybind()
        {
            activatingKeybinds.Clear();
            inProgressKeybinds.Clear();
            for (int i = 0; i < keybinds.Count; i++)
            {
                Keybind keybind = keybinds[i];
                KeybindState state = keybind.CheckKeybind();
                switch (state)
                {
                    case KeybindState.Complete:
                        activatingKeybinds.Add(keybind);
                        break;
                    case KeybindState.InProgress:
                        inProgressKeybinds.Add(keybind);
                        break;
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftShift)
             || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.LeftAlt)
             || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftControl)
             || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyUp(KeyCode.RightShift)
             || Input.GetKeyDown(KeyCode.RightAlt) || Input.GetKeyUp(KeyCode.RightAlt)
             || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyUp(KeyCode.RightControl))
            {
                keybindHintList.RebuildList();
            }

            for (int i = 0; i < activatingKeybinds.Count; i++)
            {
                Keybind keybind = activatingKeybinds[i];
                Keystroke lastKeystroke = keybind.Keystrokes[keybind.Keystrokes.Length - 1];
                int complexity = lastKeystroke.Modifiers.Length;
                KeyCode key = lastKeystroke.Key;
                bool isMaxComplexity = true;
                bool hasStartedLate = false;

                for (int j = 0; j < activatingKeybinds.Count; j++)
                {
                    Keybind otherKeybind = activatingKeybinds[j];
                    Keystroke otherLastKeystroke = otherKeybind.Keystrokes[otherKeybind.Keystrokes.Length - 1];
                    if (i == j)
                    {
                        continue;
                    }

                    if (otherLastKeystroke.Modifiers.Contains(key))
                    {
                        isMaxComplexity = false;
                    }

                    if (lastKeystroke.Key != otherLastKeystroke.Key)
                    {
                        continue;
                    }

                    int otherComplexity = otherLastKeystroke.Modifiers.Length;
                    if (complexity < otherComplexity)
                    {
                        isMaxComplexity = false;
                    }
                }

                for (int j = 0; j < inProgressKeybinds.Count; j++)
                {
                    Keybind otherKeybind = inProgressKeybinds[j];
                    Keystroke otherCurrentKeystroke = otherKeybind.Keystrokes[otherKeybind.CurrentIndex - 1];
                    if (otherCurrentKeystroke.Modifiers.Contains(key))
                    {
                        isMaxComplexity = false;
                    }

                    if (lastKeystroke.Key != otherCurrentKeystroke.Key)
                    {
                        continue;
                    }

                    int otherComplexity = otherCurrentKeystroke.Modifiers.Length;
                    if (complexity < otherComplexity)
                    {
                        isMaxComplexity = false;
                    }

                    if (keybind.Keystrokes.Length < otherKeybind.CurrentIndex)
                    {
                        hasStartedLate = true;
                    }
                }

                if (isMaxComplexity && !hasStartedLate && ShouldExecute(keybind.Action))
                {
                    keybind.Action.Execute();
                }
            }
        }

        private void CheckContextRequirementChanges()
        {
            bool changed = false;
            foreach (IContextRequirement req in contextRequirements)
            {
                bool previousState = contextRequirementStates[req];
                bool newState = req.CheckRequirement();
                if (previousState != newState)
                {
                    changed = true;
                    contextRequirementStates[req] = newState;
                }
            }

            if (changed)
            {
                keybindHintList.RebuildList();
            }
        }

        private async UniTask ExecuteActionTask(EditorAction action)
        {
            // Ensure all keybinds that can trigger will do so first before resetting the rest
            await UniTask.WaitForEndOfFrame(this);
            CancelOngoingKeybinds();

            actionsInProgress.Add(action);
            bool rebuiltList = false;

            // Without try-catch the entire navigation system stops working when any exception is thrown
            try
            {
                // This causes boxing but what else can you do
                object obj = action.Method.Invoke(action.Scope.Instance, action.ParamsToPass);
                if (obj is UniTask task)
                {
                    await UniTask.DelayFrame(5);
                    if (task.Status != UniTaskStatus.Succeeded)
                    {
                        rebuiltList = true;
                        keybindHintList.RebuildList();
                        await task;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            actionsInProgress.Remove(action);
            if (rebuiltList)
            {
                keybindHintList.RebuildList();
            }
        }

        private async UniTask ExecuteActionListTask(List<IAction> actions)
        {
            foreach (IAction action in actions)
            {
                if (!ShouldExecute(action))
                {
                    return;
                }

                if (action is EditorAction editorAction)
                {
                    await ExecuteActionTask(editorAction);
                }
                else
                {
                    action.Execute();
                }
            }
        }

        private object FindComponent(Type type)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                var allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {
                    var go = allGameObjects[j];
                    object comp = go.GetComponentInChildren(type, true);
                    if (comp != null)
                    {
                        return comp;
                    }
                }
            }

            return null;
        }
    }
}