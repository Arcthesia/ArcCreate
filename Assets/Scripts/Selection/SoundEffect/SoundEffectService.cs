using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Selection.SoundEffect
{
    public class SoundEffectService : MonoBehaviour, ISoundEffectService
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private List<SoundEffectDefinition> soundEffects;

        public void Play(Sound soundEffect)
        {
            foreach (var def in soundEffects)
            {
                if (def.Type == soundEffect)
                {
                    audioSource.PlayOneShot(def.Audio);
                    return;
                }
            }
        }

        [Serializable]
        private struct SoundEffectDefinition
        {
            public Sound Type;

            public AudioClip Audio;
        }
    }
}