using System;

namespace ArcCreate.Compose.Navigation
{
    /// <summary>
    /// Attribute for altering keybind hint properties of an action.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class KeybindHintAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeybindHintAttribute"/> class.
        /// </summary>
        /// <param name="subActionId">The id of the target subaction. If set to null, then the target is the editor action.</param>
        public KeybindHintAttribute(string subActionId = null)
        {
            SubActionId = subActionId;
        }

        /// <summary>
        /// Gets or sets the priorty of this keybind on the hint display. Higher priority keybinds are displayed first.
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether to exclude this keybind from the display.
        /// </summary>
        public bool Exclude { get; set; } = false;

        public string SubActionId { get; private set; }
    }
}