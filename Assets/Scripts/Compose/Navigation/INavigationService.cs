using System.Collections.Generic;

namespace ArcCreate.Compose.Navigation
{
    public interface INavigationService
    {
        /// <summary>
        /// Start an editor action.
        /// </summary>
        /// <param name="action">The editor action.</param>
        void StartAction(EditorAction action);

        /// <summary>
        /// Gets all editor actions that has their requirements fulfilled.
        /// </summary>
        /// <returns>The list of available editor actions.</returns>
        IEnumerable<EditorAction> GetCurrentlyAvailableActions();
    }
}