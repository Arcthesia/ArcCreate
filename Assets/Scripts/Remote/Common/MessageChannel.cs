using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        private int listenOnPort;
        private int sendToPort;
        private IPAddress ip;
        private readonly IProtocol protocol;
        private readonly MessagePackager packager;
        private bool waitingForConnection = false;
        private string code = string.Empty;
        private Action<IPAddress> onConnected;

        public MessageChannel(IProtocol protocol)
        {
            this.protocol = protocol;
            packager = new MessagePackager();
        }

        public event Action OnError;

        public bool IsRunning { get; private set; }

        public IPAddress IPAddress => ip;

        public int SendToPort => sendToPort;

        public void SetupListener(int listenOnPort)
        {
            this.listenOnPort = listenOnPort;
            if (IsRunning)
            {
                throw new InvalidOperationException("Message channel is already running");
            }

            listenerThread = new Thread(new ThreadStart(ListenForData))
            {
                IsBackground = true,
            };

            listenerThread.Start();
        }

        public async UniTask SetupSender(IPAddress ip, int sendToPort, string code, int timeoutMs = 10000)
        {
            this.ip = ip;
            this.sendToPort = sendToPort;

            Thread connectThread = new Thread(new ThreadStart(TryConnectToTcpServer));
            connectThread.Start();
            var timeout = UniTask.Delay(timeoutMs);

            while (targetClient == null && timeout.Status == UniTaskStatus.Pending)
            {
                await UniTask.NextFrame();
            }

            connectThread.Abort();

            if (targetClient == null)
            {
                throw new Exception($"Could not connect to target client {ip}:{sendToPort}");
            }

            SendMessage(RemoteControl.StartConnection, code == null ? null : Encoding.ASCII.GetBytes(code));
            IsRunning = true;
        }

        public void WaitForConnection(string code, Action<IPAddress> onConnected)
        {
            waitingForConnection = true;
            this.code = code;
            this.onConnected = onConnected;
        }

        public void UpdateCode(string code)
        {
            this.code = code;
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
                Debug.LogError("Socket exception: " + listenOnPort + " " + socketException);
            }
        }

        public void Dispose()
        {
            IsRunning = false;
            targetClient?.Close();
            targetClient?.Dispose();
            listener.Stop();
            listenerThread.Abort();
        }

        private void TryConnectToTcpServer()
        {
            while (targetClient == null)
            {
                try
                {
                    targetClient = new TcpClient(ip.ToString(), sendToPort);
                }
                catch (Exception e)
                {
                    Debug.LogError("On client connect exception " + ip.ToString() + ":" + sendToPort + " " + e);
                    targetClient = null;
                }

                Thread.Sleep(1000);
            }
        }

        private void ListenForData()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, listenOnPort);
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
                                while (packager.HasQueuedMessage(out RemoteControl control, out byte[] message))
                                {
                                    if (control == RemoteControl.StartConnection && waitingForConnection && (code == null || code == Encoding.ASCII.GetString(message)))
                                    {
                                        ip = ((IPEndPoint)connectedClient.Client.RemoteEndPoint).Address;
                                        onConnected?.Invoke(ip);
                                    }

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
                OnError?.Invoke();
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