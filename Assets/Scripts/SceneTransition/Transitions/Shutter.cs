using System;
using ArcCreate.Utilities.ExternalAssets;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.SceneTransition
{
    public class Shutter : MonoBehaviour
    {
        public const int DurationMs = 650;
        public const int WaitBetweenMs = 5000;
        public const int FullSequenceMs = (2 * DurationMs) + WaitBetweenMs;
        public const float DurationSeconds = DurationMs / 1000f;
        public const float WaitBetweenSeconds = WaitBetweenMs / 1000f;
        public const float FullSequenceSeconds = FullSequenceMs / 1000f;

        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform left;
        [SerializeField] private RectTransform right;
        [SerializeField] private RectTransform shadow;
        [SerializeField] private Image shadowImage;
        [SerializeField] private RectTransform jacket;
        [SerializeField] private CanvasGroup jacketCanvasGroup;
        [SerializeField] private Image titleBack;
        [SerializeField] private RectTransform info;
        [SerializeField] private CanvasGroup infoCanvasGroup;

        [SerializeField] private AudioClip closeAudio;
        [SerializeField] private AudioClip openAudio;
        [SerializeField] private AudioClip startAudio;

        public static ExternalAudioClip ExternalCloseAudio { get; set; }

        public static ExternalAudioClip ExternalOpenAudio { get; set; }

        public static ExternalAudioClip ExternalStartAudio { get; set; }

        public static Shutter Instance { get; private set; }

        public async UniTask Open()
        {
            // TODO: MAGIC NUMBER
            left.DOAnchorPosX(-1216, DurationSeconds).SetEase(Ease.InCubic);
            right.DOAnchorPosX(425, DurationSeconds).SetEase(Ease.InCubic);
            shadow.DOAnchorPosX(-576, DurationSeconds).SetEase(Ease.InCubic);
            shadowImage.DOFade(0, DurationSeconds).SetEase(Ease.InCubic);
            titleBack.DOFade(0, DurationSeconds).SetEase(Ease.InCubic);
            jacket.DOScale(new Vector2(1.3f, 1.3f), DurationSeconds).SetEase(Ease.InCubic);
            jacketCanvasGroup.DOFade(0, DurationSeconds).SetEase(Ease.InCubic);
            infoCanvasGroup.DOFade(0, DurationSeconds).SetEase(Ease.InCubic);

            AudioSource.PlayClipAtPoint(ExternalOpenAudio.Value, default);

            await UniTask.Delay(DurationMs);

            info.anchoredPosition = new Vector3(490, info.anchoredPosition.y, 0);
            shadow.anchoredPosition = new Vector3(597, shadow.anchoredPosition.y, 0);
        }

        public async UniTask Close(bool showInfo = false)
        {
            // TODO: MAGIC NUMBER
            left.DOAnchorPosX(0, DurationSeconds).SetEase(Ease.OutCubic);
            right.DOAnchorPosX(0, DurationSeconds).SetEase(Ease.OutCubic);

            if (showInfo)
            {
                AudioSource.PlayClipAtPoint(ExternalStartAudio.Value, default);
            }
            else
            {
                AudioSource.PlayClipAtPoint(ExternalCloseAudio.Value, default);
            }

            if (showInfo)
            {
                shadowImage.DOFade(1, DurationSeconds * 0.9f).SetEase(Ease.OutCubic);
                titleBack.DOFade(0.4f, DurationSeconds * 0.2f).SetEase(Ease.OutCubic);

                jacket.DOScale(new Vector2(0.95f, 0.95f), DurationSeconds * 0.9f).SetEase(Ease.OutCubic);
                jacketCanvasGroup.DOFade(1, DurationSeconds * 0.9f).SetEase(Ease.OutCubic);

                info.DOAnchorPosX(640, DurationSeconds * 0.9f).SetEase(Ease.OutCubic);
                infoCanvasGroup.DOFade(1, DurationSeconds * 0.9f).SetEase(Ease.OutCubic);
            }

            await UniTask.Delay(DurationMs);
        }

        public void SetTargetCamera(Camera camera, string layer = null)
        {
            if (camera == null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.sortingLayerName = layer;
            }
        }

        private void Awake()
        {
            StartLoadingExternalAudio().Forget();
            gameObject.SetActive(false);
            Instance = this;
        }

        private async UniTask StartLoadingExternalAudio()
        {
            ExternalCloseAudio = new ExternalAudioClip(closeAudio, "AudioClips");
            ExternalOpenAudio = new ExternalAudioClip(openAudio, "AudioClips");
            ExternalStartAudio = new ExternalAudioClip(startAudio, "AudioClips");

            await UniTask.WhenAll(ExternalCloseAudio.Load(), ExternalOpenAudio.Load(), ExternalStartAudio.Load());
        }

        private void OnDestroy()
        {
            ExternalCloseAudio.Unload();
            ExternalOpenAudio.Unload();
            ExternalStartAudio.Unload();
        }
    }
}