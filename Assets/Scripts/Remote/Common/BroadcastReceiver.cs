using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ArcCreate.Remote.Common
{
    public class BroadcastReceiver : IDisposable
    {
        private readonly UdpClient udpClient;
        private readonly Thread thread;
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

            thread = new Thread(Receive)
            {
                IsBackground = true,
            };
            thread.Start();
        }

        public delegate void OnBroadcastDelegate(IPAddress ipAddress, string message);

        public event OnBroadcastDelegate OnBroadcastReceived;

        public bool IsRunning { get; private set; }

        public void Dispose()
        {
            udpClient.Close();
            udpClient.Dispose();
            thread.Abort();
            IsRunning = false;
        }

        private void Receive()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            while (true)
            {
                try
                {
                    byte[] bytes = udpClient.Receive(ref ip);
                    string message = Encoding.ASCII.GetString(bytes);
                    OnBroadcastReceived?.Invoke(ip.Address, message);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Debug.Log("Error receiving " + e.Message);
                }
            }
        }
    }
}