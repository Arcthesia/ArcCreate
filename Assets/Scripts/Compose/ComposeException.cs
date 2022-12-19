namespace ArcCreate.Compose
{
    public class ComposeException : System.Exception
    {
        public ComposeException(string reason)
            : base(reason)
        {
        }
    }
}