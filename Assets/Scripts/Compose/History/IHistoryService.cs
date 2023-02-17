namespace ArcCreate.Compose.History
{
    public interface IHistoryService
    {
        /// <summary>
        /// Gets the current undo-able actions count.
        /// </summary>
        int UndoCount { get; }

        /// <summary>
        /// Gets the current redo-able actions count.
        /// </summary>
        int RedoCount { get; }

        /// <summary>
        /// Add a command to the history and execute it. Commands added to the history can be undoed.
        /// </summary>
        /// <param name="command">The command to add and execute.</param>
        void AddCommand(ICommand command);

        /// <summary>
        /// Add a command to the history and without executing it. Commands added to the history can be undoed.
        /// Use this over <see cref="AddCommand(ICommand)"/> .
        /// </summary>
        /// <param name="command">The command to add and execute.</param>
        void AddCommandWithoutExecuting(ICommand command);

        void Undo();

        void Redo();
    }
}