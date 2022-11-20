using UnityEngine;
using UnityEngine.Events;

namespace ArcCreate
{
    public class IntSetting
    {
        private readonly string settings;
        private int value;

        public IntSetting(string settings, int defaultValue)
        {
            this.settings = settings;

            value = PlayerPrefs.GetInt(settings, defaultValue);
        }

        public OnChangeEvent OnValueChanged { get; set; }

        public int Value
        {
            get => value;
            set
            {
                this.value = value;
                PlayerPrefs.SetInt(settings, value);
                OnValueChanged.Invoke(value);
            }
        }

        public class OnChangeEvent : UnityEvent<int>
        {
        }
    }
}
