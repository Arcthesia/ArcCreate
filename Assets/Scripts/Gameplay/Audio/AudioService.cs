using System;
using System.Collections.Generic;
using ArcCreate.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ArcCreate.Gameplay.Audio
{
    public class AudioService : MonoBehaviour, IAudioService, IAudioControl
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private GameplayData gameplayData;
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
        /// Time (in realTimeSinceStartup unit) at which the audio started playing.
        /// </summary>
        private double realStartPlayingTime = 0;

        /// <summary>
        /// Whether to let the timing stay unchanged until the audio start playing, or to increate it linearly.
        /// </summary>
        private bool stationaryBeforeStart;

        /// <summary>
        /// The current timing value in ms.
        /// </summary>
        private int audioTiming;

        /// <summary>
        /// The audio playback speed.
        /// </summary>
        private float playbackSpeed = 1;

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

        /// <summary>
        /// Whether or not chart timing is being stationary. Example of this being true is during warming up period after a play with delay.
        /// </summary>
        private bool isStationary;

        /// <summary>
        /// Scalar to speed up or slow down chart update speed to sync with music.
        /// </summary>
        private float updatePace = 1;

        private bool audioEndReported;

        public AudioSource AudioSource => audioSource;

        public VideoPlayer VideoPlayer => videoPlayer;

        public int ChartTiming
        {
            get => AudioTiming - FullOffset;
            set
            {
                AudioTiming = value + FullOffset;
            }
        }

        public int AudioTiming
        {
            get => audioTiming;
            set
            {
                audioTiming = value;
                UpdateSlider(value);
                if (IsPlaying)
                {
                    audioSource.Stop();
                    videoPlayer.Pause();
                    Play(audioTiming, 0);
                }

                Services.Chart.ResetJudge();
            }
        }

        public float PlaybackSpeed
        {
            get => playbackSpeed;
            set
            {
                playbackSpeed = value;
                audioSource.pitch = value;
                if (IsPlayingAndNotStationary && (Application.isMobilePlatform || Settings.SyncToDSPTime.Value))
                {
                    Pause();
                    ResumeWithDelay(200, false);
                }
            }
        }

        public int AudioLength { get; private set; }

        public bool IsPlaying => audioSource.isPlaying;

        public bool IsPlayingAndNotStationary => (audioSource.isPlaying && !isStationary) || IsRendering;

        public bool IsLoaded => audioSource.clip != null;

        public bool IsRendering { get; set; }

        public AudioClip AudioClip
        {
            get => audioSource.clip;
            set
            {
                audioSource.clip = value;
                AudioLength = Mathf.RoundToInt(value.length * 1000);
            }
        }

        public AudioClip TapHitsoundClip => Services.Hitsound.TapHitsoundClip;

        public AudioClip ArcHitsoundClip => Services.Hitsound.ArcHitsoundClip;

        public Dictionary<string, AudioClip> SfxAudioClips => Services.Hitsound.SfxAudioClips;

        private int FullOffset => Values.ChartAudioOffset + Settings.GlobalAudioOffset.Value;

        public void SetAudioTimingSilent(int timing)
        {
            audioTiming = timing;
            UpdateSlider(timing);
        }

        public void UpdateTime()
        {
            double dspTime = AudioSettings.dspTime;
            if (!IsPlaying)
            {
                if (audioSource.clip != null && audioTiming >= Mathf.Max(0, AudioLength - 100))
                {
                    OnAudioEnd();
                }

                return;
            }

            if (Application.isMobilePlatform || Settings.SyncToDSPTime.Value)
            {
                isStationary = stationaryBeforeStart && dspTime <= dspStartPlayingTime;

                if (stationaryBeforeStart)
                {
                    dspTime = Math.Max(dspTime, dspStartPlayingTime);
                }

                int dspTimePassedSinceAudioStart = Mathf.RoundToInt((float)((dspTime - dspStartPlayingTime) * 1000 * playbackSpeed));
                int realTimePassedSinceAudioStart = Mathf.RoundToInt((float)((Time.realtimeSinceStartup - realStartPlayingTime) * 1000 * playbackSpeed));
                updatePace = Mathf.Approximately(realTimePassedSinceAudioStart, 0) ? 1
                           : Mathf.Lerp(updatePace, (float)dspTimePassedSinceAudioStart / realTimePassedSinceAudioStart, 0.1f);
                int newTiming = Mathf.RoundToInt(realTimePassedSinceAudioStart * updatePace) + startTime - FullOffset;

                if (!stationaryBeforeStart || dspTime > dspStartPlayingTime)
                {
                    audioTiming = newTiming;
                }
            }
            else
            {
                audioTiming = Mathf.RoundToInt(AudioSource.time * 1000f);
            }

            UpdateSlider(audioTiming);
        }

        public void PauseButtonPressed()
        {
            if (!IsPlaying)
            {
                ResumeImmediately();
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            lastPausedTiming = audioTiming;
            audioSource.Stop();
            if (videoPlayer.enabled)
            {
                videoPlayer.Pause();
            }

            if (returnOnPause)
            {
                lastPausedTiming = onPauseReturnTo;
                AudioTiming = onPauseReturnTo;
            }

            SetEnableAutorotation(true);
        }

        public void Stop()
        {
            audioSource.Stop();
            if (videoPlayer.enabled)
            {
                videoPlayer.Stop();
            }

            lastPausedTiming = 0;
            AudioTiming = 0;

            SetEnableAutorotation(true);
        }

        public void PlayImmediately(int timing)
        {
            stationaryBeforeStart = false;
            returnOnPause = false;
            Play(timing, 0);
        }

        public void PlayWithDelay(int timing, int delayMs)
        {
            stationaryBeforeStart = false;
            returnOnPause = false;
            Play(timing, delayMs);
        }

        public void ResumeImmediately(bool resetJudge = true)
        {
            stationaryBeforeStart = true;
            returnOnPause = false;
            Play(lastPausedTiming, 0, resetJudge);
        }

        public void ResumeWithDelay(int delayMs, bool resetJudge = true)
        {
            stationaryBeforeStart = true;
            returnOnPause = false;
            Play(lastPausedTiming, delayMs, resetJudge);
        }

        public void ResumeReturnableImmediately()
        {
            stationaryBeforeStart = true;
            returnOnPause = true;
            onPauseReturnTo = audioTiming;
            Play(lastPausedTiming);
        }

        public void ResumeReturnableWithDelay(int delayMs)
        {
            stationaryBeforeStart = true;
            returnOnPause = true;
            onPauseReturnTo = audioTiming;
            Play(lastPausedTiming, delayMs);
        }

        public void SetResumeAt(int timing)
        {
            lastPausedTiming = timing;
        }

        public void SetReturnOnPause(bool cond, int timing = 0)
        {
            returnOnPause = cond;
            onPauseReturnTo = timing;
        }

        private void Play(int timing = 0, int delay = 0, bool resetJudge = true)
        {
            delay = Mathf.Max(delay, 0);
            if (timing >= AudioLength - 1)
            {
                timing = 0;
            }

            if (timing < 0)
            {
                stationaryBeforeStart = false;
                timing = 0;
            }

            audioTiming = stationaryBeforeStart ? timing : timing - delay;
            updatePace = 1;

            if (resetJudge)
            {
                Services.Chart.ResetJudge();
            }

            audioSource.time = Mathf.Max(0, timing) / 1000f;
            if (timing < 0)
            {
                delay += -timing;
            }

            dspStartPlayingTime = AudioSettings.dspTime + ((double)delay / 1000);
            realStartPlayingTime = Time.realtimeSinceStartup + ((double)delay / 1000);
            startTime = timing + FullOffset;
            if (delay > 0)
            {
                audioSource.PlayScheduled(dspStartPlayingTime);
            }
            else
            {
                audioSource.Play();
            }

            if (videoPlayer.enabled)
            {
                StartDelayedVideoPlayback(timing, delay).Forget();
            }

            SetEnableAutorotation(false);
            audioEndReported = false;
        }

        private async UniTask StartDelayedVideoPlayback(int timing, int delay)
        {
            DateTime startTime = DateTime.Now;
            videoPlayer.Prepare();
            await UniTask.WhenAll(UniTask.Delay(delay), UniTask.WaitUntil(() => videoPlayer.isPrepared));
            videoPlayer.Play();
            videoPlayer.time = Mathf.Clamp(((timing - delay) / 1000f) + (DateTime.Now - startTime).Seconds, 0, (float)videoPlayer.length);
        }

        private void Awake()
        {
            gameplayData.AudioClip.OnValueChange += OnClipLoad;
            Settings.MusicAudio.OnValueChanged.AddListener(OnMusicAudioSettings);
            OnMusicAudioSettings(Settings.MusicAudio.Value);
        }

        private void OnDestroy()
        {
            gameplayData.AudioClip.OnValueChange -= OnClipLoad;
            Settings.MusicAudio.OnValueChanged.RemoveListener(OnMusicAudioSettings);
        }

        private void OnClipLoad(AudioClip clip)
        {
            AudioClip = clip;
        }

        private void SetEnableAutorotation(bool v)
        {
            Screen.autorotateToLandscapeLeft = v;
            Screen.autorotateToLandscapeRight = v;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
        }

        private void OnMusicAudioSettings(float volume)
        {
            audioSource.volume = Mathf.Clamp(volume, 0, 1);
        }

        private void OnAudioEnd()
        {
            if (!audioEndReported && Values.ShouldNotifyOnAudioEnd && !gameplayData.EnablePracticeMode.Value)
            {
                PlayResult result = Services.Score.GetPlayResult();
                gameplayData.NotifyPlayComplete(result);
            }

            audioEndReported = true;
        }

        private void UpdateSlider(float timing)
        {
            timingSlider.value = AudioLength > 0 ? Mathf.Clamp(timing / AudioLength, 0, 1) : 0;
        }
    }
}