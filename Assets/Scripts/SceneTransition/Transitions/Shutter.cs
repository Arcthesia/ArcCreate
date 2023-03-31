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

        [SerializeField] private Canvas[] canvases;

        [SerializeField] private AudioClip closeAudio;
        [SerializeField] private AudioClip openAudio;
        [SerializeField] private AudioClip startAudio;

        [SerializeField] private ScriptedAnimator animator;
        [SerializeField] private ScriptedAnimator shutterOnlyAnimator;

        public static ExternalAudioClip ExternalCloseAudio { get; set; }

        public static ExternalAudioClip ExternalOpenAudio { get; set; }

        public static ExternalAudioClip ExternalStartAudio { get; set; }

        public static Shutter Instance { get; private set; }

        public async UniTask Open(bool showInfo = false)
        {
            AudioSource.PlayClipAtPoint(ExternalOpenAudio.Value, default);

            if (showInfo)
            {
                animator.Hide();
            }
            else
            {
                shutterOnlyAnimator.Hide();
            }

            await UniTask.Delay(DurationMs);
        }

        public async UniTask Close(bool showInfo = false)
        {
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
            foreach (var canvas in canvases)
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
        }

        private void Awake()
        {
            Startup().Forget();
            Instance = this;
        }

        private async UniTask Startup()
        {
            ExternalCloseAudio = new ExternalAudioClip(closeAudio, "AudioClips");
            ExternalOpenAudio = new ExternalAudioClip(openAudio, "AudioClips");
            ExternalStartAudio = new ExternalAudioClip(startAudio, "AudioClips");

            await UniTask.WhenAll(ExternalCloseAudio.Load(), ExternalOpenAudio.Load(), ExternalStartAudio.Load());
            await UniTask.DelayFrame(2);
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            ExternalCloseAudio.Unload();
            ExternalOpenAudio.Unload();
            ExternalStartAudio.Unload();
        }
    }
}