using UnityEngine;
using UnityEngine.Events;

namespace ArcCreate
{
    public class BoolSetting
    {
        private readonly string settings;

        private bool value;

        public BoolSetting(string settings, bool defaultValue)
        {
            this.settings = settings;

            value = PlayerPrefs.GetInt(settings, defaultValue ? 1 : 0) == 1;
        }

        public OnChangeEvent OnValueChanged { get; set; }

        public bool Value
        {
            get => value;
            set
            {
                this.value = value;
                PlayerPrefs.SetInt(settings, value ? 1 : 0);
                OnValueChanged.Invoke(value);
            }
        }

        public class OnChangeEvent : UnityEvent<bool>
        {
        }
    }
}
