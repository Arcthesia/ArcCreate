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
        /// Whether to let the timing stay unchanged until the audio start playing, or to increate it linearly.
        /// </summary>
        private bool stationaryBeforeStart;

        /// <summary>
        /// The current timing value in ms.
        /// </summary>
        private int audioTiming;

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
                if (IsPlaying)
                {
                    audioSource.Stop();
                    Play(audioTiming, 0);
                }

                Services.Chart.ResetJudge();
            }
        }

        public int AudioLength { get; private set; }

        public bool IsPlaying { get => audioSource.isPlaying; }

        public bool IsLoaded => audioSource.clip != null;

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

            if (audioTiming != newTiming && dspTime >= dspStartPlayingTime)
            {
                audioTiming += Mathf.RoundToInt(Time.deltaTime * 1000);
            }
            else
            {
                audioTiming = newTiming;
            }

            timingSlider.value = (float)audioTiming / AudioLength;
        }

        public void Pause()
        {
            lastPausedTiming = audioTiming;
            audioSource.Stop();
            if (returnOnPause)
            {
                lastPausedTiming = onPauseReturnTo;
                audioTiming = onPauseReturnTo - FullOffset;
            }
        }

        public void Stop()
        {
            audioSource.Stop();
            lastPausedTiming = 0;
            AudioTiming = 0;
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
            onPauseReturnTo = audioTiming;
        }

        public void ResumeReturnableWithDelay(int delayMs)
        {
            Play(lastPausedTiming, delayMs);
            stationaryBeforeStart = true;
            returnOnPause = true;
            onPauseReturnTo = audioTiming;
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

        private void Play(int timing = 0, int delay = 0)
        {
            delay = Mathf.Max(delay, 0);
            if (timing >= AudioLength - 1)
            {
                timing = 0;
            }

            audioTiming = timing;
            Services.Chart.ResetJudge();

            audioSource.time = timing / 1000f;

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

        private void Awake()
        {
            gameplayData.AudioClip.OnValueChange += OnClipLoad;
        }

        private void OnDestroy()
        {
            gameplayData.AudioClip.OnValueChange -= OnClipLoad;
        }

        private void OnClipLoad(AudioClip clip)
        {
            AudioClip = clip;
        }
    }
}