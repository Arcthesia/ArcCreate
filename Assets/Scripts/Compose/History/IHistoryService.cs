using System;

namespace ArcCreate.Compose.History
{
    public interface IHistoryService
    {
        DateTime LastEdit { get; }

        void AddCommand(ICommand command);

        void Undo();

        void Redo();
    }
}