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
        /// <param name="id">The identifier of the sub-action. Set to null to infer from method name.</param>
        /// <param name="shouldDisplayOnContextMenu">Whether or not to display this action on the context menu.</param>
        /// <param name="defaultHotkeys">The default hotkey strings, parsed by <see cref="KeybindUtils.TryParseKeybind(string, out Keybind, out string)"/>.</param>
        public SubActionAttribute(
            string id = null,
            bool shouldDisplayOnContextMenu = true,
            params string[] defaultHotkeys)
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