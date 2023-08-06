using ArcCreate.Utility.Animation;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

namespace ArcCreate.SceneTransition
{
    public class FitCharterAndAliasText : MonoBehaviour
    {
        [SerializeField] private Vector2 padding;
        [SerializeField] private Vector2 maxSize;
        [SerializeField] private Vector2 minSize;
        [SerializeField] private float aliasLabelHeight;
        [SerializeField] private RectTransform rect;
        [SerializeField] private RectSizeAnimator animator;

        [SerializeField] private StringSO charterSO;
        [SerializeField] private StringSO aliasSO;

        [SerializeField] private TMP_Text charterText;
        [SerializeField] private TMP_Text aliasText;

        [SerializeField] private RectTransform charterRect;
        [SerializeField] private RectTransform aliasRect;

        protected void OnTextChange(string text)
        {
            if (string.IsNullOrEmpty(charterSO.Value))
            {
                rect.gameObject.SetActive(false);
                return;
            }

            rect.gameObject.SetActive(true);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxSize.x);

            charterText.text = charterSO.Value;
            float charterWidth = Mathf.Min(charterText.preferredWidth, maxSize.x - padding.x);

            float aliasWidth = 0;
            if (string.IsNullOrEmpty(aliasSO.Value))
            {
                aliasRect.gameObject.SetActive(false);
            }
            else
            {
                aliasRect.gameObject.SetActive(true);
                aliasText.text = aliasSO.Value;
                aliasWidth = Mathf.Min(aliasText.preferredWidth, maxSize.x - padding.x);
            }

            float w = Mathf.Max(charterWidth, aliasWidth);
            w = Mathf.Clamp(w, minSize.x, maxSize.x);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);

            charterText.text = charterSO.Value;
            aliasText.text = aliasSO.Value ?? string.Empty;

            float h = charterText.preferredHeight + (string.IsNullOrEmpty(aliasSO.Value) ? 0 : aliasText.preferredHeight + aliasLabelHeight) + padding.y;
            h = Mathf.Clamp(h, minSize.y, maxSize.y);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            aliasRect.offsetMax = new Vector2(aliasRect.offsetMax.x, -charterText.preferredHeight);

            if (animator != null)
            {
                animator.SetDefaultSize(new Vector2(w, h));
            }
        }

        private void Awake()
        {
            charterSO.OnValueChange.AddListener(OnTextChange);
            aliasSO.OnValueChange.AddListener(OnTextChange);
            OnTextChange(string.Empty);
        }

        private void OnDestroy()
        {
            charterSO.OnValueChange.RemoveListener(OnTextChange);
            aliasSO.OnValueChange.RemoveListener(OnTextChange);
        }
    }
}