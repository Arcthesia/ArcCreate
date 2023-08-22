using UnityEngine;

namespace ArcCreate.Utility
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceFix : MonoBehaviour
    {
        private static double lastResetAt = double.MinValue;

        [SerializeField] private bool reactivateAudio = true;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfig;
        }

        private void OnDestroy()
        {
            AudioSettings.OnAudioConfigurationChanged -= OnAudioConfig;
        }

        private void OnAudioConfig(bool deviceWasChanged)
        {
            if (deviceWasChanged && Time.realtimeSinceStartup > lastResetAt)
            {
                AudioConfiguration config = AudioSettings.GetConfiguration();
                config.dspBufferSize = 512;
                AudioSettings.Reset(config);
                lastResetAt = Time.realtimeSinceStartup;
            }

            if (reactivateAudio)
            {
                audioSource.Play();
            }
        }
    }
}