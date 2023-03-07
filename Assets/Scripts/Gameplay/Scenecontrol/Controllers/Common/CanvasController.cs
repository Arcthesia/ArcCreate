using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for a canvas")]
    public class CanvasController : Controller, ILayerController, IPositionController, IRectController
    {
        private StringChannel layer;
        private ValueChannel sort;
        private ValueChannel alpha;
        private ValueChannel translationX;
        private ValueChannel translationY;
        private ValueChannel translationZ;
        private ValueChannel rotationX;
        private ValueChannel rotationY;
        private ValueChannel rotationZ;
        private ValueChannel scaleX;
        private ValueChannel scaleY;
        private ValueChannel scaleZ;
        private ValueChannel rectW;
        private ValueChannel rectH;
        private ValueChannel anchorMinX;
        private ValueChannel anchorMinY;
        private ValueChannel anchorMaxY;
        private ValueChannel pivotX;
        private ValueChannel pivotY;
        private ValueChannel anchorMaxX;
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

        public StringChannel Layer
        {
            get => layer;
            set
            {
                layer = value;
                EnableLayerModule = true;
            }
        }

        public ValueChannel Sort
        {
            get => sort;
            set
            {
                sort = value;
                EnableLayerModule = true;
            }
        }

        public ValueChannel Alpha
        {
            get => alpha;
            set
            {
                alpha = value;
                EnableLayerModule = true;
            }
        }

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

        [MoonSharpHidden] public Canvas Canvas => canvas;

        [MoonSharpHidden] public RectTransform RectTransform => rectTransform;

        public bool EnableLayerModule { get; set; }

        public bool EnablePositionModule { get; set; }

        public bool EnableRectModule { get; set; }

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
        public virtual void UpdateRect(float w, float h, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        }
    }
}