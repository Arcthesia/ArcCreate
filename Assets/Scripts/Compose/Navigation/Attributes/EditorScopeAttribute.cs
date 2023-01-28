using System;

namespace ArcCreate.Compose.Navigation
{
    /// <summary>
    /// Attribute for marking a class as an editor scope.
    /// An editor scope is a way to group editor actions together into a single scope,
    /// which can be whitelisted by other editor actions all at once (see <see cref="WhitelistScopesAttribute"/>).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorScopeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorScopeAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The name of the scope displayed on the context menu.</param>
        public EditorScopeAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; private set; }
    }
}