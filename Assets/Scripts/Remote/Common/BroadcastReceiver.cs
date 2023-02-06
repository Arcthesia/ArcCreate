using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ArcCreate.Remote.Common
{
    public class BroadcastReceiver : IDisposable
    {
        private readonly UdpClient udpClient;
        private readonly int port;

        public BroadcastReceiver(int port)
        {
            this.port = port;
            IsRunning = true;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.Bind(endPoint);

            udpClient.BeginReceive(Receive, new object());
        }

        public delegate void OnBroadcastDelegate(IPAddress ipAddress, string message);

        public event OnBroadcastDelegate OnBroadcastReceived;

        public bool IsRunning { get; private set; }

        public void Dispose()
        {
            udpClient.Close();
            IsRunning = true;
        }

        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            byte[] bytes = udpClient.EndReceive(ar, ref ip);
            string message = Encoding.ASCII.GetString(bytes);
            OnBroadcastReceived?.Invoke(ip.Address, message);
            udpClient.BeginReceive(Receive, new object());
        }
    }
}