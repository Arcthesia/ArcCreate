using System;
using ArcCreate.Compose.Popups;
using ArcCreate.Gameplay.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class ArcTypeSelector : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Sprite[] sprites;

        private ArcLineType value;
        private ArcTypePickerWindow window;

        public event Action<ArcLineType> OnTypeChanged;

        public ArcLineType Value => value;

        /// <summary>
        /// Set the value without invoking <see cref="OnValueChange"/> event.
        /// </summary>
        /// <param name="type">The type value.</param>
        public void SetValueWithoutNotify(ArcLineType type)
        {
            value = type;
            SetPreview(type);
        }

        /// <summary>
        /// Set the value without invoking <see cref="OnValueChange"/> event.
        /// </summary>
        /// <param name="type">The type value.</param>
        public void SetValue(ArcLineType type)
        {
            value = type;
            SetPreview(type);
            OnTypeChanged?.Invoke(type);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            window = Services.Popups.OpenArcTypePicker(eventData.position, value, this);
            window.OnTypeChanged = OnWindow;
        }

        private void OnWindow(ArcLineType type)
        {
            value = type;
            SetPreview(type);
            OnTypeChanged?.Invoke(type);
        }

        private void SetPreview(ArcLineType type)
        {
            text.text = type.ToString();
            image.sprite = sprites[(int)type];
            if (ReferenceEquals(window.Owner, this))
            {
                window.SetTypeWithoutNotify(type);
            }
        }
    }
}