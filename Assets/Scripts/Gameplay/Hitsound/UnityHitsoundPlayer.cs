using UnityEngine;

namespace ArcCreate.Gameplay.Hitsound
{
    public class UnityHitsoundPlayer : IHitsoundPlayer
    {
        private readonly AudioSource audioSource;
        private AudioClip arcClip;
        private AudioClip tapClip;

        public UnityHitsoundPlayer(AudioSource audioSource)
        {
            this.audioSource = audioSource;
        }

        public float Volume
        {
            get => audioSource.volume;
            set => audioSource.volume = value;
        }

        public void Dispose()
        {
        }

        public void LoadArc(AudioClip clip)
        {
            arcClip = clip;
        }

        public void LoadTap(AudioClip clip)
        {
            tapClip = clip;
        }

        public void PlayArc()
        {
            audioSource.PlayOneShot(arcClip);
        }

        public void PlayTap()
        {
            audioSource.PlayOneShot(tapClip);
        }
    }
}