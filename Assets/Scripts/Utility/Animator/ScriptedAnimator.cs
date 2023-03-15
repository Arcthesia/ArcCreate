using DG.Tweening;
using UnityEngine;

namespace ArcCreate.Utility.Animation
{
    public class ScriptedAnimator : MonoBehaviour
    {
        [SerializeField] private ScriptedAnimatorComponent[] components = new ScriptedAnimatorComponent[0];
        [SerializeField] private bool disableGameObject = false;
        private float length = 0;
        private bool isSetup = false;

        public void Show()
        {
            if (disableGameObject)
            {
                gameObject.SetActive(true);
            }

            GetShowTween(out float _).Play();
        }

        public void Hide()
        {
            GetHideTween(out float _).Play().OnComplete(() =>
            {
                if (disableGameObject)
                {
                    gameObject.SetActive(false);
                }
            });
        }

        public void Reset()
        {
            if (!isSetup)
            {
                SetupComponents();
            }

            foreach (var c in components)
            {
                c.Reset();
            }
        }

        public void RegisterDefaultValues()
        {
            if (!isSetup)
            {
                SetupComponents();
            }

            foreach (var c in components)
            {
                c.RegisterDefaultValues();
            }
        }

        public Tween GetShowTween(out float duration)
        {
            if (!isSetup)
            {
                SetupComponents();
            }

            Sequence sequence = DOTween.Sequence();
            foreach (var c in components)
            {
                sequence = sequence.Join(c.GetShowTween());
            }

            duration = length;
            return sequence;
        }

        public Tween GetHideTween(out float duration)
        {
            if (!isSetup)
            {
                SetupComponents();
            }

            Sequence sequence = DOTween.Sequence();
            foreach (var c in components)
            {
                sequence = sequence.Join(c.GetHideTween());
            }

            duration = length;
            return sequence;
        }

        private void Awake()
        {
            SetupComponents();
        }

        private void SetupComponents()
        {
            foreach (var c in components)
            {
                c.SetupComponents();
                length = Mathf.Max(length, c.AnimationLength);
            }

            isSetup = true;
        }
    }
}