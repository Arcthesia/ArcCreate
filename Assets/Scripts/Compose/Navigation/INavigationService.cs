using System.Collections.Generic;

namespace ArcCreate.Compose.Navigation
{
    public interface INavigationService
    {
        /// <summary>
        /// Gets the path to the configuration file. Does not guarantee that the file exists.
        /// </summary>
        string ConfigFilePath { get; }

        /// <summary>
        /// Reload hotkeys from configuration.
        /// </summary>
        void ReloadHotkeys();

        /// <summary>
        /// Start an editor action.
        /// </summary>
        /// <param name="action">The editor action.</param>
        void StartAction(EditorAction action);

        /// <summary>
        /// Start an editor action from a path.
        /// </summary>
        /// <param name="path">The editor action path.</param>
        void StartAction(string path);

        /// <summary>
        /// Starts a sequence of action, and make sure an action only starts after the previous has finished.
        /// </summary>
        /// <param name="actions">The actions to start in sequence.</param>
        void StartActionsInSequence(List<IAction> actions);

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