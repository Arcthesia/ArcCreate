using System;
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

        public float Length => length;

        public bool IsShown { get; private set; }

        public void ShowImmediate()
        {
            foreach (var c in components)
            {
                c.ShowImmediate();
            }
        }

        public void HideImmediate()
        {
            foreach (var c in components)
            {
                c.HideImmediate();
            }
        }

        public void Show()
        {
            if (disableGameObject)
            {
                gameObject.SetActive(true);
            }

            IsShown = true;
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

                IsShown = false;
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
                sequence = sequence.Insert(0, c.GetShowTween());
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
                sequence = sequence.Insert(0, c.GetHideTween());
            }

            duration = length;
            return sequence;
        }

        public void SetupComponents()
        {
            foreach (var c in components)
            {
                c.SetupComponents();
                length = Mathf.Max(length, c.AnimationLength);
            }

            isSetup = true;
        }

        private void Awake()
        {
            SetupComponents();
        }
    }
}