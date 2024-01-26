using System;
using System.Security.Cryptography.X509Certificates;
using ArcCreate.Compose.Popups;
using ArcCreate.Gameplay.Chart;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace ArcCreate.Compose.Components
{
    public class TimingGroupField : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private TMP_Text text;
        private TimingGroupPicker window;
        private TimingGroup value;

        public event Action<TimingGroup> OnValueChanged;

        public TimingGroup Value => value;

        public void Open(string title)
        {
            Vector2 position = Input.mousePosition;
            OpenWindow(position, title);
        }

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
            OpenWindow(eventData.position, string.Empty);
        }

        private void OpenWindow(Vector2 position, string title)
        {
            if (Services.Gameplay?.IsLoaded ?? false)
            {
                window = Services.Popups.OpenTimingGroupPicker(position, value.GroupNumber, title, this);
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