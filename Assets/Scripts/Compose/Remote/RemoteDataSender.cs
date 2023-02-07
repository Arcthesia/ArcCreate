using System;
using System.Text;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using ArcCreate.Remote.Common;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Remote
{
    public class RemoteDataSender : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Toggle loopToggle;
        [SerializeField] private Toggle showLogToggle;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Marker remoteCurrentTiming;
        [SerializeField] private MarkerRange remoteTimingRange;
        [SerializeField] private TMP_InputField fromTimingField;
        [SerializeField] private TMP_InputField toTimingField;

        [Header("Sync Buttons")]
        [SerializeField] private Button syncAllButton;
        [SerializeField] private Button syncChartButton;
        [SerializeField] private Button syncAudioButton;
        [SerializeField] private Button syncJacketButton;
        [SerializeField] private Button syncBackgroundButton;
        [SerializeField] private Button syncInformationButton;
        [SerializeField] private Button syncVideoBackgroundButton;
        [SerializeField] private Button syncSettingsButton;
        [SerializeField] private Button syncSkinButton;

        [Header("Window")]
        [SerializeField] private Window remoteWindow;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button windowSyncChartButton;
        private MessageChannel channel;
        private bool isRemotePlaying;
        private int remoteLoopStart;
        private int remoteLoopEnd;
        private bool isClientLooping;

        private int AudioLength
            => Mathf.RoundToInt(gameplayData.AudioClip.Value.length * 1000);

        public void SetTarget(MessageChannel channel)
        {
            this.channel = channel;
            canvasGroup.interactable = true;

            fromTimingField.SetTextWithoutNotify("0");
            toTimingField.SetTextWithoutNotify(AudioLength.ToString());
            remoteTimingRange.SetTiming(0, AudioLength);
            remoteLoopStart = 0;
            remoteLoopEnd = AudioLength;
            isClientLooping = loopToggle.isOn;

            StartListeningForEvents();
            SendAll();
            isRemotePlaying = false;
            remoteCurrentTiming.gameObject.SetActive(true);
            remoteTimingRange.gameObject.SetActive(true);
            remoteWindow.gameObject.SetActive(true);
            remoteWindow.IsVisible = true;
        }

        public void RemoveTarget()
        {
            channel = null;
            StopListeningForEvents();
            isRemotePlaying = false;
            canvasGroup.interactable = false;
            remoteCurrentTiming.gameObject.SetActive(false);
            remoteTimingRange.gameObject.SetActive(false);
            remoteWindow.gameObject.SetActive(false);
        }

        private void StartListeningForEvents()
        {
            remoteCurrentTiming.OnDragDebounced += SendTiming;
            remoteTimingRange.OnDragDebounced += OnTimingRange;
            loopToggle.onValueChanged.AddListener(SendLoop);
            showLogToggle.onValueChanged.AddListener(SendShowLog);

            fromTimingField.onEndEdit.AddListener(OnRangeField);
            toTimingField.onEndEdit.AddListener(OnRangeField);

            syncSettingsButton.onClick.AddListener(SendAll);
            syncChartButton.onClick.AddListener(SendChart);
            syncAudioButton.onClick.AddListener(SendAudio);
            syncJacketButton.onClick.AddListener(SendJacket);
            syncBackgroundButton.onClick.AddListener(SendBackgrond);
            syncInformationButton.onClick.AddListener(SendInformation);
            syncVideoBackgroundButton.onClick.AddListener(SendVideoBackgrond);
            syncSettingsButton.onClick.AddListener(SendSettings);
            syncSkinButton.onClick.AddListener(SendSkin);

            playButton.onClick.AddListener(SendPlay);
            pauseButton.onClick.AddListener(SendPause);
            windowSyncChartButton.onClick.AddListener(SendChart);
        }

        private void StopListeningForEvents()
        {
            remoteCurrentTiming.OnDragDebounced -= SendTiming;
            remoteTimingRange.OnDragDebounced -= OnTimingRange;
            loopToggle.onValueChanged.RemoveListener(SendLoop);
            showLogToggle.onValueChanged.RemoveListener(SendShowLog);

            fromTimingField.onEndEdit.RemoveListener(OnRangeField);
            toTimingField.onEndEdit.RemoveListener(OnRangeField);

            syncSettingsButton.onClick.RemoveListener(SendAll);
            syncChartButton.onClick.RemoveListener(SendChart);
            syncAudioButton.onClick.RemoveListener(SendAudio);
            syncJacketButton.onClick.RemoveListener(SendJacket);
            syncBackgroundButton.onClick.RemoveListener(SendBackgrond);
            syncInformationButton.onClick.RemoveListener(SendInformation);
            syncVideoBackgroundButton.onClick.RemoveListener(SendVideoBackgrond);
            syncSettingsButton.onClick.RemoveListener(SendSettings);
            syncSkinButton.onClick.RemoveListener(SendSkin);

            playButton.onClick.RemoveListener(SendPlay);
            pauseButton.onClick.RemoveListener(SendPause);
            windowSyncChartButton.onClick.RemoveListener(SendChart);
        }

        private void OnDestroy()
        {
            StopListeningForEvents();
        }

        private void SendTiming(Marker marker, int timing)
        {
            channel?.SendMessage(RemoteControl.CurrentTiming, FromInt(timing));
        }

        private void SendTimingRange(int from, int to)
        {
            remoteLoopStart = from;
            remoteLoopEnd = to;
            AlignTimingRange();
            channel?.SendMessage(RemoteControl.StartTiming, FromInt(from));
            channel?.SendMessage(RemoteControl.EndTiming, FromInt(to));
        }

        private void SendLoop(bool loop)
        {
            isClientLooping = loop;
            channel?.SendMessage(RemoteControl.Loop, FromBool(loop));
        }

        private void SendPause()
        {
            channel?.SendMessage(RemoteControl.Pause, null);
            isRemotePlaying = false;
        }

        private void SendPlay()
        {
            channel?.SendMessage(RemoteControl.Play, null);
            isRemotePlaying = true;
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

        private void SendVideoBackgrond()
        {
            bool enabled = !string.IsNullOrEmpty(Services.Project.CurrentChart.VideoPath);
            channel?.SendMessage(RemoteControl.VideoBackground, FromBool(enabled));
        }

        private void SendTitle()
        {
            string title = Services.Project.CurrentChart.Title;
            channel?.SendMessage(RemoteControl.Title, FromStringUTF32(title));
        }

        private void SendComposer()
        {
            string composer = Services.Project.CurrentChart.Composer;
            channel?.SendMessage(RemoteControl.Composer, FromStringUTF32(composer));
        }

        private void SendDifficultyName()
        {
            string diff = Services.Project.CurrentChart.Difficulty;
            channel?.SendMessage(RemoteControl.DifficultyName, FromStringUTF32(diff));
        }

        private void SendDifficultyColor()
        {
            string c = Services.Project.CurrentChart.DifficultyColor;
            channel?.SendMessage(RemoteControl.DifficultyColor, FromStringASCII(c));
        }

        private void SendBaseBpm()
        {
            float bpm = gameplayData.BaseBpm.Value;
            channel?.SendMessage(RemoteControl.BaseBpm, FromFloat(bpm));
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

        private void SendAlignmentSkin()
        {
            string skin = Services.Project.CurrentChart.Skin?.Side ?? string.Empty;
            channel?.SendMessage(RemoteControl.AlignmentSkin, FromStringASCII(skin));
        }

        private void SendAccentSkin()
        {
            string skin = Services.Project.CurrentChart.Skin?.Accent ?? string.Empty;
            channel?.SendMessage(RemoteControl.AccentSkin, FromStringASCII(skin));
        }

        private void SendNoteSkin()
        {
            string skin = Services.Project.CurrentChart.Skin?.Note ?? string.Empty;
            channel?.SendMessage(RemoteControl.NoteSkin, FromStringASCII(skin));
        }

        private void SendParticleSkin()
        {
            string skin = Services.Project.CurrentChart.Skin?.Particle ?? string.Empty;
            channel?.SendMessage(RemoteControl.ParticleSkin, FromStringASCII(skin));
        }

        private void SendSingleLineSkin()
        {
            string skin = Services.Project.CurrentChart.Skin?.SingleLine ?? string.Empty;
            channel?.SendMessage(RemoteControl.SingleLineSkin, FromStringASCII(skin));
        }

        private void SendTrackSkin()
        {
            string skin = Services.Project.CurrentChart.Skin?.Track ?? string.Empty;
            channel?.SendMessage(RemoteControl.TrackSkin, FromStringASCII(skin));
        }

        private void SendShowLog(bool val)
        {
            channel?.SendMessage(RemoteControl.ShowLog, FromBool(val));
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

        private byte[] FromStringUTF32(string val)
        {
            return Encoding.UTF32.GetBytes(val);
        }

        private byte[] FromFloat(float val)
        {
            return BitConverter.GetBytes(val);
        }

        private void SendAll()
        {
            if (channel == null)
            {
                return;
            }

            SendTiming(remoteCurrentTiming, remoteCurrentTiming.Timing);
            SendTimingRange(remoteTimingRange.Timing, remoteTimingRange.EndTiming);
            SendLoop(loopToggle.isOn);
            SendChart();
            SendAudio();
            SendJacket();
            SendBackgrond();
            SendVideoBackgrond();
            SendInformation();
            SendSettings();
            SendSkin();
            SendShowLog(showLogToggle.isOn);
        }

        private void SendInformation()
        {
            SendTitle();
            SendComposer();
            SendDifficultyName();
            SendDifficultyColor();
        }

        private void SendSettings()
        {
            SendBaseBpm();
            SendSpeed();
            SendGlobalOffset();
        }

        private void SendSkin()
        {
            SendAlignmentSkin();
            SendAccentSkin();
            SendNoteSkin();
            SendParticleSkin();
            SendSingleLineSkin();
            SendTrackSkin();
        }

        private void OnRangeField(string arg0)
        {
            if (Evaluator.TryInt(fromTimingField.text, out int from)
             && Evaluator.TryInt(toTimingField.text, out int to))
            {
                from = Mathf.Clamp(from, 0, AudioLength);
                to = Mathf.Clamp(to, 0, AudioLength);
                remoteTimingRange.SetTiming(from, to);
            }
        }

        private void OnTimingRange(int from, int to)
        {
            SendTimingRange(from, to);
            fromTimingField.SetTextWithoutNotify(from.ToString());
            toTimingField.SetTextWithoutNotify(to.ToString());
        }

        private void Update()
        {
            if (isRemotePlaying && remoteCurrentTiming.gameObject.activeInHierarchy)
            {
                int delta = Mathf.RoundToInt(Time.deltaTime * 1000);
                int newTiming = remoteCurrentTiming.Timing + delta;
                if (isClientLooping && newTiming > remoteLoopEnd)
                {
                    newTiming = remoteLoopStart;
                }

                remoteCurrentTiming.SetTiming(newTiming);
            }
        }

        private void AlignTimingRange()
        {
            remoteLoopStart = Mathf.Min(remoteLoopStart, remoteLoopEnd - Constants.MinPlaybackRange);
            remoteLoopStart = Mathf.Max(remoteLoopStart, 0);
            remoteLoopEnd = Mathf.Max(remoteLoopEnd, remoteLoopStart + Constants.MinPlaybackRange);
            remoteLoopEnd = Mathf.Min(remoteLoopEnd, Services.Gameplay.Audio.AudioLength);

            fromTimingField.text = remoteLoopStart.ToString();
            toTimingField.text = remoteLoopEnd.ToString();

            remoteTimingRange.SetTiming(remoteLoopStart, remoteLoopEnd);
        }
    }
}