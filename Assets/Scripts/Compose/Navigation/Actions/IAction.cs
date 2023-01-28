namespace ArcCreate.Compose.Navigation
{
    public interface IAction
    {
        string Id { get; }

        string Category { get; }

        bool ShouldDisplayOnContextMenu { get; }

        void Execute();
    }
}