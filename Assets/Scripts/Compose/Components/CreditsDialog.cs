using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class CreditsDialog : Dialog
    {
        [SerializeField] private RectTransform contentRect;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Button closeButton;
        [SerializeField] private Vector3 animationStartScale;
        [SerializeField] private float animationScaleDuration;
        [SerializeField] private float animationAlphaDuration;
        [SerializeField] private Ease animationScaleEasing;
        [SerializeField] private Ease animationAlphaEasing;

        public override void Open()
        {
            base.Open();
            contentRect.localScale = animationStartScale;
            contentRect.DOScale(Vector3.one, animationScaleDuration).SetEase(animationScaleEasing);
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, animationAlphaDuration).SetEase(animationAlphaEasing);
        }

        public override void Close()
        {
            contentRect.DOScale(animationStartScale, animationScaleDuration).SetEase(animationScaleEasing);
            canvasGroup.DOFade(0, animationAlphaDuration).SetEase(animationAlphaEasing)
                .OnComplete(base.Close);
        }

        private void Awake()
        {
            closeButton.onClick.AddListener(Close);
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }
}