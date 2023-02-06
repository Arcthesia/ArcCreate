using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ArcCreate.Remote.Common
{
    public class BroadcastSender
    {
        private readonly UdpClient udpClient;
        private readonly int port;

        public BroadcastSender(int port)
        {
            this.port = port;
        }

        public void Broadcast(string msg)
        {
            UdpClient client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, port);
            byte[] bytes = Encoding.ASCII.GetBytes(msg);
            client.EnableBroadcast = true;
            client.Send(bytes, bytes.Length, ip);
            client.Close();
        }

        public void Destroy()
        {
            udpClient.Close();
        }
    }
}