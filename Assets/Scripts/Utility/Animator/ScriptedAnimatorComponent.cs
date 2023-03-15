using DG.Tweening;
using UnityEngine;

namespace ArcCreate.Utility.Animation
{
    public abstract class ScriptedAnimatorComponent : MonoBehaviour
    {
        public abstract float AnimationLength { get; }

        public abstract Tween GetShowTween();

        public abstract Tween GetHideTween();

        public abstract void SetupComponents();

        public abstract void RegisterDefaultValues();

        public abstract void Reset();
    }
}