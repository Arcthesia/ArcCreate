using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;

namespace ArcCreate.Compose.Navigation
{
    public class EditorAction : IAction
    {
        public EditorAction(
            string displayName,
            bool shouldDisplayOnContextMenu,
            List<IContextRequirement> contextRequirements,
            List<Type> whitelist,
            object scopeInstance,
            MethodInfo method)
        {
            DisplayName = displayName;
            ShouldDisplayOnContextMenu = shouldDisplayOnContextMenu;
            ContextRequirements = contextRequirements;
            Whitelist = whitelist;
            ScopeInstance = scopeInstance;
            Method = method;

            bool shouldPassSelf = method.GetParameters().Length != 0;
            if (shouldPassSelf)
            {
                ParamsToPass = new object[] { this };
            }
            else
            {
                ParamsToPass = new object[0];
            }
        }

        public string DisplayName { get; private set; }

        public bool ShouldDisplayOnContextMenu { get; private set; }

        public List<IContextRequirement> ContextRequirements { get; private set; }

        public List<Type> Whitelist { get; private set; }

        public object ScopeInstance { get; private set; }

        public MethodInfo Method { get; private set; }

        public List<SubAction> SubActions { get; private set; }

        public List<Keybind> SubActionsKeybinds { get; private set; }

        public object[] ParamsToPass { get; private set; }

        public void Execute()
        {
            Services.Navigation.StartAction(this);
        }

        public SubAction GetSubAction(string name)
        {
            foreach (SubAction sub in SubActions)
            {
                if (sub.DisplayName == name)
                {
                    return sub;
                }
            }

            throw new Exception($"Invalid subaction name {name}");
        }

        public bool CheckRequirement()
        {
            foreach (IContextRequirement requirement in ContextRequirements)
            {
                if (!requirement.CheckRequirement())
                {
                    return false;
                }
            }

            return true;
        }
    }
}