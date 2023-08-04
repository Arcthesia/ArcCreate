using DG.Tweening;
using UnityEngine;

namespace ArcCreate.Utility.Animation
{
    public class ZAnimator : ScriptedAnimatorComponent
    {
        [SerializeField] private float animationMoveAmount = 100;
        [SerializeField] private float delay = 0;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease animationEase = Ease.OutCubic;
        [SerializeField] private float defaultZ;

        public override float AnimationLength => animationDuration + delay;

        public override Tween GetShowTween()
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, defaultZ - animationMoveAmount);
            return transform.DOLocalMoveZ(defaultZ, animationDuration).SetEase(animationEase).SetDelay(delay);
        }

        public override Tween GetHideTween()
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, defaultZ);
            return transform.DOLocalMoveZ(defaultZ - animationMoveAmount, animationDuration).SetEase(animationEase).SetDelay(delay);
        }

        public override void SetupComponents()
        {
        }

        public override void RegisterDefaultValues()
        {
        }

        public override void Reset()
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, defaultZ);
        }

        public override void HideImmediate()
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, defaultZ - animationMoveAmount);
        }

        public override void ShowImmediate()
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, defaultZ);
        }
    }
}