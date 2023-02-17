using System;
using ArcCreate.Compose.Popups;
using ArcCreate.Utility.Extension;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Field for handling arc color index input. Summons a <see cref="ArcColorPickerWindow"/> for picking the arc color index.
    /// </summary>
    public class ArcColorSelector : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private UIGradient gradient;
        [SerializeField] private TMP_Text text;

        private int value;
        private ArcColorPickerWindow window;

        public event Action<int> OnColorChanged;

        public int Value => value;

        /// <summary>
        /// Gets the color for arc at y=1 corresponding to the current <see cref="Value"/>.
        /// </summary>
        public Color PreviewColor { get; private set; }

        /// <summary>
        /// Gets the color for arc at y=0 corresponding to the current <see cref="Value"/>.
        /// </summary>
        public Color PreviewColorLow { get; private set; }

        /// <summary>
        /// Set the value without invoking <see cref="OnValueChange"/> event.
        /// </summary>
        /// <param name="color">The color id value.</param>
        public void SetValueWithoutNotify(int color)
        {
            value = color;
            SetPreview(color);
        }

        /// <summary>
        /// Set the value without invoking <see cref="OnValueChange"/> event.
        /// </summary>
        /// <param name="color">The color id value.</param>
        public void SetValue(int color)
        {
            value = color;
            SetPreview(color);
            OnColorChanged?.Invoke(color);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            window = Services.Popups.OpenArcColorPicker(eventData.position, value, this);
            window.OnColorChanged = OnWindow;
        }

        private void OnWindow(int color)
        {
            value = color;
            SetPreview(color);
            OnColorChanged?.Invoke(color);
        }

        private void SetPreview(int color)
        {
            switch (color)
            {
                case int.MinValue:
                    text.text = I18n.S("Compose.UI.Inspector.Mixed");
                    break;
                case 0:
                    text.text = I18n.S("Compose.UI.Project.Label.Blue");
                    break;
                case 1:
                    text.text = I18n.S("Compose.UI.Project.Label.Red");
                    break;
                case 2:
                    text.text = I18n.S("Compose.UI.Project.Label.Green");
                    break;
                default:
                    text.text = I18n.S("Compose.UI.Project.Label.Custom", color);
                    break;
            }

            gradient.gameObject.SetActive(color != int.MinValue);

            var chart = Services.Project.CurrentChart;
            if (chart.Colors == null)
            {
                Color high = Services.Gameplay.Skin.DefaultArcColors[Mathf.Clamp(color, 0, 2)];
                Color low = Services.Gameplay.Skin.DefaultArcLowColors[Mathf.Clamp(color, 0, 2)];
                gradient.m_color1 = high;
                gradient.m_color2 = low;
                gradient.enabled = false;
                gradient.enabled = true;

                PreviewColor = high;
                PreviewColorLow = low;
            }
            else
            {
                chart.Colors.Arc[Mathf.Clamp(color, 0, chart.Colors.Arc.Count - 1)].ConvertHexToColor(out Color high);
                chart.Colors.ArcLow[Mathf.Clamp(color, 0, chart.Colors.ArcLow.Count - 1)].ConvertHexToColor(out Color low);
                gradient.m_color1 = high;
                gradient.m_color2 = low;
                gradient.enabled = false;
                gradient.enabled = true;

                PreviewColor = high;
                PreviewColorLow = low;
            }

            if (window != null && ReferenceEquals(window.Owner, this))
            {
                window.SetColorWithoutNotify(color);
            }
        }
    }
}
