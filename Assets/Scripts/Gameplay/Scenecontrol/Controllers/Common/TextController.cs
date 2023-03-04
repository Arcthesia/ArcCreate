using System.Collections.Generic;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class TextController : Controller, IPositionController, IRectController, ITextController, IColorController
    {
        private static readonly Dictionary<string, TMP_FontAsset> FontCache = new Dictionary<string, TMP_FontAsset>();
        private string defaultText;

        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private RectTransform rectTransform;

        public Vector3 DefaultTranslation { get; private set; }

        public Quaternion DefaultRotation { get; private set; }

        public Vector3 DefaultScale { get; private set; }

        public float DefaultRectW { get; private set; }

        public float DefaultRectH { get; private set; }

        public Vector2 DefaultAnchorMin { get; private set; }

        public Vector2 DefaultAnchorMax { get; private set; }

        public Vector2 DefaultPivot { get; private set; }

        public Color DefaultColor { get; private set; }

        public float DefaultFontSize { get; private set; }

        public float DefaultLineSpacing { get; private set; }

        public TMP_FontAsset DefaultFontAsset { get; private set; }

        public string DefaultFont { get; private set; }

        public ValueChannel TranslationX { get; set; }

        public ValueChannel TranslationY { get; set; }

        public ValueChannel TranslationZ { get; set; }

        public ValueChannel RotationX { get; set; }

        public ValueChannel RotationY { get; set; }

        public ValueChannel RotationZ { get; set; }

        public ValueChannel ScaleX { get; set; }

        public ValueChannel ScaleY { get; set; }

        public ValueChannel ScaleZ { get; set; }

        public ValueChannel RectW { get; set; }

        public ValueChannel RectH { get; set; }

        public ValueChannel AnchorMinX { get; set; }

        public ValueChannel AnchorMinY { get; set; }

        public ValueChannel AnchorMaxX { get; set; }

        public ValueChannel AnchorMaxY { get; set; }

        public ValueChannel PivotX { get; set; }

        public ValueChannel PivotY { get; set; }

        public TextChannel Text { get; set; }

        public ValueChannel ColorR { get; set; }

        public ValueChannel ColorG { get; set; }

        public ValueChannel ColorB { get; set; }

        public ValueChannel ColorH { get; set; }

        public ValueChannel ColorS { get; set; }

        public ValueChannel ColorV { get; set; }

        public ValueChannel ColorA { get; set; }

        public ValueChannel FontSize { get; set; }

        public ValueChannel LineSpacing { get; set; }

        public StringChannel Font { get; set; }

        public virtual string DefaultText => defaultText;

        public TMP_Text TextComponent => textComponent;

        public override void SetupDefault()
        {
            DefaultColor = textComponent.color;
            DefaultRectW = rectTransform.rect.width;
            DefaultRectH = rectTransform.rect.height;
            DefaultTranslation = rectTransform.anchoredPosition3D;
            DefaultRotation = rectTransform.localRotation;
            DefaultScale = rectTransform.localScale;
            DefaultAnchorMin = rectTransform.anchorMin;
            DefaultAnchorMax = rectTransform.anchorMax;
            DefaultPivot = rectTransform.pivot;
            defaultText = textComponent.text;
            DefaultFontSize = textComponent.fontSize;
            DefaultLineSpacing = textComponent.lineSpacing;
            DefaultFontAsset = textComponent.font;
            DefaultFont = textComponent.font.name;
            if (!FontCache.ContainsKey(textComponent.font.name))
            {
                FontCache.Add(textComponent.font.name, textComponent.font);
            }
        }

        public TextController Copy()
        {
            var c = Instantiate(gameObject, transform.parent).GetComponent<TextController>();
            c.IsPersistent = false;

            Controller[] children = GetChildren();
            int i = 0;
            foreach (Controller child in c.GetChildren())
            {
                child.IsPersistent = false;
                child.Start();
                child.CopyAllChannelsFrom(children[i]);
                i++;
            }

            c.Start();
            c.CopyAllChannelsFrom(this);
            Services.Scenecontrol.AddReferencedController(c);
            return c;
        }

        public void UpdateColor(Color color)
        {
            textComponent.color = color;
        }

        public void UpdatePosition(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            rectTransform.anchoredPosition3D = translation;
            rectTransform.localScale = scale;
            rectTransform.localRotation = rotation;
        }

        public void UpdateRect(float w, float h, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        }

        public void UpdateProperties(float fontSize, float lineSpacing, string font)
        {
            textComponent.lineSpacing = lineSpacing;
            textComponent.fontSize = Mathf.RoundToInt(fontSize);
            SetFont(font);
        }

        public void SetFont(string font)
        {
            if (font == "default")
            {
                textComponent.font = Services.Scenecontrol.DefaultFont;
            }

            if (font == textComponent.font.name)
            {
                return;
            }

            if (FontCache.ContainsKey(font))
            {
                textComponent.font = FontCache[font];
            }
            else
            {
                Font customFont = new Font(font);
                TMP_FontAsset customFontAsset = TMP_FontAsset.CreateFontAsset(customFont);
                textComponent.font = customFont != null ? customFontAsset : DefaultFontAsset;
            }

            return;
        }

        public void UpdateText(char[] text, int start, int length)
        {
            textComponent.SetCharArray(text, start, length);
        }
    }
}