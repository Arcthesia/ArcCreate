using System;
using System.Collections.Generic;
using System.Net;
using ArcCreate.Gameplay;
using ArcCreate.Remote.Common;
using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcCreate.Remote.Gameplay
{
    public class RemoteManager : SceneRepresentative, IProtocol
    {
        [SerializeField] private GameObject selectDevicePrefab;
        [SerializeField] private Transform selectDeviceParent;
        [SerializeField] private GameObject indicateListening;
        [SerializeField] private TMP_Text statusText;
        private readonly List<RemoteDeviceRow> rows = new List<RemoteDeviceRow>();

        private IGameplayControl gameplay;
        private RemoteSessionGameplay remoteSession;
        private BroadcastReceiver broadcastReceiver;
        private MessageChannel channel;
        private readonly BroadcastSender broadcastSender = new BroadcastSender(Ports.Compose);

        public void ConnectTo(IPAddress ipAddress, string code)
        {
            StartConnectionTo(ipAddress, code).Forget();
        }

        public void Process(byte[] data)
        {
        }

        public override void OnNoBootScene()
        {
            // LoadGameplayScene();
        }

        public override void OnUnloadScene()
        {
        }

        protected override void OnSceneLoad()
        {
            StartListeningForBroadcast();
        }

        private void LoadGameplayScene()
        {
            if (SceneTransitionManager.Instance == null)
            {
                return;
            }

            SceneTransitionManager.Instance.LoadSceneAdditive(
                SceneNames.GameplayScene,
                rep =>
                {
                    var gameplayControl = rep as IGameplayControl;
                    gameplay = gameplayControl ?? throw new Exception("Could not load gameplay scene");
                    gameplayControl.ShouldUpdateInputSystem = true;
                    gameplayControl.Chart.EnableColliderGeneration = false;

                    StartListeningForBroadcast();

                    Debug.Log(I18n.S("Compose.Notify.GameplayLoaded"));
                }).Forget();
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

            rows.Clear();
            Debug.Log($"Gameplay: Stopped listening");
        }

        private async UniTask StartConnectionTo(IPAddress ipAddress, string code)
        {
            indicateListening.SetActive(false);
            broadcastSender.Broadcast(code);
            channel = new MessageChannel(ipAddress, Ports.Compose, this);
            await channel.Setup();
            StopListeningForBroadcast();

            Debug.Log($"Gameplay: Connected to {ipAddress}:{Ports.Compose} {code}");
        }

        private void StopSession()
        {
            remoteSession?.Dispose();
            Debug.Log("Gameplay: Stopped session");
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
                    if (message == Strings.Abort)
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
    }
}