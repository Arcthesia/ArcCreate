using System;

namespace ArcCreate.Compose.History
{
    public interface IHistoryService
    {
        int UndoCount { get; }

        int RedoCount { get; }

        DateTime LastEdit { get; }

        void AddCommand(ICommand command);

        void Undo();

        void Redo();
    }
}