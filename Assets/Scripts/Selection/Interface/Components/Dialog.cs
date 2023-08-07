using ArcCreate.Utility.Animation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class Dialog : MonoBehaviour
    {
        [SerializeField] private Button copyButton;
        [SerializeField] private Button hideButton;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private ScriptedAnimator animator;

        public void Show()
        {
            animator.Show();
        }

        public void Hide()
        {
            animator.Hide();
        }

        public void Copy()
        {
            GUIUtility.systemCopyBuffer = contentText.text;
        }

        protected virtual void Awake()
        {
            hideButton.onClick.AddListener(Hide);
            if (copyButton != null)
            {
                copyButton.onClick.AddListener(Copy);
            }
        }

        protected virtual void OnDestroy()
        {
            hideButton.onClick.RemoveListener(Hide);
            if (copyButton != null)
            {
                copyButton.onClick.RemoveListener(Copy);
            }
        }
    }
}