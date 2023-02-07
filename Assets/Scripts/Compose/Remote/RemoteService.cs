using System;
using System.IO;
using System.Net;
using ArcCreate.Compose.Timeline;
using ArcCreate.Remote.Common;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Remote
{
    public class RemoteService : MonoBehaviour, IProtocol, IFileProvider
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

        [Header("Others")]
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private RemoteDataSender remoteDataSender;
        [SerializeField] private Marker remoteCurrentTiming;
        [SerializeField] private RectTransform layout;

        private string code = "------";
        private readonly BroadcastSender broadcastSender = new BroadcastSender(Ports.Gameplay);
        private BroadcastReceiver broadcastReceiver;
        private MessageChannel channel;
        private FileHoster fileHoster;
        private double lastConnectionCheck = double.MaxValue;
        private RemoteState state = RemoteState.Idle;

        private enum RemoteState
        {
            Idle,
            Broadcasting,
            Connected,
            Sending,
        }

        public bool IsConnected => state == RemoteState.Connected;

        public void Process(RemoteControl control, byte[] message)
        {
            switch (control)
            {
                case RemoteControl.CurrentTiming:
                    SetRemoteMarkerTiming(BitConverter.ToInt32(message, 0)).Forget();
                    break;
                case RemoteControl.Abort:
                    OnTargetDisconnect().Forget();
                    break;
            }
        }

        public Stream RespondToFileRequest(string path, out string extension)
        {
            string filePath = string.Empty;
            switch (path)
            {
                case "audio":
                    filePath = Services.Project.CurrentChart.AudioPath;
                    break;
                case "jacket":
                    filePath = Services.Project.CurrentChart.JacketPath;
                    break;
                case "background":
                    filePath = Services.Project.CurrentChart.BackgroundPath;
                    break;
                case "chart":
                    // TODO: flatten the file
                    filePath = Services.Project.CurrentChart.ChartPath;
                    break;
                case "video":
                    // video backgrounds are absolute path
                    filePath = Services.Project.CurrentChart.VideoPath;
                    extension = Path.GetExtension(filePath);
                    return File.OpenRead(filePath);
                default:
                    throw new FileNotFoundException(path);
            }

            extension = Path.GetExtension(filePath);
            string dir = Path.GetDirectoryName(Services.Project.CurrentProject.Path);
            filePath = Path.Combine(dir, filePath);

            return File.OpenRead(filePath);
        }

        private void SetState(RemoteState state)
        {
            this.state = state;

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

            LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
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
            Dispose();
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
            fileHoster = new FileHoster(Ports.HttpCompose, this);
            await channel.Setup();
            while (!fileHoster.IsRunning)
            {
                await UniTask.NextFrame();
            }

            lastConnectionCheck = Time.realtimeSinceStartup;
            remoteDataSender.SetTarget(channel);

            SetState(RemoteState.Connected);
            Debug.Log($"Compose: Started session with {ipAddress}:{Ports.Gameplay} {code}");
        }

        private void StopSession()
        {
            Dispose();
            remoteDataSender.RemoveTarget();
            SetState(RemoteState.Idle);

            lastConnectionCheck = double.MaxValue;
            Debug.Log($"Compose: Stopped session");
        }

        private void Dispose()
        {
            channel?.SendMessage(RemoteControl.Abort, System.Text.Encoding.ASCII.GetBytes("From Compose"));
            broadcastSender.Broadcast(Constants.Abort);
            DisposeAny();
        }

        private async UniTask OnTargetDisconnect()
        {
            await UniTask.SwitchToMainThread();
            DisposeAny();
            SetState(RemoteState.Idle);
            broadcastSender.Broadcast(Constants.Abort);
            remoteDataSender.RemoveTarget();

            lastConnectionCheck = double.MaxValue;
            Debug.Log($"Compose: Target disconnected");
            Services.Popups.Notify(Popups.Severity.Info, I18n.S("Remote.State.TargetDisconnected.Compose"));
        }

        private string GenerateRandomMessage()
        {
            int code = UnityEngine.Random.Range(0, 999999);
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

            if (fileHoster?.IsRunning ?? false)
            {
                fileHoster.Dispose();
                fileHoster = null;
            }
        }

        private async UniTask SetRemoteMarkerTiming(int timing)
        {
            await UniTask.SwitchToMainThread();
            remoteCurrentTiming.SetTiming(timing);
        }
    }
}