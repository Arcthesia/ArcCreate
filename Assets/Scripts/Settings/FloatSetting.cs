using UnityEngine;
using UnityEngine.Events;

namespace ArcCreate
{
    public class FloatSetting
    {
        private readonly string settings;

        private float value;

        public FloatSetting(string settings, float defaultValue)
        {
            this.settings = settings;

            value = PlayerPrefs.GetFloat(settings, defaultValue);

            OnValueChanged = new OnChangeEvent();
        }

        public float Value
        {
            get => value;
            set
            {
                this.value = value;
                PlayerPrefs.SetFloat(settings, value);
                OnValueChanged.Invoke(value);
            }
        }

        public OnChangeEvent OnValueChanged { get; set; }

        public class OnChangeEvent : UnityEvent<float>
        {
        }
    }
}