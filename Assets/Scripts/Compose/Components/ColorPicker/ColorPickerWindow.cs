using System;
using ArcCreate.Utility.Extension;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Component for handling color picker window.
    /// The same color picker window can be reused for different input fields.
    /// </summary>
    public class ColorPickerWindow : MonoBehaviour, IColorPickerService
    {
        [SerializeField] private GameObject window;
        [SerializeField] private ColorPicker picker;
        [SerializeField] private Slider alphaSlider;
        [SerializeField] private Button closeButton;
        [SerializeField] private float minDistanceFromBorder;
        [SerializeField] private TMP_InputField redField;
        [SerializeField] private TMP_InputField greenField;
        [SerializeField] private TMP_InputField blueField;
        [SerializeField] private TMP_InputField hueField;
        [SerializeField] private TMP_InputField saturationField;
        [SerializeField] private TMP_InputField valueField;
        [SerializeField] private TMP_InputField alphaField;
        [SerializeField] private TMP_InputField hexField;
        [SerializeField] private Image sliderGradient;
        [SerializeField] private Image colorPreview;
        [SerializeField] private Image colorAlphaPreview;
        private RectTransform rect;

        public Action<Color> OnColorChanged { get; set; }

        public Color Color
        {
            get => picker.color;
            set
            {
                SetRGBFields(value);
                SetHSVFields(value);
                SetCommon(value);
            }
        }

        public void OpenAt(Vector2 screenPosition, Color setColor)
        {
            window.SetActive(true);
            closeButton.gameObject.SetActive(true);

            Color = setColor;

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float rectWidth = rect.rect.width;
            float rectHeight = rect.rect.height;
            Vector2 pivot = rect.pivot;

            float x = Mathf.Clamp(
                screenPosition.x,
                minDistanceFromBorder + (rectWidth * pivot.x),
                screenWidth - minDistanceFromBorder - (rectWidth * (1 - pivot.x)));

            float y = Mathf.Clamp(
                screenPosition.y,
                minDistanceFromBorder + (rectHeight * pivot.y),
                screenHeight - minDistanceFromBorder - (rectHeight * (1 - pivot.y)));

            rect.anchoredPosition = new Vector2(x, y);
        }

        private void OnPicker(Color color)
        {
            color.a = Evaluator.Float(alphaField.text) / 255;
            Color = color;
            OnColorChanged?.Invoke(color);
        }

        private void OnAlphaSlider(float val)
        {
            Color c = Color;
            c.a = val;
            Color = c;
            OnColorChanged?.Invoke(c);
        }

        private Color ReadFromRGBField()
        {
            Evaluator.TryFloat(redField.text, out float red);
            Evaluator.TryFloat(greenField.text, out float green);
            Evaluator.TryFloat(blueField.text, out float blue);
            Evaluator.TryFloat(alphaField.text, out float alpha);

            red = Mathf.Clamp(red, 0, 255);
            green = Mathf.Clamp(green, 0, 255);
            blue = Mathf.Clamp(blue, 0, 255);
            alpha = Mathf.Clamp(alpha, 0, 255);

            return new Color(red / 255, green / 255, blue / 255, alpha / 255);
        }

        private Color ReadFromHSVField()
        {
            Evaluator.TryFloat(hueField.text, out float hue);
            Evaluator.TryFloat(saturationField.text, out float saturation);
            Evaluator.TryFloat(valueField.text, out float value);
            Evaluator.TryFloat(alphaField.text, out float alpha);

            hue = Mathf.Clamp(hue, 0, 1);
            saturation = Mathf.Clamp(saturation, 0, 1);
            value = Mathf.Clamp(value, 0, 1);
            alpha = Mathf.Clamp(alpha, 0, 255);

            Color c = Color.HSVToRGB(hue, saturation, value);
            c.a = alpha / 255;
            return c;
        }

        private void SetRGBFields(Color color)
        {
            redField.SetTextWithoutNotify(Mathf.RoundToInt(color.r * 255).ToString());
            greenField.SetTextWithoutNotify(Mathf.RoundToInt(color.g * 255).ToString());
            blueField.SetTextWithoutNotify(Mathf.RoundToInt(color.b * 255).ToString());
            alphaField.SetTextWithoutNotify(Mathf.RoundToInt(color.a * 255).ToString());
        }

        private void SetHSVFields(Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            hueField.SetTextWithoutNotify(h.ToString("F4"));
            saturationField.SetTextWithoutNotify(s.ToString("F4"));
            valueField.SetTextWithoutNotify(v.ToString("F4"));
        }

        private void SetCommon(Color color)
        {
            picker.SetColorWithoutNotify(color);
            alphaSlider.SetValueWithoutNotify(color.a);
            hexField.text = color.ConvertToHexCode();
            colorAlphaPreview.color = color;
            color.a = 1;
            colorPreview.color = color;
            sliderGradient.color = color;
        }

        private void OnRGBFieldChange(string val)
        {
            Color rgb = ReadFromRGBField();
            SetHSVFields(rgb);
            SetCommon(rgb);
            OnColorChanged?.Invoke(rgb);
        }

        private void OnRGBFieldConfirm(string val)
        {
            Color rgb = ReadFromRGBField();
            Color = rgb;
            OnColorChanged?.Invoke(rgb);
        }

        private void OnHSVFieldChange(string val)
        {
            Color rgb = ReadFromHSVField();
            rgb.a = Evaluator.Float(alphaField.text) / 255;
            SetRGBFields(rgb);
            SetCommon(rgb);
            OnColorChanged?.Invoke(rgb);
        }

        private void OnHSVFieldConfirm(string val)
        {
            Color rgb = ReadFromHSVField();
            Color = rgb;
            OnColorChanged?.Invoke(rgb);
        }

        private void OnHexFieldConfirm(string hex)
        {
            if (hex.ConvertHexToColor(out Color c))
            {
                Color = c;
                OnColorChanged?.Invoke(c);
            }
            else
            {
                hexField.text = Color.ConvertToHexCode();
            }
        }

        private void Awake()
        {
            picker.onColorChanged += OnPicker;
            rect = window.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);

            closeButton.onClick.AddListener(CloseWindow);

            redField.onValueChanged.AddListener(OnRGBFieldChange);
            greenField.onValueChanged.AddListener(OnRGBFieldChange);
            blueField.onValueChanged.AddListener(OnRGBFieldChange);

            alphaSlider.onValueChanged.AddListener(OnAlphaSlider);
            alphaField.onValueChanged.AddListener(OnRGBFieldChange);

            redField.onEndEdit.AddListener(OnRGBFieldConfirm);
            greenField.onEndEdit.AddListener(OnRGBFieldConfirm);
            blueField.onEndEdit.AddListener(OnRGBFieldConfirm);
            alphaField.onEndEdit.AddListener(OnRGBFieldConfirm);

            hueField.onValueChanged.AddListener(OnHSVFieldChange);
            saturationField.onValueChanged.AddListener(OnHSVFieldChange);
            valueField.onValueChanged.AddListener(OnHSVFieldChange);

            hueField.onEndEdit.AddListener(OnHSVFieldConfirm);
            saturationField.onEndEdit.AddListener(OnHSVFieldConfirm);
            valueField.onEndEdit.AddListener(OnHSVFieldConfirm);

            hexField.onEndEdit.AddListener(OnHexFieldConfirm);
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveAllListeners();
            picker.onColorChanged -= OnPicker;

            redField.onValueChanged.RemoveAllListeners();
            greenField.onValueChanged.RemoveAllListeners();
            blueField.onValueChanged.RemoveAllListeners();

            alphaSlider.onValueChanged.RemoveAllListeners();
            alphaField.onValueChanged.RemoveAllListeners();

            redField.onEndEdit.RemoveAllListeners();
            greenField.onEndEdit.RemoveAllListeners();
            blueField.onEndEdit.RemoveAllListeners();
            alphaField.onEndEdit.RemoveAllListeners();

            hueField.onValueChanged.RemoveAllListeners();
            saturationField.onValueChanged.RemoveAllListeners();
            valueField.onValueChanged.RemoveAllListeners();

            hueField.onEndEdit.RemoveAllListeners();
            saturationField.onEndEdit.RemoveAllListeners();
            valueField.onEndEdit.RemoveAllListeners();

            hexField.onEndEdit.RemoveAllListeners();
        }

        private void CloseWindow()
        {
            window.SetActive(false);
            closeButton.gameObject.SetActive(false);
        }
    }
}