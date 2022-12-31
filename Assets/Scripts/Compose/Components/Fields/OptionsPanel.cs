using System;
using UnityEngine;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Field for selecting a one out of a limited number of options.
    /// Does not support changing the option list on runtime.
    /// The option list will be created based on all <see cref="Option"/> components attached to children of this component's GameObject.
    /// </summary>
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
            OnValueChanged?.Invoke(option.Value);
        }

        public Option GetOptionByValue(string value)
        {
            foreach (var opt in options)
            {
                if (opt.Value == value)
                {
                    return opt;
                }
            }

            return null;
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