using UnityEngine;
using UnityEngine.Events;

namespace ArcCreate
{
    public class FloatSetting
    {
        private readonly string settings;
        private readonly float minvalue;
        private readonly float maxvalue;
        private float value;

        public FloatSetting(string settings, float defaultValue, float minvalue = float.MinValue, float maxvalue = float.MaxValue)
        {
            this.settings = settings;
            this.minvalue = minvalue;
            this.maxvalue = maxvalue;
            value = PlayerPrefs.GetFloat(settings, defaultValue);

            OnValueChanged = new OnChangeEvent();
        }

        public float Value
        {
            get => value;
            set
            {
                value = Mathf.Clamp(value, minvalue, maxvalue);
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