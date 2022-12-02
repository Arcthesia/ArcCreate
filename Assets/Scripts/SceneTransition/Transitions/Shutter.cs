using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.SceneTransition
{
    public class Shutter : MonoBehaviour
    {
        public const int DurationMs = 650;
        public const int WaitBetween = 5000;
        public const float FullSequence = (2 * DurationMs) + WaitBetween;

        [SerializeField] private RectTransform left;
        [SerializeField] private RectTransform right;
        [SerializeField] private RectTransform shadow;
        [SerializeField] private Image shadowImage;
        [SerializeField] private RectTransform jacket;
        [SerializeField] private CanvasGroup jacketCanvasGroup;
        [SerializeField] private Image titleBack;
        [SerializeField] private RectTransform info;
        [SerializeField] private CanvasGroup infoCanvasGroup;
        [SerializeField] private Sprite defaultJacket;
        [SerializeField] private Sprite defaultJacketShadow;
        [SerializeField] private Image jacketImage;
        [SerializeField] private Image jacketShadow;
        [SerializeField] private Text title;
        [SerializeField] private Text composer;
        [SerializeField] private Text illustrator;
        [SerializeField] private GameObject illustratorLabel;
        [SerializeField] private Text charter;
        [SerializeField] private GameObject charterLabel;
        [SerializeField] private AudioClip closeAudio;
        [SerializeField] private AudioClip openAudio;
        [SerializeField] private AudioClip startAudio;

        public async UniTask Open()
        {
            // TODO: MAGIC NUMBER
            left.DOAnchorPosX(-1216, DurationMs).SetEase(Ease.InCubic);
            right.DOAnchorPosX(425, DurationMs).SetEase(Ease.InCubic);
            shadow.DOAnchorPosX(-576, DurationMs).SetEase(Ease.InCubic);
            shadowImage.DOFade(0, DurationMs).SetEase(Ease.InCubic);
            titleBack.DOFade(0, DurationMs).SetEase(Ease.InCubic);
            jacket.DOScale(new Vector2(1.3f, 1.3f), DurationMs).SetEase(Ease.InCubic);
            jacketCanvasGroup.DOFade(0, DurationMs).SetEase(Ease.InCubic);
            infoCanvasGroup.DOFade(0, DurationMs).SetEase(Ease.InCubic);

            AudioSource.PlayClipAtPoint(openAudio, default);

            await UniTask.Delay(DurationMs);

            info.anchoredPosition = new Vector3(490, info.anchoredPosition.y, 0);
            shadow.anchoredPosition = new Vector3(597, shadow.anchoredPosition.y, 0);
        }

        public async UniTask Close(bool showInfo = false)
        {
            // TODO: MAGIC NUMBER
            left.DOAnchorPosX(0, DurationMs).SetEase(Ease.OutCubic);
            right.DOAnchorPosX(0, DurationMs).SetEase(Ease.OutCubic);

            if (showInfo)
            {
                AudioSource.PlayClipAtPoint(startAudio, default);
            }
            else
            {
                AudioSource.PlayClipAtPoint(closeAudio, default);
            }

            if (showInfo)
            {
                shadowImage.DOFade(1, DurationMs * 0.9f).SetEase(Ease.OutCubic);
                titleBack.DOFade(0.4f, DurationMs * 0.2f).SetEase(Ease.OutCubic);

                jacket.DOScale(new Vector2(0.95f, 0.95f), DurationMs * 0.9f).SetEase(Ease.OutCubic);
                jacketCanvasGroup.DOFade(1, DurationMs * 0.9f).SetEase(Ease.OutCubic);

                info.DOAnchorPosX(640, DurationMs * 0.9f).SetEase(Ease.OutCubic);
                infoCanvasGroup.DOFade(1, DurationMs * 0.9f).SetEase(Ease.OutCubic);
            }

            await UniTask.Delay(DurationMs);
        }
    }
}