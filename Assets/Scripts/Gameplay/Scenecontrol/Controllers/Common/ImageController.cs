using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for an image")]
    public class ImageController : Controller, IPositionController, IColorController, IRectController
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image image;

        [MoonSharpHidden] public Color DefaultColor { get; private set; }

        [MoonSharpHidden] public Vector3 DefaultTranslation { get; private set; } = Vector3.zero;

        [MoonSharpHidden] public Quaternion DefaultRotation { get; private set; } = Quaternion.identity;

        [MoonSharpHidden] public Vector3 DefaultScale { get; private set; } = Vector3.one;

        [MoonSharpHidden] public float DefaultRectW { get; private set; }

        [MoonSharpHidden] public float DefaultRectH { get; private set; }

        [MoonSharpHidden] public Vector2 DefaultAnchorMin { get; private set; }

        [MoonSharpHidden] public Vector2 DefaultAnchorMax { get; private set; }

        [MoonSharpHidden] public Vector2 DefaultPivot { get; private set; }

        public ValueChannel TranslationX { get; set; }

        public ValueChannel TranslationY { get; set; }

        public ValueChannel TranslationZ { get; set; }

        public ValueChannel RotationX { get; set; }

        public ValueChannel RotationY { get; set; }

        public ValueChannel RotationZ { get; set; }

        public ValueChannel ScaleX { get; set; }

        public ValueChannel ScaleY { get; set; }

        public ValueChannel ScaleZ { get; set; }

        public ValueChannel ColorR { get; set; }

        public ValueChannel ColorG { get; set; }

        public ValueChannel ColorB { get; set; }

        public ValueChannel ColorH { get; set; }

        public ValueChannel ColorS { get; set; }

        public ValueChannel ColorV { get; set; }

        public ValueChannel ColorA { get; set; }

        public ValueChannel RectW { get; set; }

        public ValueChannel RectH { get; set; }

        public ValueChannel AnchorMinX { get; set; }

        public ValueChannel AnchorMinY { get; set; }

        public ValueChannel AnchorMaxX { get; set; }

        public ValueChannel AnchorMaxY { get; set; }

        public ValueChannel PivotX { get; set; }

        public ValueChannel PivotY { get; set; }

        [MoonSharpHidden] public Image Image => image;

        [MoonSharpHidden]
        public override void SetupDefault()
        {
            DefaultColor = image.color;
            DefaultRectW = rectTransform.rect.width;
            DefaultRectH = rectTransform.rect.height;
            DefaultTranslation = rectTransform.anchoredPosition3D;
            DefaultRotation = rectTransform.localRotation;
            DefaultScale = rectTransform.localScale;
            DefaultAnchorMin = rectTransform.anchorMin;
            DefaultAnchorMax = rectTransform.anchorMax;
            DefaultPivot = rectTransform.pivot;
        }

        [EmmyDoc("Creates a copy of this controller")]
        public ImageController Copy()
        {
            var c = Instantiate(gameObject, transform.parent).GetComponent<ImageController>();
            c.image.material = Instantiate(c.image.material);
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
            image.color = color;
        }

        [MoonSharpHidden]
        public virtual void UpdatePosition(Vector3 translation, Quaternion rotation, Vector3 scale)
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
    }
}