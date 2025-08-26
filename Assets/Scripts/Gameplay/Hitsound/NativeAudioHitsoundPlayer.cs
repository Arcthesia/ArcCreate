#if USE_NATIVE_AUDIO
using E7.Native;
using UnityEngine;

namespace ArcCreate.Gameplay.Hitsound
{
    public class NativeAudioHitsoundPlayer : IHitsoundPlayer
    {
        private static bool initialized = false;
        private NativeAudioPointer arcClip;
        private NativeAudioPointer tapClip;
        private float volume = 0.2f;
        private readonly NativeSource arcSource;
        private readonly NativeSource tapSource;

        public float Volume
        {
            get => volume;
            set => volume = value;
        }

        public NativeAudioHitsoundPlayer()
        {
            if (!initialized)
            {
                NativeAudio.Initialize(new NativeAudio.InitializationOptions
                {
                    androidAudioTrackCount = 2,
                });
            }

            initialized = true;
            tapSource = NativeAudio.GetNativeSource(1);
            arcSource = NativeAudio.GetNativeSource(0);
        }

        public void Dispose()
        {
            arcSource.Stop();
            tapSource.Stop();
            arcClip.Unload();
            tapClip.Unload();
            NativeAudio.Dispose();
            initialized = false;
        }

        public void LoadArc(AudioClip clip)
        {
            arcClip = NativeAudio.Load(clip);
        }

        public void LoadTap(AudioClip clip)
        {
            tapClip = NativeAudio.Load(clip);
        }

        public void PlayArc()
        {
            arcSource.Play(arcClip);
            arcSource.SetVolume(volume);
        }

        public void PlayTap()
        {
            tapSource.Play(tapClip);
            tapSource.SetVolume(volume);
        }
    }
}
#endif