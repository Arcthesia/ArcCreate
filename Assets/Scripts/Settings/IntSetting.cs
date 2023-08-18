using System;
using UnityEngine;
using UnityEngine.Events;

namespace ArcCreate
{
    public class IntSetting
    {
        private readonly string settings;
        private readonly int minvalue;
        private readonly int maxvalue;
        private int value;

        public IntSetting(string settings, int defaultValue, int minvalue = int.MinValue, int maxvalue = int.MaxValue)
        {
            this.settings = settings;
            this.minvalue = minvalue;
            this.maxvalue = maxvalue;
            value = PlayerPrefs.GetInt(settings, defaultValue);

            OnValueChanged = new OnChangeEvent();
        }

        public OnChangeEvent OnValueChanged { get; set; }

        public int Value
        {
            get => value;
            set
            {
                value = Mathf.Clamp(value, minvalue, maxvalue);
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
