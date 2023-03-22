using ArcCreate.Utility.Animation;
using ArcCreate.Utility.ExternalAssets;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

        [SerializeField] private AudioClip closeAudio;
        [SerializeField] private AudioClip openAudio;
        [SerializeField] private AudioClip startAudio;

        [SerializeField] private ScriptedAnimator animator;
        [SerializeField] private ScriptedAnimator shutterOnlyAnimator;

        public static ExternalAudioClip ExternalCloseAudio { get; set; }

        public static ExternalAudioClip ExternalOpenAudio { get; set; }

        public static ExternalAudioClip ExternalStartAudio { get; set; }

        public static Shutter Instance { get; private set; }

        public async UniTask Open()
        {
            AudioSource.PlayClipAtPoint(ExternalOpenAudio.Value, default);

            await UniTask.Delay(DurationMs);

            animator.Hide();
        }

        public async UniTask Close(bool showInfo = false)
        {
            // TODO: MAGIC NUMBER
            if (showInfo)
            {
                AudioSource.PlayClipAtPoint(ExternalStartAudio.Value, default);
                animator.Show();
            }
            else
            {
                AudioSource.PlayClipAtPoint(ExternalCloseAudio.Value, default);
                shutterOnlyAnimator.Show();
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