using System;
using ArcCreate.Compose.Popups;
using ArcCreate.Gameplay.Chart;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Components
{
    public class TimingGroupField : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private TMP_Text text;
        private TimingGroupPicker window;
        private TimingGroup value;

        public event Action<TimingGroup> OnValueChanged;

        public TimingGroup Value => value;

        /// <summary>
        /// Set the value without invoking <see cref="OnValueChanged"/> event.
        /// </summary>
        /// <param name="tg">The timing group instance.</param>
        public void SetValueWithoutNotify(TimingGroup tg)
        {
            value = tg;
            SetPreview(tg);
        }

        /// <summary>
        /// Set the value without invoking <see cref="OnValueChangeddd"/> event.
        /// </summary>
        /// <param name="tg">The timing group instance.</param>
        public void SetValue(TimingGroup tg)
        {
            value = tg;
            SetPreview(tg);
            OnValueChanged?.Invoke(tg);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Services.Gameplay?.IsLoaded ?? false)
            {
                window = Services.Popups.OpenTimingGroupPicker(eventData.position, value.GroupNumber, this);
                window.OnEndEdit = OnWindow;
            }
        }

        private void OnWindow(TimingGroup obj)
        {
            value = obj;
            SetPreview(obj);
            OnValueChanged?.Invoke(obj);
        }

        private void SetPreview(TimingGroup group)
        {
            if (group == null)
            {
                text.text = string.Empty;
                return;
            }

            if (group.GroupNumber == 0)
            {
                text.text = "Base group";
            }
            else
            {
                if (string.IsNullOrEmpty(group.GroupProperties.Name))
                {
                    text.text = $"Group {group.GroupNumber}";
                }
                else
                {
                    text.text = $"{group.GroupProperties.Name} ({group.GroupNumber})";
                }
            }

            if (window != null && ReferenceEquals(window.Owner, this))
            {
                window.SetValueWithoutNotify(value);
            }
        }
    }
}