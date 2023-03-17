using DG.Tweening;
using UnityEngine;

namespace ArcCreate.Utility.Animation
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeAnimator : ScriptedAnimatorComponent
    {
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease animationEase = Ease.OutCubic;
        [SerializeField] private float defaultAlpha;
        private CanvasGroup canvasGroup;

        public override float AnimationLength => animationDuration;

        public override Tween GetShowTween()
        {
            canvasGroup.alpha = 0;
            return canvasGroup.DOFade(defaultAlpha, animationDuration).SetEase(animationEase);
        }

        public override Tween GetHideTween()
        {
            canvasGroup.alpha = defaultAlpha;
            return canvasGroup.DOFade(0, animationDuration).SetEase(animationEase);
        }

        public override void SetupComponents()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void RegisterDefaultValues()
        {
            defaultAlpha = canvasGroup.alpha;
        }

        public override void Reset()
        {
            canvasGroup.alpha = defaultAlpha;
        }

        public override void HideImmediate()
        {
            canvasGroup.alpha = 0;
        }

        public override void ShowImmediate()
        {
            canvasGroup.alpha = defaultAlpha;
        }
    }
}