using DG.Tweening;
using UnityEngine;

namespace ArcCreate.Utility.Animation
{
    [RequireComponent(typeof(RectTransform))]
    public class PositionAnimator : ScriptedAnimatorComponent
    {
        [SerializeField] private Vector2 animationMoveVector = new Vector2(0, 300);
        [SerializeField] private float delay = 0;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease animationEase = Ease.OutCubic;
        [SerializeField] private Vector2 defaultPosition;
        private RectTransform rect;

        public override float AnimationLength => animationDuration + delay;

        public override Tween GetShowTween()
        {
            rect.anchoredPosition = defaultPosition - animationMoveVector;
            return rect.DOAnchorPos(defaultPosition, animationDuration).SetEase(animationEase).SetDelay(delay);
        }

        public override Tween GetHideTween()
        {
            rect.anchoredPosition = defaultPosition;
            return rect.DOAnchorPos(defaultPosition - animationMoveVector, animationDuration).SetEase(animationEase).SetDelay(delay);
        }

        public override void SetupComponents()
        {
            rect = GetComponent<RectTransform>();
        }

        public override void RegisterDefaultValues()
        {
            defaultPosition = rect.anchoredPosition;
        }

        public override void Reset()
        {
            rect.anchoredPosition = defaultPosition;
        }

        public override void HideImmediate()
        {
            rect.anchoredPosition = defaultPosition - animationMoveVector;
        }

        public override void ShowImmediate()
        {
            rect.anchoredPosition = defaultPosition;
        }
    }
}