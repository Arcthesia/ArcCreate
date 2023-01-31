using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Popups
{
    public enum Severity
    {
        Info,
        Warning,
        Error,
    }

    public class Notification : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject infoIcon;
        [SerializeField] private Color infoColor;
        [SerializeField] private GameObject warningIcon;
        [SerializeField] private Color warningColor;
        [SerializeField] private GameObject errorIcon;
        [SerializeField] private Color errorColor;
        [SerializeField] private Image background;
        [SerializeField] private Image hoverBackground;
        [SerializeField] private TMP_Text mainText;
        [SerializeField] private TMP_Text fullText;
        [SerializeField] private float padding = 3;
        [SerializeField] private RectTransform showOnHover;
        [SerializeField] private CanvasGroup canvasGroup;

        private const float FadeDuration = 1;
        private const float FadeDelay = 10;
        private RectTransform mainTextRect;

        private bool Overflow =>
            mainText.preferredWidth > mainTextRect.rect.width
            || mainText.preferredHeight > mainTextRect.rect.height;

        private Severity Severity
        {
            set
            {
                infoIcon.SetActive(false);
                warningIcon.SetActive(false);
                errorIcon.SetActive(false);

                switch (value)
                {
                    case Severity.Info:
                        infoIcon.SetActive(true);
                        background.color = infoColor;
                        hoverBackground.color = infoColor;
                        break;
                    case Severity.Warning:
                        warningIcon.SetActive(true);
                        background.color = warningColor;
                        hoverBackground.color = warningColor;
                        break;
                    case Severity.Error:
                        errorIcon.SetActive(true);
                        background.color = errorColor;
                        hoverBackground.color = errorColor;
                        break;
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Overflow)
            {
                showOnHover.gameObject.SetActive(true);
                canvasGroup.alpha = 1;
                canvasGroup.DOKill();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            showOnHover.gameObject.SetActive(false);
            canvasGroup.alpha = 1;
            canvasGroup.DOKill();
            canvasGroup.DOFade(0, FadeDuration).SetDelay(FadeDelay);
        }

        public void SetContent(Severity severity, string text)
        {
            gameObject.SetActive(true);
            Severity = severity;
            mainText.text = text;
            fullText.text = text;

            // i love unity
            showOnHover.gameObject.SetActive(true);
            showOnHover.sizeDelta = new Vector2(
                showOnHover.sizeDelta.x,
                fullText.preferredHeight + (padding * 2));
            showOnHover.gameObject.SetActive(false);

            canvasGroup.alpha = 1;
            canvasGroup.DOKill();
            canvasGroup.DOFade(0, FadeDuration).SetDelay(FadeDelay)
                .OnComplete(OnFadeComplete);
        }

        private void OnFadeComplete()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            gameObject.SetActive(false);
            mainText.text = string.Empty;
            fullText.text = string.Empty;
            mainTextRect = mainText.GetComponent<RectTransform>();
            EasterEggs.AddAffectedByColor(background);
        }
    }
}