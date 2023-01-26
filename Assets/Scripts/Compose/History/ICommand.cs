namespace ArcCreate.Compose.History
{
    public interface ICommand
    {
        string Name { get; }

        void Undo();

        void Execute();
    }
}