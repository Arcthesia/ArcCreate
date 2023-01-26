namespace ArcCreate.Compose.History
{
    public interface ICommand
    {
        void Undo();

        void Execute();
    }
}