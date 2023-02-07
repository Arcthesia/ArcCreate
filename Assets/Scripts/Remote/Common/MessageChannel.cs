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

        private readonly int listenOnPort;
        private readonly int sendToPort;
        private readonly IPAddress ip;
        private readonly IProtocol protocol;
        private readonly MessagePackager packager;

        public MessageChannel(IPAddress ip, int listenOnPort, int sendToPort, IProtocol protocol)
        {
            this.listenOnPort = listenOnPort;
            this.sendToPort = sendToPort;
            this.ip = ip;
            this.protocol = protocol;
            packager = new MessagePackager();
        }

        public bool IsRunning { get; private set; }

        public IPAddress IPAddress => ip;

        public int SendToPort => sendToPort;

        public bool CheckConnection(byte[] data)
        {
            try
            {
                SendMessage(RemoteControl.CheckConnection, data);
                return true;
            }
            catch
            {
                return false;
            }
        }

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

        public void SendMessage(RemoteControl control, byte[] msg)
        {
            try
            {
                if (targetClient == null || !targetClient.Connected)
                {
                    return;
                }

                NetworkStream stream = targetClient.GetStream();
                if (stream.CanWrite)
                {
                    if (msg != null)
                    {
                        byte[] header = packager.CreateHeader(control, msg.Length);
                        stream.Write(header, 0, header.Length);
                        stream.Write(msg, 0, msg.Length);
                    }
                    else
                    {
                        byte[] header = packager.CreateHeader(control, 0);
                        stream.Write(header, 0, header.Length);
                    }

                    stream.Flush();
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
            listener.Stop();
            listenerThread.Abort();
        }

        private bool TryConnectToTcpServer(out Exception exception)
        {
            try
            {
                targetClient = new TcpClient(ip.ToString(), sendToPort);
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
                listener = new TcpListener(ip, listenOnPort);
                listener.Start();
                byte[] buffer = new byte[1024 * 1024 * 5]; // 5MB
                while (true)
                {
                    using (TcpClient connectedClient = listener.AcceptTcpClient())
                    {
                        using (NetworkStream stream = connectedClient.GetStream())
                        {
                            int length;
                            while ((length = stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                packager.ProcessMessage(buffer, 0, length);
                                if (packager.HasQueuedMessage(out RemoteControl control, out byte[] message))
                                {
                                    protocol.Process(control, message);
                                }
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