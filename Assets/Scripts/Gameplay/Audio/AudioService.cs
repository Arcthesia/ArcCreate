using UnityEngine;
using UnityEngine.Video;

namespace ArcCreate.Gameplay.Audio
{
    public class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private VideoPlayer videoPlayer;

        private int startTime = 0;
        private int delay = 0;
        private double dspStartPlayingTime = 0;
        private int timing;
        private bool stationaryBeforeStart;

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

                Services.Chart.ResetJudge();
            }
        }

        public int SongLength { get; set; }

        public bool IsPlaying { get => audioSource.isPlaying; }

        public void LoadClip(AudioClip clip)
        {
            audioSource.clip = clip;
            SongLength = Mathf.RoundToInt(clip.length * 1000);
        }

        public void Resume(int delay = 0)
        {
            Play(timing);
            stationaryBeforeStart = true;
        }

        public void Play(int timing = 0, int delay = 0)
        {
            stationaryBeforeStart = timing <= 0;
            this.delay = delay;
            dspStartPlayingTime = AudioSettings.dspTime;
            startTime = timing;
            audioSource.PlayScheduled(dspStartPlayingTime + ((double)delay / 1000));
        }

        public void Pause()
        {
            audioSource.Stop();
        }

        public void Stop()
        {
            audioSource.Stop();
            Timing = 0;
        }

        public void UpdateTime()
        {
            double dspTime = AudioSettings.dspTime;
            if (!IsPlaying)
            {
                return;
            }

            if (dspTime < (dspStartPlayingTime + (delay / 1000)) && stationaryBeforeStart)
            {
                return;
            }

            timing = Mathf.RoundToInt(
                ((float)(dspTime - dspStartPlayingTime) * 1000) + startTime - delay) - Values.Offset;
        }
    }
}