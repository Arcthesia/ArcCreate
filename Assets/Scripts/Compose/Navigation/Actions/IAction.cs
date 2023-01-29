namespace ArcCreate.Compose.Navigation
{
    public interface IAction
    {
        string Id { get; }

        string FullPath { get; }

        string CategoryI18nName { get; }

        string I18nName { get; }

        bool ShouldDisplayOnContextMenu { get; }

        void Execute();
    }
}