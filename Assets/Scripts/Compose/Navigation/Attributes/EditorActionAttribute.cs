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
        /// <param name="id">The identifier of the action. Set to null to infer from method name.</param>
        /// <param name="shouldDisplayOnContextMenu">Whether or not to display this action on the context menu.</param>
        /// <param name="defaultHotkeys">The default hotkey strings, parsed by <see cref="KeybindUtils.TryParseKeybind(string, out Keybind, out string)"/>.</param>
        public EditorActionAttribute(string id, bool shouldDisplayOnContextMenu = true, params string[] defaultHotkeys)
        {
            Id = id;
            ShouldDisplayOnContextMenu = shouldDisplayOnContextMenu;
            DefaultHotkeys = defaultHotkeys;
        }

        public string Id { get; private set; }

        public bool ShouldDisplayOnContextMenu { get; private set; }

        public string[] DefaultHotkeys { get; private set; }
    }
}