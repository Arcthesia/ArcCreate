namespace ArcCreate.Compose.Components
{
    public interface IItemNameDialogConsumer
    {
        bool IsValidName(string value, out string reason);

        void SaveItem(string value);
    }
}