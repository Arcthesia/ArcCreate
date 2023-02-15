using System;

namespace ArcCreate.Compose.History
{
    public interface IHistoryService
    {
        int UndoCount { get; }

        int RedoCount { get; }

        void AddCommand(ICommand command);

        void AddCommandWithoutExecuting(ICommand command);

        void Undo();

        void Redo();
    }
}