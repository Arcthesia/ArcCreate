using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.Remote.Common
{
    public class MessageChannel : IDisposable
    {
        private TcpListener listener;
        private TcpClient targetClient;
        private Thread listenerThread;

        private readonly int port;
        private readonly IPAddress ip;
        private readonly IProtocol protocol;

        public MessageChannel(IPAddress ip, int port, IProtocol protocol)
        {
            this.port = port;
            this.ip = ip;
            this.protocol = protocol;
        }

        public bool IsRunning { get; private set; }

        public IPAddress IPAddress => ip;

        public int Port => port;

        public async UniTask Setup()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Message channel is already running");
            }

            listenerThread = new Thread(new ThreadStart(ListenForData))
            {
                IsBackground = true,
            };

            listenerThread.Start();

            int retry = 10;

            Exception e = null;
            while (retry > 0)
            {
                await UniTask.Delay(1000);
                if (TryConnectToTcpServer(out e))
                {
                    break;
                }

                retry -= 1;
            }

            if (retry <= 0)
            {
                throw new Exception($"Could not connect to socket: {e}");
            }

            IsRunning = true;
        }

        public void SendMessage(byte[] msg)
        {
            try
            {
                if (targetClient == null)
                {
                    throw new Exception("No client to send message to");
                }

                NetworkStream stream = targetClient.GetStream();
                if (stream.CanWrite)
                {
                    stream.Write(msg, 0, msg.Length);
                }
                else
                {
                    throw new Exception("Socket stream is not writable");
                }
            }
            catch (SocketException socketException)
            {
                Debug.LogError("Socket exception: " + socketException);
            }
        }

        public void Dispose()
        {
            IsRunning = false;
            targetClient?.Close();
            listenerThread.Abort();
        }

        private bool TryConnectToTcpServer(out Exception exception)
        {
            try
            {
                targetClient = new TcpClient(ip.ToString(), port);
                exception = null;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("On client connect exception " + e);
                exception = e;
                return false;
            }
        }

        private void ListenForData()
        {
            try
            {
                listener = new TcpListener(ip, port);
                listener.Start();
                byte[] bytes = new byte[1024 * 1024 * 5]; // 5MB
                while (true)
                {
                    using (TcpClient connectedClient = listener.AcceptTcpClient())
                    {
                        using (NetworkStream stream = connectedClient.GetStream())
                        {
                            int length;
                            while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                var incomingData = new byte[length];
                                Array.Copy(bytes, 0, incomingData, 0, length);
                                protocol.Process(incomingData);
                            }
                        }
                    }
                }
            }
            catch (SocketException socketException)
            {
                Debug.LogError("Socket exception: " + socketException);
            }
            catch (ThreadAbortException)
            {
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogError("Invalid operation exception: " + exception);
            }
        }
    }
}