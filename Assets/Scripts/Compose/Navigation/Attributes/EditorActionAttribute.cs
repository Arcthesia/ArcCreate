using System;

namespace ArcCreate.Compose.Navigation
{
    /// <summary>
    /// Attribute for marking a method as an editor action.
    /// An editor action can be activated by navigations.
    /// The method must belong to a class with the attribute <see cref="EditorScopeAttribute"/> to be recognizeed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EditorActionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorActionAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The name of the action displayed on context menu. Set to null to hide this action from context menu.</param>
        /// <param name="defaultHotkeys">The default hotkey strings, parsed by <see cref="KeybindUtils.TryParseKeybind(string, out Keybind, out string)"/>.</param>
        public EditorActionAttribute(string displayName = null, params string[] defaultHotkeys)
        {
            DisplayName = displayName;
            DefaultHotkeys = defaultHotkeys;
        }

        public string DisplayName { get; private set; }

        public string[] DefaultHotkeys { get; private set; }
    }
}