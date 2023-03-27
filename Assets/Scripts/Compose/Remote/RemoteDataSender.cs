using System;
using System.IO;
using System.Text;
using ArcCreate.ChartFormat;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.Project;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using ArcCreate.Remote.Common;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Remote
{
    public class RemoteDataSender : MonoBehaviour, IFileProvider
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Toggle showLogToggle;
        [SerializeField] private Toggle showDebugToggle;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Marker remoteCurrentTiming;

        [Header("Sync Buttons")]
        [SerializeField] private Button syncAllButton;
        [SerializeField] private Button syncChartButton;
        [SerializeField] private Button syncAudioButton;
        [SerializeField] private Button syncJacketButton;
        [SerializeField] private Button syncBackgroundButton;
        [SerializeField] private Button syncMetadata;

        [Header("Window")]
        [SerializeField] private Window remoteWindow;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button windowSyncChartButton;
        [SerializeField] private Button windowSendTimingToCurrentButton;
        private MessageChannel channel;

        public Stream RespondToFileRequest(string path, out string extension)
        {
            string filePath = string.Empty;
            string dir = Path.GetDirectoryName(Services.Project.CurrentProject.Path);

            if (path.StartsWith("scenecontrol/"))
            {
                path = path.Substring("scenecontrol/".Length);
                filePath = Path.Combine(Values.ScenecontrolFolder, path);
                extension = Path.GetExtension(filePath);
            }
            else if (path.StartsWith("sfx/"))
            {
                path = path.Substring("sfx/".Length);
                filePath = Path.Combine(Path.GetDirectoryName(Services.Project.CurrentChart.ChartPath), path);
                extension = Path.GetExtension(filePath);
            }
            else
            {
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
                        filePath = "remote.aff";
                        new ChartSerializer(new PhysicalFileAccess(), dir).WriteSingleFile(
                            filePath,
                            gameplayData.AudioOffset.Value,
                            gameplayData.TimingPointDensityFactor.Value,
                            new RawEventsBuilder().GetEvents());
                        break;
                    case "scjson":
                        filePath = "remote.sc.json";
                        string json = Services.Gameplay.Scenecontrol.Export();
                        File.WriteAllText(Path.Combine(dir, filePath), json);
                        break;
                    case "video":
                        // video backgrounds are absolute path
                        filePath = Services.Project.CurrentChart.VideoPath;
                        extension = Path.GetExtension(filePath);
                        return File.OpenRead(filePath);
                    case "metadata":
                        filePath = Services.Project.CurrentProject.Path;
                        extension = Path.GetExtension(filePath);
                        break;
                    default:
                        throw new FileNotFoundException(path);
                }
            }

            extension = Path.GetExtension(filePath);
            filePath = Path.Combine(dir, filePath);

            return File.OpenRead(filePath);
        }

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
            showDebugToggle.onValueChanged.AddListener(SendShowDebug);

            syncAllButton.onClick.AddListener(SendAll);
            syncChartButton.onClick.AddListener(SendChart);
            syncAudioButton.onClick.AddListener(SendAudio);
            syncJacketButton.onClick.AddListener(SendJacket);
            syncBackgroundButton.onClick.AddListener(SendBackgrond);
            syncMetadata.onClick.AddListener(SendMetadata);

            playButton.onClick.AddListener(SendPlay);
            pauseButton.onClick.AddListener(SendPause);
            windowSyncChartButton.onClick.AddListener(SendChart);
            windowSendTimingToCurrentButton.onClick.AddListener(SendTimingToCurrent);
        }

        private void StopListeningForEvents()
        {
            remoteCurrentTiming.OnEndEdit -= SendTiming;
            showLogToggle.onValueChanged.RemoveListener(SendShowLog);
            showDebugToggle.onValueChanged.RemoveListener(SendShowDebug);

            syncAllButton.onClick.RemoveListener(SendAll);
            syncChartButton.onClick.RemoveListener(SendChart);
            syncAudioButton.onClick.RemoveListener(SendAudio);
            syncJacketButton.onClick.RemoveListener(SendJacket);
            syncBackgroundButton.onClick.RemoveListener(SendBackgrond);
            syncMetadata.onClick.RemoveListener(SendMetadata);

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
            channel?.SendMessage(RemoteControl.Scenecontrol, null);
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
            string dir = Path.GetDirectoryName(Services.Project.CurrentProject.Path);
            bool useDefault = jacket == null || !File.Exists(Path.Combine(dir, jacket));
            channel?.SendMessage(RemoteControl.JacketArt, FromBool(useDefault));
        }

        private void SendBackgrond()
        {
            string bg = Services.Project.CurrentChart.BackgroundPath;
            string dir = Path.GetDirectoryName(Services.Project.CurrentProject.Path);
            bool useDefault = bg == null || !File.Exists(Path.Combine(dir, bg));
            channel?.SendMessage(RemoteControl.Background, FromBool(useDefault));
        }

        private void SendShowLog(bool val)
        {
            channel?.SendMessage(RemoteControl.ShowLog, FromBool(val));
        }

        private void SendShowDebug(bool val)
        {
            channel?.SendMessage(RemoteControl.ShowDebug, FromBool(val));
        }

        private void SendAll()
        {
            if (channel == null)
            {
                return;
            }

            string dir = Path.GetDirectoryName(Services.Project.CurrentProject.Path);

            string audioPath = Services.Project.CurrentChart.AudioPath;
            string ext = System.IO.Path.GetExtension(audioPath);

            string jacket = Services.Project.CurrentChart.JacketPath;
            bool useDefaultJacket = jacket == null || !File.Exists(Path.Combine(dir, jacket));

            string bg = Services.Project.CurrentChart.BackgroundPath;
            bool useDefaultBg = bg == null || !File.Exists(Path.Combine(dir, bg));

            string chartPath = Services.Project.CurrentChart.ChartPath;

            // terrible
            string msg = $"{ext},{useDefaultJacket},{useDefaultBg},{chartPath}";

            channel.SendMessage(RemoteControl.ReloadAllFiles, FromStringASCII(msg));
        }

        private void SendMetadata()
        {
            string chartPath = Services.Project.CurrentChart.ChartPath;
            channel.SendMessage(RemoteControl.Metadata, FromStringASCII(chartPath));
        }

        private void SendTimingToCurrent()
        {
            channel?.SendMessage(RemoteControl.CurrentTiming, FromInt(Services.Gameplay.Audio.AudioTiming));
        }

        private byte[] FromInt(int val)
        {
            return BitConverter.GetBytes(val);
        }

        private byte[] FromFloat(float val)
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