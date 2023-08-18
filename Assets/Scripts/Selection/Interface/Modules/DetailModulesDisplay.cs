using System;
using ArcCreate.Utility.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class DetailModulesDisplay : MonoBehaviour
    {
        [SerializeField] private Module[] modules;
        [SerializeField] private ScriptedAnimator jacketAnimator;
        [SerializeField] private Color buttonEnabledColor;
        [SerializeField] private Color buttonDisabledColor;
        private bool jacketAnimatorShown = true;

        private void Awake()
        {
            foreach (var module in modules)
            {
                module.Setup(this);
                module.Hide();
            }
        }

        private void OnDestroy()
        {
            foreach (var module in modules)
            {
                module.Teardown();
            }
        }

        private void ShowJacket()
        {
            jacketAnimator.Show();
            jacketAnimatorShown = true;
        }

        private void HideAllExcept(Module module)
        {
            if (jacketAnimatorShown)
            {
                jacketAnimator.Hide();
                jacketAnimatorShown = false;
            }

            foreach (var m in modules)
            {
                if (m.Animator != module.Animator)
                {
                    m.Hide();
                }
            }
        }

        [Serializable]
        private class Module
        {
            [SerializeField] private ScriptedAnimator animator;
            [SerializeField] private Button button;

            private DetailModulesDisplay parent;
            private bool isShown;

            public ScriptedAnimator Animator => animator;

            public Button Button => button;

            public void Setup(DetailModulesDisplay parent)
            {
                this.parent = parent;
                Button.onClick.AddListener(OnClick);
            }

            public void Teardown()
            {
                Button.onClick.RemoveListener(OnClick);
            }

            public void Show()
            {
                isShown = true;
                Animator.Show();
                Button.targetGraphic.color = parent.buttonEnabledColor;
            }

            public void Hide()
            {
                isShown = false;
                Animator.Hide();
                Button.targetGraphic.color = parent.buttonDisabledColor;
            }

            private void OnClick()
            {
                if (isShown)
                {
                    parent.ShowJacket();
                    Hide();
                }
                else
                {
                    parent.HideAllExcept(this);
                    Show();
                }
            }
        }
    }
}