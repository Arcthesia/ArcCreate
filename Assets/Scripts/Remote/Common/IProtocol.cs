namespace ArcCreate.Remote.Common
{
    public interface IProtocol
    {
        void Process(byte[] data);
    }
}