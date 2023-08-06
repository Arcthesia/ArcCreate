using ArcCreate.Utility.Animation;
using UnityEngine;

namespace ArcCreate.SceneTransition
{
    public class FitToContentText : ReactiveText
    {
        [SerializeField] private Vector2 padding;
        [SerializeField] private Vector2 maxSize;
        [SerializeField] private Vector2 minSize;
        [SerializeField] private RectTransform rect;
        [SerializeField] private RectSizeAnimator animator;

        protected override void OnTextChange(string text)
        {
            base.OnTextChange(text);
            float w = Mathf.Clamp(CachedText.preferredWidth + padding.x, minSize.x, maxSize.x);
            float h = Mathf.Clamp(CachedText.preferredHeight + padding.y, minSize.y, maxSize.y);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            rect.gameObject.SetActive(!string.IsNullOrEmpty(text));
            if (animator != null)
            {
                animator.SetDefaultSize(new Vector2(w, h));
            }
        }
    }
}