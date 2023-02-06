using System;
using System.Net;
using System.Text;
using ArcCreate.Gameplay;
using ArcCreate.Remote.Common;
using Cysharp.Threading.Tasks;

namespace ArcCreate.Remote.Gameplay
{
    public class RemoteSessionGameplay : IProtocol, IDisposable
    {
        private readonly IGameplayControl gameplay;
        private readonly MessageChannel channel;

        public RemoteSessionGameplay(IGameplayControl gameplay, IPAddress address, int port)
        {
            this.gameplay = gameplay;
            channel = new MessageChannel(address, port, this);
            channel.Setup().Forget();
        }

        public void Process(byte[] data)
        {
            UnityEngine.Debug.Log(Encoding.ASCII.GetString(data));
        }

        public void Dispose()
        {
            if (channel.IsRunning)
            {
                channel.Dispose();
            }
        }
    }
}