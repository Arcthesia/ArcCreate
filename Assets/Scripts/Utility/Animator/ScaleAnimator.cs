using ArcCreate.Utility.Extension;
using DG.Tweening;
using UnityEngine;

namespace ArcCreate.Utility.Animation
{
    [RequireComponent(typeof(RectTransform))]
    public class ScaleAnimator : ScriptedAnimatorComponent
    {
        [SerializeField] private Vector3 animationScaleMultiplier = new Vector3(1.3f, 1.3f, 1);
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease animationEase = Ease.OutCubic;
        [SerializeField] private Vector3 defaultScale;

        public override float AnimationLength => animationDuration;

        public override Tween GetShowTween()
        {
            transform.localScale = defaultScale.Multiply(animationScaleMultiplier);
            return transform.DOScale(defaultScale, animationDuration).SetEase(animationEase);
        }

        public override Tween GetHideTween()
        {
            transform.localScale = defaultScale;
            return transform.DOScale(defaultScale.Multiply(animationScaleMultiplier), animationDuration).SetEase(animationEase);
        }

        public override void RegisterDefaultValues()
        {
            defaultScale = transform.localScale;
        }

        public override void Reset()
        {
            transform.localScale = defaultScale;
        }

        public override void SetupComponents()
        {
        }
    }
}