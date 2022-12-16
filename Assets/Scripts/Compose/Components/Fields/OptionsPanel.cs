using System;
using UnityEngine;

namespace ArcCreate.Compose.Components
{
    public class OptionsPanel : MonoBehaviour
    {
        private Option[] options;

        private string value;

        public event Action<string> OnValueChanged;

        public string Value
        {
            get => value;
            set
            {
                foreach (var opt in options)
                {
                    if (opt.Value == value)
                    {
                        this.value = value;
                        SetHighlight(opt);
                        OnValueChanged?.Invoke(opt.Value);
                        break;
                    }
                }
            }
        }

        public void SetValueWithoutNotify(string value)
        {
            foreach (var opt in options)
            {
                if (opt.Value == value)
                {
                    this.value = value;
                    SetHighlight(opt);
                    break;
                }
            }
        }

        public void OnOptionSelected(Option option)
        {
            value = option.Value;
            SetHighlight(option);
        }

        private void SetHighlight(Option option)
        {
            foreach (var opt in options)
            {
                opt.Highlighted = opt == option;
            }
        }

        private void Awake()
        {
            options = GetComponentsInChildren<Option>();
            if (options.Length > 0)
            {
                SetHighlight(options[0]);
            }

            foreach (var opt in options)
            {
                opt.Panel = this;
            }
        }
    }
}