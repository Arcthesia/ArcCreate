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

        private enum RemoteState
        {
            Idle,
            Broadcasting,
            Connected,
            Sending,
        }

        public void Process(byte[] data)
        {
        }

        private void SetState(RemoteState state)
        {
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
                    descriptionText.text = I18n.S("Remote.Description.Connected", channel.IPAddress, channel.Port);
                    break;
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
            channel = new MessageChannel(ipAddress, Ports.Gameplay, this);
            await channel.Setup();

            SetState(RemoteState.Connected);
            Debug.Log($"Compose: Started session with {ipAddress}:{Ports.Gameplay} {code}");
        }

        private void StopSession()
        {
            DisposeAny();
            SetState(RemoteState.Idle);
            broadcastSender.Broadcast(Strings.Abort);
            Debug.Log($"Compose: Stopped session");
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