using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for a canvas")]
    public class CanvasController : Controller, ILayerController, IPositionController, IRectController
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform rectTransform;

        [MoonSharpHidden] public Vector3 DefaultTranslation { get; private set; }

        [MoonSharpHidden] public Quaternion DefaultRotation { get; private set; }

        [MoonSharpHidden] public Vector3 DefaultScale { get; private set; }

        [MoonSharpHidden] public string DefaultLayer { get; private set; }

        [MoonSharpHidden] public int DefaultSort { get; private set; }

        [MoonSharpHidden] public float DefaultAlpha { get; private set; }

        [MoonSharpHidden] public float DefaultRectW { get; private set; }

        [MoonSharpHidden] public float DefaultRectH { get; private set; }

        [MoonSharpHidden] public Vector2 DefaultAnchorMin { get; private set; }

        [MoonSharpHidden] public Vector2 DefaultAnchorMax { get; private set; }

        [MoonSharpHidden] public Vector2 DefaultPivot { get; private set; }

        public StringChannel Layer { get; set; }

        public ValueChannel Sort { get; set; }

        public ValueChannel Alpha { get; set; }

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

        [MoonSharpHidden] public Canvas Canvas => canvas;

        [MoonSharpHidden]
        public override void SetupDefault()
        {
            DefaultLayer = canvas.sortingLayerName;
            DefaultSort = canvas.sortingOrder;
            DefaultAlpha = canvasGroup.alpha;
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
        public CanvasController Copy()
        {
            var c = Instantiate(gameObject, transform.parent).GetComponent<CanvasController>();
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
        public void UpdateLayer(string layer, int sort, float alpha)
        {
            canvas.sortingLayerName = layer;
            canvas.sortingOrder = sort;
            canvasGroup.alpha = alpha;
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
    }
}