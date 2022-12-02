using UnityEngine;

namespace ArcCreate.Gameplay.Audio
{
    public interface IAudioService
    {
        AudioSource AudioSource { get; }

        int Timing { get; set; }

        int SongLength { get; set; }

        bool IsPlaying { get; }

        void LoadClip(AudioClip clip);

        void Resume(int delay = 0);

        void Play(int timing = 0, int delay = 0);

        void Pause();

        void Stop();

        void UpdateTime();
    }
}