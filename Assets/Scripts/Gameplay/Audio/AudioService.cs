using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ArcCreate.Gameplay.Audio
{
    public class AudioService : MonoBehaviour, IAudioService, IAudioControl
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private AudioClipSO audioClipSO;
        [SerializeField] private Slider timingSlider;

        /// <summary>
        /// Timing at which the audio started playing from.
        /// </summary>
        private int startTime = 0;

        /// <summary>
        /// Time (in dsp unit) at which the audio started playing.
        /// </summary>
        private double dspStartPlayingTime = 0;

        /// <summary>
        /// Whether to let the timing stay unchanged until the audio start playing, or to increate it linearly.
        /// </summary>
        private bool stationaryBeforeStart;

        /// <summary>
        /// The current timing value in ms.
        /// </summary>
        private int timing;

        /// <summary>
        /// Last timing the audio was paused at.
        /// </summary>
        private int lastPausedTiming = 0;

        /// <summary>
        /// Whether to return to <see cref="onPauseReturnTo"/> timing point after next pause.
        /// </summary>
        private bool returnOnPause = false;

        /// <summary>
        /// Return to this timing point after next pause.
        /// </summary>
        private int onPauseReturnTo = 0;

        public AudioSource AudioSource => audioSource;

        public VideoPlayer VideoPlayer => videoPlayer;

        public int Timing
        {
            get => timing;
            set
            {
                timing = value;
                if (IsPlaying)
                {
                    Pause();
                    Play(timing, 0);
                }
                else
                {
                    Services.Chart.ResetJudge();
                }
            }
        }

        public int AudioLength { get; private set; }

        public bool IsPlaying { get => audioSource.isPlaying; }

        public bool AutomaticallyReturnOnAudioEnd { get; set; } = true;

        public AudioClip AudioClip
        {
            get => audioSource.clip;
            set
            {
                audioSource.clip = value;
                AudioLength = Mathf.RoundToInt(value.length * 1000);
            }
        }

        private int FullOffset => Values.ChartAudioOffset + Settings.GlobalAudioOffset.Value;

        public void LoadAudio(string path)
        {
            StartLoadingAudio(path).Forget();
        }

        public void UpdateTime()
        {
            double dspTime = AudioSettings.dspTime;

            if (!IsPlaying)
            {
                return;
            }

            if (stationaryBeforeStart)
            {
                dspTime = System.Math.Max(dspTime, dspStartPlayingTime);
            }

            int timePassedSinceAudioStart = Mathf.RoundToInt((float)((dspTime - dspStartPlayingTime) * 1000));
            int newTiming = timePassedSinceAudioStart + startTime - FullOffset;

            if (timing != newTiming && dspTime >= dspStartPlayingTime)
            {
                timing += Mathf.RoundToInt(Time.deltaTime * 1000);
            }
            else
            {
                timing = newTiming;
            }

            if (AutomaticallyReturnOnAudioEnd && timing > AudioLength)
            {
                Stop();
            }

            timingSlider.value = (float)timing / AudioLength;
        }

        public void Pause()
        {
            lastPausedTiming = timing;
            audioSource.Stop();
            if (returnOnPause)
            {
                lastPausedTiming = onPauseReturnTo;
                timing = onPauseReturnTo;
            }
        }

        public void Stop()
        {
            audioSource.Stop();
            Timing = 0;
        }

        public void PlayImmediately(int timing)
        {
            Play(timing, 0);
            stationaryBeforeStart = false;
            returnOnPause = false;
        }

        public void PlayWithDelay(int timing, int delayMs)
        {
            Play(timing, delayMs);
            stationaryBeforeStart = false;
            returnOnPause = false;
        }

        public void ResumeImmediately()
        {
            Play(lastPausedTiming);
            stationaryBeforeStart = true;
            returnOnPause = false;
        }

        public void ResumeWithDelay(int delayMs)
        {
            Play(lastPausedTiming, delayMs);
            stationaryBeforeStart = true;
            returnOnPause = false;
        }

        public void ResumeReturnableImmediately()
        {
            Play(lastPausedTiming);
            stationaryBeforeStart = true;
            returnOnPause = true;
            onPauseReturnTo = timing;
        }

        public void ResumeReturnableWithDelay(int delayMs)
        {
            Play(lastPausedTiming, delayMs);
            stationaryBeforeStart = true;
            returnOnPause = true;
            onPauseReturnTo = timing;
        }

        private void Play(int timing = 0, int delay = 0)
        {
            delay = Mathf.Max(delay, 0);
            this.timing = timing;
            Services.Chart.ResetJudge();

            audioSource.time = (float)timing / 1000;

            dspStartPlayingTime = AudioSettings.dspTime + ((double)delay / 1000);
            startTime = timing;
            if (delay > 0)
            {
                audioSource.PlayScheduled(dspStartPlayingTime);
            }
            else
            {
                audioSource.Play();
            }
        }

        private async UniTask StartLoadingAudio(string path)
        {
            using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(
                Uri.EscapeUriString("file:///" + path.Replace("\\", "/")),
                path.EndsWith("wav") ? AudioType.WAV : AudioType.OGGVORBIS))
            {
                await req.SendWebRequest();
                if (!string.IsNullOrWhiteSpace(req.error))
                {
                    throw new IOException(I18n.S("Gameplay.Exception.LoadAudio", new Dictionary<string, object>()
                    {
                        { "Path", path },
                        { "Error", req.error },
                    }));
                }

                audioClipSO.Value = DownloadHandlerAudioClip.GetContent(req);
            }
        }

        private void Awake()
        {
            audioClipSO.OnValueChange.AddListener(OnClipLoad);
        }

        private void OnDestroy()
        {
            audioClipSO.OnValueChange.RemoveListener(OnClipLoad);
        }

        private void OnClipLoad(AudioClip clip)
        {
            AudioClip = clip;
        }
    }
}