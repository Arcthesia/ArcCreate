using System;
using ArcCreate.Compose.Popups;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Field for inputing color.
    /// </summary>
    public class ColorInputField : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Image colorPreview;
        [SerializeField] private Image colorWithAlphaPreview;
        [SerializeField] private Color currentColor;

        /// <summary>
        /// Event invoked after the color value has changed.
        /// </summary>
        public event Action<Color> OnValueChange;

        public Color Value => currentColor;

        public void OnPointerDown(PointerEventData eventData)
        {
            ColorPickerWindow window = Services.Popups.OpenColorPicker(eventData.position, currentColor);
            window.OnColorChanged = SetValue;
        }

        /// <summary>
        /// Set the value and invoke <see cref="OnValueChange"/> event.
        /// </summary>
        /// <param name="color">The color value.</param>
        public void SetValue(Color color)
        {
            currentColor = color;
            SetColorPreview(color);
            OnValueChange?.Invoke(color);
        }

        /// <summary>
        /// Set the value without invoking <see cref="OnValueChange"/> event.
        /// </summary>
        /// <param name="color">The color value.</param>
        public void SetValueWithoutNotify(Color color)
        {
            currentColor = color;
            SetColorPreview(color);
        }

        private void Awake()
        {
            SetColorPreview(currentColor);
        }

        private void SetColorPreview(Color color)
        {
            colorWithAlphaPreview.color = color;
            color.a = 1;
            colorPreview.color = color;
        }
    }
}