using System;
using System.Text;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using ArcCreate.Remote.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Remote
{
    public class RemoteDataSender : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Toggle showLogToggle;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Marker remoteCurrentTiming;

        [Header("Sync Buttons")]
        [SerializeField] private Button syncAllButton;
        [SerializeField] private Button syncChartButton;
        [SerializeField] private Button syncAudioButton;
        [SerializeField] private Button syncJacketButton;
        [SerializeField] private Button syncBackgroundButton;
        [SerializeField] private Button syncMetadata;
        [SerializeField] private Button syncSettingsButton;

        [Header("Window")]
        [SerializeField] private Window remoteWindow;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button windowSyncChartButton;
        [SerializeField] private Button windowSendTimingToCurrentButton;
        private MessageChannel channel;

        public void SetTarget(MessageChannel channel)
        {
            this.channel = channel;
            canvasGroup.interactable = true;

            StartListeningForEvents();
            SendAll();
            remoteCurrentTiming.gameObject.SetActive(true);
            remoteWindow.gameObject.SetActive(true);
            remoteWindow.IsVisible = true;
        }

        public void RemoveTarget()
        {
            channel = null;
            StopListeningForEvents();
            canvasGroup.interactable = false;
            remoteCurrentTiming.gameObject.SetActive(false);
            remoteWindow.gameObject.SetActive(false);
        }

        private void StartListeningForEvents()
        {
            remoteCurrentTiming.OnEndEdit += SendTiming;
            showLogToggle.onValueChanged.AddListener(SendShowLog);

            syncAllButton.onClick.AddListener(SendAll);
            syncChartButton.onClick.AddListener(SendChart);
            syncAudioButton.onClick.AddListener(SendAudio);
            syncJacketButton.onClick.AddListener(SendJacket);
            syncBackgroundButton.onClick.AddListener(SendBackgrond);
            syncMetadata.onClick.AddListener(SendMetadata);
            syncSettingsButton.onClick.AddListener(SendSettings);

            playButton.onClick.AddListener(SendPlay);
            pauseButton.onClick.AddListener(SendPause);
            windowSyncChartButton.onClick.AddListener(SendChart);
            windowSendTimingToCurrentButton.onClick.AddListener(SendTimingToCurrent);
        }

        private void StopListeningForEvents()
        {
            remoteCurrentTiming.OnEndEdit -= SendTiming;
            showLogToggle.onValueChanged.RemoveListener(SendShowLog);

            syncAllButton.onClick.RemoveListener(SendAll);
            syncChartButton.onClick.RemoveListener(SendChart);
            syncAudioButton.onClick.RemoveListener(SendAudio);
            syncJacketButton.onClick.RemoveListener(SendJacket);
            syncBackgroundButton.onClick.RemoveListener(SendBackgrond);
            syncMetadata.onClick.RemoveListener(SendMetadata);
            syncSettingsButton.onClick.RemoveListener(SendSettings);

            playButton.onClick.RemoveListener(SendPlay);
            pauseButton.onClick.RemoveListener(SendPause);
            windowSyncChartButton.onClick.RemoveListener(SendChart);
            windowSendTimingToCurrentButton.onClick.RemoveListener(SendTimingToCurrent);
        }

        private void OnDestroy()
        {
            StopListeningForEvents();
        }

        private void SendTiming(Marker marker, int timing)
        {
            channel?.SendMessage(RemoteControl.CurrentTiming, FromInt(timing));
        }

        private void SendPause()
        {
            channel?.SendMessage(RemoteControl.Pause, null);
        }

        private void SendPlay()
        {
            channel?.SendMessage(RemoteControl.Play, null);
        }

        private void SendChart()
        {
            channel?.SendMessage(RemoteControl.Chart, null);
        }

        private void SendAudio()
        {
            string audioPath = Services.Project.CurrentChart.AudioPath;
            string ext = System.IO.Path.GetExtension(audioPath);
            channel?.SendMessage(RemoteControl.Audio, FromStringASCII(ext));
        }

        private void SendJacket()
        {
            string jacket = Services.Project.CurrentChart.JacketPath;
            bool useDefault = jacket == null;
            channel?.SendMessage(RemoteControl.JacketArt, FromBool(useDefault));
        }

        private void SendBackgrond()
        {
            string bg = Services.Project.CurrentChart.BackgroundPath;
            bool useDefault = bg == null;
            channel?.SendMessage(RemoteControl.Background, FromBool(useDefault));
        }

        private void SendSpeed()
        {
            int dr = Settings.DropRate.Value;
            channel?.SendMessage(RemoteControl.Speed, FromInt(dr));
        }

        private void SendGlobalOffset()
        {
            int offset = Settings.GlobalAudioOffset.Value;
            channel?.SendMessage(RemoteControl.GlobalOffset, FromInt(offset));
        }

        private void SendShowLog(bool val)
        {
            channel?.SendMessage(RemoteControl.ShowLog, FromBool(val));
        }

        private void SendAll()
        {
            if (channel == null)
            {
                return;
            }

            string audioPath = Services.Project.CurrentChart.AudioPath;
            string ext = System.IO.Path.GetExtension(audioPath);

            string jacket = Services.Project.CurrentChart.JacketPath;
            bool useDefaultJacket = jacket == null;

            string bg = Services.Project.CurrentChart.BackgroundPath;
            bool useDefaultBg = bg == null;

            string chartPath = Services.Project.CurrentChart.ChartPath;

            // terrible
            string msg = $"{ext},{useDefaultJacket},{useDefaultBg},{chartPath}";

            channel.SendMessage(RemoteControl.ReloadAllFiles, FromStringASCII(msg));
            SendSettings();
        }

        private void SendMetadata()
        {
            string chartPath = Services.Project.CurrentChart.ChartPath;
            channel.SendMessage(RemoteControl.Metadata, FromStringASCII(chartPath));
        }

        private void SendSettings()
        {
            SendSpeed();
            SendGlobalOffset();
        }

        private void SendTimingToCurrent()
        {
            channel?.SendMessage(RemoteControl.CurrentTiming, FromInt(Services.Gameplay.Audio.AudioTiming));
        }

        private byte[] FromInt(int val)
        {
            return BitConverter.GetBytes(val);
        }

        private byte[] FromBool(bool val)
        {
            return BitConverter.GetBytes(val);
        }

        private byte[] FromStringASCII(string val)
        {
            return Encoding.ASCII.GetBytes(val);
        }
    }
}