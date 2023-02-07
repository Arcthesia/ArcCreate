namespace ArcCreate.Remote.Common
{
    public interface IProtocol
    {
        void Process(RemoteControl control, byte[] message);
    }
}