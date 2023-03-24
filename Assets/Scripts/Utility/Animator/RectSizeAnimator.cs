using ArcCreate.Utility.Extension;
using DG.Tweening;
using UnityEngine;

namespace ArcCreate.Utility.Animation
{
    [RequireComponent(typeof(RectTransform))]
    public class RectSizeAnimator : ScriptedAnimatorComponent
    {
        [SerializeField] private Vector2 animationSizeMultiplier = new Vector3(1.3f, 1.3f, 1);
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease animationEase = Ease.OutCubic;
        [SerializeField] private Vector2 defaultSize;
        private RectTransform rect;

        public override float AnimationLength => animationDuration;

        public override Tween GetShowTween()
        {
            rect.sizeDelta = defaultSize.Multiply(animationSizeMultiplier);
            return rect.DOSizeDelta(defaultSize, animationDuration).SetEase(animationEase);
        }

        public override Tween GetHideTween()
        {
            rect.sizeDelta = defaultSize;
            return rect.DOSizeDelta(defaultSize.Multiply(animationSizeMultiplier), animationDuration).SetEase(animationEase);
        }

        public override void RegisterDefaultValues()
        {
            defaultSize = rect.sizeDelta;
        }

        public override void Reset()
        {
            rect.sizeDelta = defaultSize;
        }

        public override void SetupComponents()
        {
            rect = GetComponent<RectTransform>();
        }

        public override void HideImmediate()
        {
            rect.sizeDelta = defaultSize.Multiply(animationSizeMultiplier);
        }

        public override void ShowImmediate()
        {
            rect.sizeDelta = defaultSize;
        }
    }
}