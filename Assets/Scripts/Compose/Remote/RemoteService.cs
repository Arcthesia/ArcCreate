using System.Net;
using ArcCreate.Remote.Common;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Remote
{
    public class RemoteService : MonoBehaviour, IProtocol
    {
        [Header("Icons")]
        [SerializeField] private GameObject idleIndicator;
        [SerializeField] private GameObject broadcastingIndicator;
        [SerializeField] private GameObject connectedIndicator;
        [SerializeField] private GameObject sendingIndicator;

        [Header("Buttons")]
        [SerializeField] private Button startBroadcastButton;
        [SerializeField] private Button abortBroadcastButton;
        [SerializeField] private Button broadcastAgainButton;
        [SerializeField] private Button stopSessionButton;

        [Header("Text")]
        [SerializeField] private TMP_Text descriptionText;

        private string code = "------";
        private readonly BroadcastSender broadcastSender = new BroadcastSender(Ports.Gameplay);
        private BroadcastReceiver broadcastReceiver;
        private MessageChannel channel;
        private double lastConnectionCheck = double.MaxValue;

        private enum RemoteState
        {
            Idle,
            Broadcasting,
            Connected,
            Sending,
        }

        public bool IsConnected { get; private set; }

        public void Process(RemoteControl control, byte[] message)
        {
            print("Compose received: " + control + " - " + System.Text.Encoding.ASCII.GetString(message));
            switch (control)
            {
                case RemoteControl.Abort:
                    OnTargetDisconnect().Forget();
                    break;
            }
        }

        private void SetState(RemoteState state)
        {
            IsConnected = state == RemoteState.Connected;

            idleIndicator.SetActive(state == RemoteState.Idle);
            broadcastingIndicator.SetActive(state == RemoteState.Broadcasting);
            connectedIndicator.SetActive(state == RemoteState.Connected);
            sendingIndicator.SetActive(state == RemoteState.Sending);

            startBroadcastButton.gameObject.SetActive(state == RemoteState.Idle);
            abortBroadcastButton.gameObject.SetActive(state == RemoteState.Broadcasting);
            broadcastAgainButton.gameObject.SetActive(state == RemoteState.Broadcasting);
            stopSessionButton.gameObject.SetActive(state == RemoteState.Connected);

            switch (state)
            {
                case RemoteState.Idle:
                    descriptionText.text = I18n.S("Remote.Description.Idle");
                    break;
                case RemoteState.Broadcasting:
                    descriptionText.text = I18n.S("Remote.Description.Broadcasting", code);
                    break;
                case RemoteState.Connected:
                case RemoteState.Sending:
                    descriptionText.text = I18n.S("Remote.Description.Connected", channel.IPAddress, channel.SendToPort);
                    break;
            }
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup > lastConnectionCheck + Constants.Timeout)
            {
                lastConnectionCheck = Time.realtimeSinceStartup;
                if (channel != null && !channel.CheckConnection(System.Text.Encoding.ASCII.GetBytes("From Compose")))
                {
                    OnTargetDisconnect().Forget();
                }
            }
        }

        private void Awake()
        {
            startBroadcastButton.onClick.AddListener(StartBroadcast);
            abortBroadcastButton.onClick.AddListener(StopSession);
            broadcastAgainButton.onClick.AddListener(BroadcastAgain);
            stopSessionButton.onClick.AddListener(StopSession);
            SetState(RemoteState.Idle);
        }

        private void OnDestroy()
        {
            startBroadcastButton.onClick.RemoveListener(StartBroadcast);
            abortBroadcastButton.onClick.RemoveListener(StopSession);
            broadcastAgainButton.onClick.RemoveListener(BroadcastAgain);
            stopSessionButton.onClick.RemoveListener(StopSession);
            StopSession();
        }

        private void StartBroadcast()
        {
            code = GenerateRandomMessage();
            broadcastSender.Broadcast(code);

            DisposeAny();
            SetState(RemoteState.Broadcasting);

            broadcastReceiver = new BroadcastReceiver(Ports.Compose);
            broadcastReceiver.OnBroadcastReceived += OnBroadcastReceived;

            Debug.Log($"Compose: Sent broadcast to port {Ports.Gameplay} {code}");
            Debug.Log($"Compose: Start listening on port {Ports.Compose}");
        }

        private void BroadcastAgain()
        {
            code = GenerateRandomMessage();
            broadcastSender.Broadcast(code);
            Debug.Log($"Compose: Sent broadcast to port {Ports.Gameplay} {code}");
            SetState(RemoteState.Broadcasting);
        }

        private async UniTask StartSession(IPAddress ipAddress)
        {
            DisposeAny();
            channel = new MessageChannel(ipAddress, Ports.Compose, Ports.Gameplay, this);
            await channel.Setup();
            lastConnectionCheck = Time.realtimeSinceStartup;

            SetState(RemoteState.Connected);
            Debug.Log($"Compose: Started session with {ipAddress}:{Ports.Gameplay} {code}");
        }

        private void StopSession()
        {
            channel?.SendMessage(RemoteControl.Abort, System.Text.Encoding.ASCII.GetBytes("From Compose"));
            DisposeAny();
            SetState(RemoteState.Idle);
            broadcastSender.Broadcast(Constants.Abort);

            lastConnectionCheck = double.MaxValue;
            Debug.Log($"Compose: Stopped session");
        }

        private async UniTask OnTargetDisconnect()
        {
            await UniTask.SwitchToMainThread();
            DisposeAny();
            SetState(RemoteState.Idle);
            broadcastSender.Broadcast(Constants.Abort);

            lastConnectionCheck = double.MaxValue;
            Debug.Log($"Compose: Target disconnected");
            Services.Popups.Notify(Popups.Severity.Info, I18n.S("Remote.State.TargetDisconnected.Compose"));
        }

        private string GenerateRandomMessage()
        {
            int code = Random.Range(0, 999999);
            return code.ToString("D6");
        }

        private void OnBroadcastReceived(IPAddress ipAddress, string message)
        {
            if (message == code)
            {
                StartSession(ipAddress).Forget();
            }
        }

        private void DisposeAny()
        {
            if (broadcastReceiver?.IsRunning ?? false)
            {
                broadcastReceiver.Dispose();
                broadcastReceiver.OnBroadcastReceived -= OnBroadcastReceived;
                broadcastReceiver = null;
            }

            if (channel?.IsRunning ?? false)
            {
                channel.Dispose();
                channel = null;
            }
        }
    }
}