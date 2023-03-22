using ArcCreate.Utility.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class Dialog : MonoBehaviour
    {
        [SerializeField] private Button hideButton;
        [SerializeField] private ScriptedAnimator animator;

        public void Show()
        {
            animator.Show();
        }

        public void Hide()
        {
            animator.Hide();
        }

        protected virtual void Awake()
        {
            hideButton.onClick.AddListener(Hide);
        }

        protected virtual void OnDestroy()
        {
            hideButton.onClick.RemoveListener(Hide);
        }
    }
}