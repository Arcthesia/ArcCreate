using EmmySharp;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for a text object")]
    public class TextController : Controller, IPositionController, IRectController, ITextController, IColorController
    {
        private string defaultText;
        private ValueChannel translationX;
        private ValueChannel translationY;
        private ValueChannel translationZ;
        private ValueChannel rotationX;
        private ValueChannel rotationY;
        private ValueChannel rotationZ;
        private ValueChannel scaleX;
        private ValueChannel scaleY;
        private ValueChannel scaleZ;
        private ValueChannel colorR;
        private ValueChannel colorG;
        private ValueChannel colorB;
        private ValueChannel colorH;
        private ValueChannel colorS;
        private ValueChannel colorV;
        private ValueChannel colorA;
        private ValueChannel rectW;
        private ValueChannel rectH;
        private ValueChannel anchorMinX;
        private ValueChannel anchorMinY;
        private ValueChannel anchorMaxX;
        private ValueChannel anchorMaxY;
        private ValueChannel pivotX;
        private ValueChannel pivotY;
        private TextChannel text;
        private ValueChannel fontSize;
        private ValueChannel lineSpacing;
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private RectTransform rectTransform;

        [MoonSharpHidden] public Vector3 DefaultTranslation { get; private set; }

        [MoonSharpHidden] public Quaternion DefaultRotation { get; private set; }

        [MoonSharpHidden] public Vector3 DefaultScale { get; private set; }

        [MoonSharpHidden] public float DefaultRectW { get; private set; }

        [MoonSharpHidden] public float DefaultRectH { get; private set; }

        [MoonSharpHidden] public Vector2 DefaultAnchorMin { get; private set; }

        [MoonSharpHidden] public Vector2 DefaultAnchorMax { get; private set; }

        [MoonSharpHidden] public Vector2 DefaultPivot { get; private set; }

        [MoonSharpHidden] public Color DefaultColor { get; private set; }

        [MoonSharpHidden] public float DefaultFontSize { get; private set; }

        [MoonSharpHidden] public float DefaultLineSpacing { get; private set; }

        [MoonSharpHidden] public TMP_FontAsset DefaultFontAsset { get; private set; }

        [MoonSharpHidden] public string DefaultFont { get; private set; }

        public ValueChannel TranslationX
        {
            get => translationX;
            set
            {
                translationX = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel TranslationY
        {
            get => translationY;
            set
            {
                translationY = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel TranslationZ
        {
            get => translationZ;
            set
            {
                translationZ = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel RotationX
        {
            get => rotationX;
            set
            {
                rotationX = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel RotationY
        {
            get => rotationY;
            set
            {
                rotationY = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel RotationZ
        {
            get => rotationZ;
            set
            {
                rotationZ = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel ScaleX
        {
            get => scaleX;
            set
            {
                scaleX = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel ScaleY
        {
            get => scaleY;
            set
            {
                scaleY = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel ScaleZ
        {
            get => scaleZ;
            set
            {
                scaleZ = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel RectW
        {
            get => rectW;
            set
            {
                rectW = value;
                EnableRectModule = true;
            }
        }

        public ValueChannel RectH
        {
            get => rectH;
            set
            {
                rectH = value;
                EnableRectModule = true;
            }
        }

        public ValueChannel AnchorMinX
        {
            get => anchorMinX;
            set
            {
                anchorMinX = value;
                EnableRectModule = true;
            }
        }

        public ValueChannel AnchorMinY
        {
            get => anchorMinY;
            set
            {
                anchorMinY = value;
                EnableRectModule = true;
            }
        }

        public ValueChannel AnchorMaxX
        {
            get => anchorMaxX;
            set
            {
                anchorMaxX = value;
                EnableRectModule = true;
            }
        }

        public ValueChannel AnchorMaxY
        {
            get => anchorMaxY;
            set
            {
                anchorMaxY = value;
                EnableRectModule = true;
            }
        }

        public ValueChannel PivotX
        {
            get => pivotX;
            set
            {
                pivotX = value;
                EnableRectModule = true;
            }
        }

        public ValueChannel PivotY
        {
            get => pivotY;
            set
            {
                pivotY = value;
                EnableRectModule = true;
            }
        }

        public TextChannel Text
        {
            get => text;
            set
            {
                text = value;
                EnableTextModule = true;
            }
        }

        public ValueChannel ColorR
        {
            get => colorR;
            set
            {
                colorR = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel ColorG
        {
            get => colorG;
            set
            {
                colorG = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel ColorB
        {
            get => colorB;
            set
            {
                colorB = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel ColorH
        {
            get => colorH;
            set
            {
                colorH = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel ColorS
        {
            get => colorS;
            set
            {
                colorS = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel ColorV
        {
            get => colorV;
            set
            {
                colorV = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel ColorA
        {
            get => colorA;
            set
            {
                colorA = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel FontSize
        {
            get => fontSize;
            set
            {
                fontSize = value;
                EnableTextModule = true;
            }
        }

        public ValueChannel LineSpacing
        {
            get => lineSpacing;
            set
            {
                lineSpacing = value;
                EnableTextModule = true;
            }
        }

        public string CustomFont { get; set; }

        [MoonSharpHidden] public virtual string DefaultText => defaultText;

        [MoonSharpHidden] public TMP_Text TextComponent => textComponent;

        public bool EnablePositionModule { get; set; }

        public bool EnableRectModule { get; set; }

        public bool EnableTextModule { get; set; }

        public bool EnableColorModule { get; set; }

        [MoonSharpHidden]
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
        }

        [EmmyDoc("Creates a copy of this controller")]
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

        [MoonSharpHidden]
        public void UpdateColor(Color color)
        {
            textComponent.color = color;
        }

        [MoonSharpHidden]
        public void UpdatePosition(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            rectTransform.anchoredPosition3D = translation;
            rectTransform.localScale = scale;
            rectTransform.localRotation = rotation;
        }

        [MoonSharpHidden]
        public void UpdateRect(float w, float h, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        }

        [MoonSharpHidden]
        public void UpdateProperties(float fontSize, float lineSpacing)
        {
            textComponent.lineSpacing = lineSpacing;
            textComponent.fontSize = Mathf.RoundToInt(fontSize);
        }

        [EmmyDoc("Change the font of this text object.")]
        public void SetFont(string font)
        {
            textComponent.font = Services.Scenecontrol.GetFont(font);
        }

        public void ApplyCustomFont(string font)
        {
            if (!string.IsNullOrEmpty(font))
            {
                SetFont(font);
            }
        }

        [MoonSharpHidden]
        public void UpdateText(char[] text, int start, int length)
        {
            textComponent.SetCharArray(text, start, length);
        }
    }
}