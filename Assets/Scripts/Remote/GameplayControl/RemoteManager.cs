using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using ArcCreate.Gameplay;
using ArcCreate.Remote.Common;
using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ArcCreate.Remote.Gameplay
{
    public class RemoteManager : SceneRepresentative, IProtocol
    {
        [SerializeField] private GameObject selectDevicePrefab;
        [SerializeField] private Transform selectDeviceParent;
        [SerializeField] private GameObject indicateListening;
        [SerializeField] private Button startNewSessionButton;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text logText;
        [SerializeField] private Color warningColor;
        [SerializeField] private Color errorColor;
        [SerializeField] private Color normalColor;
        [SerializeField] private RemoteGameplayControl remoteGameplayControl;
        private readonly List<RemoteDeviceRow> rows = new List<RemoteDeviceRow>();

        private IGameplayControl gameplay;
        private BroadcastReceiver broadcastReceiver;
        private MessageChannel channel;
        private readonly BroadcastSender broadcastSender = new BroadcastSender(Ports.Compose);
        private readonly CancellationTokenSource ctx = new CancellationTokenSource();

        public void Process(RemoteControl control, byte[] message)
        {
            switch (control)
            {
                case RemoteControl.Abort:
                    OnTargetDisconnect().Forget();
                    break;
                default:
                    remoteGameplayControl.HandleMessage(control, message).Forget();
                    break;
            }
        }

        public void ConnectTo(IPAddress ipAddress, string code)
        {
            StartConnectionTo(ipAddress, code).Forget();
        }

        public override void OnUnloadScene()
        {
            startNewSessionButton.onClick.RemoveListener(OnStartNewSessionButton);
            Application.logMessageReceived -= DisplayLog;
        }

        protected override void OnSceneLoad()
        {
            startNewSessionButton.onClick.AddListener(OnStartNewSessionButton);
            startNewSessionButton.gameObject.SetActive(false);
            selectDeviceParent.gameObject.SetActive(true);
            Application.logMessageReceived += DisplayLog;
            LoadGameplayScene();
        }

        private void OnStartNewSessionButton()
        {
            StartListeningForBroadcast();
            startNewSessionButton.gameObject.SetActive(false);
            selectDeviceParent.gameObject.SetActive(true);
        }

        private void LoadGameplayScene()
        {
            if (SceneTransitionManager.Instance == null)
            {
                return;
            }

            var gameplayManager = FindObjectOfType<GameplayManager>();
            if (gameplayManager != null)
            {
                UseGameplay(gameplayManager);
                return;
            }

            SceneTransitionManager.Instance.LoadSceneAdditive(
                SceneNames.GameplayScene,
                rep =>
                {
                    var gameplayControl = rep as IGameplayControl;
                    gameplay = gameplayControl ?? throw new Exception("Could not load gameplay scene");
                    UseGameplay(gameplayControl);
                }).Forget();
        }

        private void UseGameplay(IGameplayControl gameplay)
        {
            gameplay.ShouldUpdateInputSystem = true;
            gameplay.Chart.EnableColliderGeneration = false;
            remoteGameplayControl.SetGameplay(gameplay);

            Debug.Log(I18n.S("Compose.Notify.GameplayLoaded"));

            StartListeningForBroadcast();
        }

        private void Update()
        {
            if (gameplay == null || !gameplay.IsLoaded)
            {
                InputSystem.Update();
            }
        }

        private void OnDestroy()
        {
            StopListeningForBroadcast();
            StopSession();
        }

        private void StartListeningForBroadcast()
        {
            broadcastReceiver = new BroadcastReceiver(Ports.Gameplay);
            broadcastReceiver.OnBroadcastReceived += OnBroadcastReceived;
            indicateListening.SetActive(true);

            foreach (var row in rows)
            {
                Destroy(row.gameObject);
            }

            rows.Clear();

            UpdateStatusText();
            Debug.Log($"Gameplay: Start listening on port {Ports.Gameplay}");

            if (Application.platform == RuntimePlatform.Android)
            {
                AcquireMulticastLockPeriodically(ctx.Token).Forget();
            }
        }

        private void StopListeningForBroadcast()
        {
            if (broadcastReceiver?.IsRunning ?? false)
            {
                broadcastReceiver.Dispose();
            }

            indicateListening.SetActive(false);

            foreach (var row in rows)
            {
                Destroy(row.gameObject);
            }

            ctx.Cancel();
            rows.Clear();
            Debug.Log($"Gameplay: Stopped listening");
        }

        // https://answers.unity.com/questions/250732/android-build-is-not-receiving-udp-broadcasts.html
        private async UniTask AcquireMulticastLockPeriodically(CancellationToken ctx)
        {
            string lockTag = "remotePlay";

            try
            {
                using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
                    {
                        AndroidJavaObject multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", lockTag);
                        multicastLock.Call("acquire");

                        while (!ctx.IsCancellationRequested)
                        {
                            bool isHeld = multicastLock.Call<bool>("isHeld");
                            if (!isHeld)
                            {
                                multicastLock.Call("acquire");
                            }

                            if (!multicastLock.Call<bool>("isHeld"))
                            {
                                Debug.LogError("Could not acquire multicast lock");
                            }

                            await UniTask.Delay(Constants.AcquireMulticastLockInterval, cancellationToken: ctx);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception err)
            {
                Debug.LogError(err.ToString());
            }
        }

        private async UniTask StartConnectionTo(IPAddress ipAddress, string code)
        {
            indicateListening.SetActive(false);
            broadcastSender.Broadcast(code);

            channel = new MessageChannel(this);
            channel.OnError += OnChannelError;
            channel.SetupListener(Ports.Gameplay);
            await channel.SetupSender(ipAddress, Ports.Compose, code);

            remoteGameplayControl.SetTarget(ipAddress, Ports.HttpCompose, channel);
            StopListeningForBroadcast();

            Debug.Log($"Gameplay: Connected to {ipAddress}:{Ports.Compose} {code}");
        }

        private void StopSession()
        {
            channel?.SendMessage(RemoteControl.Abort, System.Text.Encoding.ASCII.GetBytes("From Gameplay"));
            channel?.Dispose();
            channel.OnError -= OnChannelError;
            channel = null;
            Debug.Log("Gameplay: Stopped session");
        }

        private async UniTask OnTargetDisconnect()
        {
            await UniTask.SwitchToMainThread();
            channel?.Dispose();
            channel.OnError -= OnChannelError;
            channel = null;
            Debug.Log($"Gameplay: Target disconnected");

            statusText.text = I18n.S("Remote.State.TargetDisconnected.Gameplay");
            indicateListening.SetActive(true);
            startNewSessionButton.gameObject.SetActive(true);
            selectDeviceParent.gameObject.SetActive(false);
            gameplay.Audio.Pause();
        }

        private void OnChannelError()
        {
            OnTargetDisconnect().Forget();
        }

        private void OnBroadcastReceived(IPAddress ipAddress, string message)
        {
            OnBroadcastReceivedMainThread(ipAddress, message).Forget();
        }

        private async UniTask OnBroadcastReceivedMainThread(IPAddress ipAddress, string message)
        {
            await UniTask.SwitchToMainThread();
            foreach (var row in rows)
            {
                if (row.IPAddress.Equals(ipAddress))
                {
                    if (message == Constants.Abort)
                    {
                        Destroy(row.gameObject);
                        rows.Remove(row);
                        Debug.Log($"Gameplay: Unregister device {ipAddress}");
                    }
                    else
                    {
                        row.SetMessage(message);
                        Debug.Log($"Gameplay: Update device {ipAddress} {message}");
                    }

                    UpdateStatusText();
                    return;
                }
            }

            if (message != Constants.Abort)
            {
                GameObject go = Instantiate(selectDevicePrefab, selectDeviceParent);
                var row = go.GetComponent<RemoteDeviceRow>();
                row.Setup(ipAddress, message, this);
                rows.Add(row);
                UpdateStatusText();
                Debug.Log($"Gameplay: Register new device {ipAddress} {message}");
            }
        }

        private void UpdateStatusText()
        {
            if (rows.Count > 0)
            {
                statusText.text = I18n.S("Remote.State.Found");
                return;
            }

            statusText.text = I18n.S("Remote.State.Listening");
        }

        private void DisplayLog(string condition, string stackTrace, LogType type)
        {
            logText.text = condition;
            switch (type)
            {
                case LogType.Warning:
                    logText.color = warningColor;
                    break;
                case LogType.Error:
                case LogType.Exception:
                    logText.color = errorColor;
                    break;
                default:
                    logText.color = normalColor;
                    break;
            }
        }
    }
}