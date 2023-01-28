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
        /// Whether or not the action should be executed.
        /// </summary>
        /// <param name="action">The action to check.</param>
        /// <returns>The boolean value.</returns>
        bool ShouldExecute(IAction action);

        /// <summary>
        /// Gets all actions that has their requirements fulfilled.
        /// </summary>
        /// <param name="calledByAction">Whether or not this method was called through an action.</param>
        /// <returns>The list of available editor actions.</returns>
        List<IAction> GetContextMenuEntries(bool calledByAction = false);
    }
}