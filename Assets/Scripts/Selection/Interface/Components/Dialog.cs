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

        private void Awake()
        {
            hideButton.onClick.AddListener(Hide);
        }

        private void OnDestroy()
        {
            hideButton.onClick.RemoveListener(Hide);
        }
    }
}