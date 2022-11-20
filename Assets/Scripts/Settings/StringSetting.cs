using UnityEngine;
using UnityEngine.Events;

namespace ArcCreate
{
    public class StringSetting
    {
        private readonly string settings;
        private string value;

        public StringSetting(string settings, string defaultValue)
        {
            this.settings = settings;

            value = PlayerPrefs.GetString(settings, defaultValue);
        }

        public OnChangeEvent OnValueChanged { get; set; }

        public string Value
        {
            get => value;
            set
            {
                this.value = value;
                PlayerPrefs.SetString(settings, value);
                OnValueChanged.Invoke(value);
            }
        }

        public class OnChangeEvent : UnityEvent<string>
        {
        }
    }
}
