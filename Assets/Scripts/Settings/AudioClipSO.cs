using UnityEngine;
using UnityEngine.Events;

namespace ArcCreate
{
    [CreateAssetMenu(fileName = "AudioClip", menuName = "ScriptableObject/AudioClip")]
    public class AudioClipSO : ScriptableObject
    {
        private AudioClip value;

        public AudioClip Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChange.Invoke(value);
            }
        }

        public OnChangeEvent OnValueChange { get; set; } = new OnChangeEvent();

        public class OnChangeEvent : UnityEvent<AudioClip>
        {
        }
    }
}