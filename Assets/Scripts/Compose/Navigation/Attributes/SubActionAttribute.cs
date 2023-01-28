using System;

namespace ArcCreate.Compose.Navigation
{
    /// <summary>
    /// Attribute for registering sub-actions for an editor action.
    /// Sub-actions can be triggered while the editor action is in progress.
    /// Example: When the "Copypasting notes" action is in progress, sub-actions such as "Confirm" or "Cancel" can be triggered.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SubActionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubActionAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The name of the sub-action. Set to null to hide this sub-action from context menu.</param>
        /// <param name="defaultHotkeys">The default hotkey strings, parsed by <see cref="KeybindUtils.TryParseKeybind(string, out Keybind, out string)"/>.</param>
        public SubActionAttribute(
            string displayName = null,
            params string[] defaultHotkeys)
        {
            DisplayName = displayName;
            DefaultHotkeys = defaultHotkeys;
        }

        public string DisplayName { get; private set; }

        public string[] DefaultHotkeys { get; private set; }
    }
}