using ArcCreate.Utility;
using ArcCreate.Utility.Animation;
using ArcCreate.Utility.ExternalAssets;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.SceneTransition
{
    public class TransitionScene : MonoBehaviour
    {
        [Header("Objects")]
        [SerializeField] private Image triangleTileImage;
        [SerializeField] private GameObject playRetryCountParent;
        [SerializeField] private GameObject infoParent;
        [SerializeField] private GameObject decorationParent;
        [SerializeField] private Canvas[] canvases;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip renderStartAudio;
        [SerializeField] private AudioClip enterGameplayAudio;
        [SerializeField] private AudioClip gameplayLoadCompleteAudio;
        [SerializeField] private AudioClip retryAudio;
        [SerializeField] private AudioClip generalTransitionAudio;

        [Header("TriangleTile")]
        [SerializeField] private Color preGreetingColor1;
        [SerializeField] private Color preGreetingColor2;
        [SerializeField] private Color greetingSceneColor1;
        [SerializeField] private Color greetingSceneColor2;
        [SerializeField] private ThemeGroup themeGroup;
        [SerializeField] private ThemeColor color1;
        [SerializeField] private ThemeColor color2;

        [Header("Animation")]
        [SerializeField] private ScriptedAnimator playRetryAnimator;
        [SerializeField] private ScriptedAnimator infoAnimator;
        [SerializeField] private ScriptedAnimator decorationAnimator;
        [SerializeField] private float greetingTriangleScale;
        [SerializeField] private float defaultTriangleScale;
        [SerializeField] private float zoomedTriangleScale;
        [SerializeField] private float colorAnimationDuration;
        [SerializeField] private Ease colorAnimationEase;
        [SerializeField] private float scaleAnimationDurationInOut;
        [SerializeField] private float scaleAnimationDurationOut;
        [SerializeField] private Ease scaleAnimationEaseInOut;
        [SerializeField] private Ease scaleAnimationEaseOut;
        private readonly int fromColor1ShaderId = Shader.PropertyToID("_FromColor1");
        private readonly int fromColor2ShaderId = Shader.PropertyToID("_FromColor2");
        private readonly int toColor1ShaderId = Shader.PropertyToID("_ToColor1");
        private readonly int toColor2ShaderId = Shader.PropertyToID("_ToColor2");
        private readonly int progressShaderId = Shader.PropertyToID("_Progress");
        private readonly int scaleShaderId = Shader.PropertyToID("_Scale");
        private Color lastColor1;
        private Color lastColor2;

        public enum Sound
        {
            RenderStart,
            EnterGameplay,
            GameplayLoadComplete,
            Retry,
            General,
        }

        public static TransitionScene Instance { get; private set; }

        public static ExternalAudioClip ExternalRenderStartAudio { get; set; }

        public static ExternalAudioClip ExternalEnterGameplayAudio { get; set; }

        public static ExternalAudioClip ExternalGameplayLoadCompleteAudio { get; set; }

        public static ExternalAudioClip ExternalGeneralTransitionAudio { get; set; }

        public static ExternalAudioClip ExternalRetryAudio { get; set; }

        public GameObject TriangleTileGameObject => triangleTileImage.gameObject;

        public GameObject PlayRetryCountGameObject => playRetryCountParent;

        public GameObject InfoGameObject => infoParent;

        public GameObject DecorationGameObject => decorationParent;

        public int PlayRetryCountAnimationDurationMs => (int)(playRetryAnimator.Length * 1000);

        public int InfoAnimationDurationMs => (int)(infoAnimator.Length * 1000);

        public int DecorationAnimationDurationMs => (int)(decorationAnimator.Length * 1000);

        public int TriangleTileAnimationDurationMs(bool inOutVariant)
            => (int)Mathf.Max(
                colorAnimationDuration,
                (inOutVariant ? scaleAnimationDurationInOut : scaleAnimationDurationOut) * 1000);

        public void EnterGreetingScene()
        {
            StopAllAnimations();
            AnimateTriangleTilesBetweenColors(preGreetingColor1, preGreetingColor2, greetingSceneColor1, greetingSceneColor2, true);
            triangleTileImage.material.SetFloat(scaleShaderId, greetingTriangleScale);
        }

        public void EnterSelectScene()
        {
            Theme theme = themeGroup.LastSelectedTheme;
            StopAllAnimations();
            AnimateTriangleTilesBetweenColors(greetingSceneColor1, greetingSceneColor2, color1.GetColor(theme), color2.GetColor(theme), true);
            AnimateTriangleTileScale(greetingTriangleScale, defaultTriangleScale, true);
        }

        public async UniTask ShowTriangleTile(bool inOutVariant)
        {
            StopAllAnimations();
            Color curr1 = triangleTileImage.material.GetColor(toColor1ShaderId);
            Color curr2 = triangleTileImage.material.GetColor(toColor2ShaderId);
            AnimateTriangleTilesBetweenColors(curr1, curr2, lastColor1, lastColor2, false);
            AnimateTriangleTileScale(defaultTriangleScale, zoomedTriangleScale, inOutVariant);
            await UniTask.Delay(TriangleTileAnimationDurationMs(inOutVariant));
        }

        public async UniTask HideTriangleTile(bool inOutVariant)
        {
            Color to1 = lastColor1;
            Color to2 = lastColor2;
            to1.a = 0;
            to2.a = 0;
            AnimateTriangleTilesBetweenColors(lastColor1, lastColor2, to1, to2, false);
            AnimateTriangleTileScale(zoomedTriangleScale, defaultTriangleScale, inOutVariant);
            await UniTask.Delay(TriangleTileAnimationDurationMs(inOutVariant));
        }

        public UniTask ShowPlayRetryCount() => ShowAnimation(playRetryAnimator);

        public UniTask HidePlayRetryCount() => HideAnimation(playRetryAnimator);

        public UniTask ShowInfo() => ShowAnimation(infoAnimator);

        public UniTask HideInfo() => HideAnimation(infoAnimator);

        public UniTask ShowDecoration() => ShowAnimation(decorationAnimator);

        public UniTask HideDecoration() => HideAnimation(decorationAnimator);

        public void PlaySoundEffect(Sound sound)
        {
            switch (sound)
            {
                case Sound.RenderStart:
                    audioSource.PlayOneShot(ExternalRenderStartAudio.Value);
                    return;
                case Sound.EnterGameplay:
                    audioSource.PlayOneShot(ExternalEnterGameplayAudio.Value);
                    return;
                case Sound.GameplayLoadComplete:
                    audioSource.PlayOneShot(ExternalGameplayLoadCompleteAudio.Value);
                    return;
                case Sound.Retry:
                    audioSource.PlayOneShot(ExternalRetryAudio.Value);
                    return;
                case Sound.General:
                    audioSource.PlayOneShot(ExternalGeneralTransitionAudio.Value);
                    return;
            }
        }

        public void SetTargetCamera(Camera camera, string layer = null, int order = 0)
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
                    canvas.sortingOrder = order;

                    // I don't care that this is duplicate code i just want to get out of this hell.
                    camera.fieldOfView = Mathf.Lerp(50, 65, ((camera.pixelHeight / (camera.pixelWidth / 16f)) - 9) / 3f);
                }
            }
        }

        private async UniTask ShowAnimation(ScriptedAnimator animator)
        {
            animator.Show();
            await UniTask.Delay((int)(animator.Length * 1000));
        }

        private async UniTask HideAnimation(ScriptedAnimator animator)
        {
            animator.Hide();
            await UniTask.Delay((int)(animator.Length * 1000));
        }

        private void Awake()
        {
            Instance = this;
            triangleTileImage.material = Instantiate(triangleTileImage.material);
            themeGroup.OnValueChange.AddListener(OnThemeChange);

            ExternalRenderStartAudio = new ExternalAudioClip(renderStartAudio, "AudioClips");
            ExternalEnterGameplayAudio = new ExternalAudioClip(enterGameplayAudio, "AudioClips");
            ExternalGameplayLoadCompleteAudio = new ExternalAudioClip(gameplayLoadCompleteAudio, "AudioClips");
            ExternalGeneralTransitionAudio = new ExternalAudioClip(generalTransitionAudio, "AudioClips");
            ExternalRetryAudio = new ExternalAudioClip(retryAudio, "AudioClips");

            ExternalRenderStartAudio.Load().Forget();
            ExternalEnterGameplayAudio.Load().Forget();
            ExternalGameplayLoadCompleteAudio.Load().Forget();
            ExternalGeneralTransitionAudio.Load().Forget();
            ExternalRetryAudio.Load().Forget();
        }

        private void OnDestroy()
        {
            if (triangleTileImage != null && triangleTileImage.material != null)
            {
                Destroy(triangleTileImage.material);
            }

            themeGroup.OnValueChange.RemoveListener(OnThemeChange);
            ExternalRenderStartAudio.Unload();
            ExternalEnterGameplayAudio.Unload();
            ExternalGameplayLoadCompleteAudio.Unload();
            ExternalGeneralTransitionAudio.Unload();
            ExternalRetryAudio.Unload();
        }

        private void OnThemeChange(Theme theme)
        {
            StopAllAnimations();
            AnimateTriangleTilesBetweenColors(lastColor1, lastColor2, color1.GetColor(theme), color2.GetColor(theme), true);
        }

        private void StopAllAnimations()
        {
            triangleTileImage.material.DOKill();
        }

        private void AnimateTriangleTilesBetweenColors(Color from1, Color from2, Color to1, Color to2, bool saveColor)
        {
            triangleTileImage.material.SetColor(fromColor1ShaderId, from1);
            triangleTileImage.material.SetColor(fromColor2ShaderId, from2);
            triangleTileImage.material.SetColor(toColor1ShaderId, to1);
            triangleTileImage.material.SetColor(toColor2ShaderId, to2);
            triangleTileImage.material.SetFloat(progressShaderId, 0);
            triangleTileImage.material.DOFloat(2, progressShaderId, colorAnimationDuration).SetEase(colorAnimationEase);
            if (saveColor)
            {
                lastColor1 = to1;
                lastColor2 = to2;
            }
        }

        private void AnimateTriangleTileScale(float fromScale, float toScale, bool inOutVariant)
        {
            triangleTileImage.material.SetFloat(scaleShaderId, fromScale);
            triangleTileImage.material
                .DOFloat(toScale, scaleShaderId, inOutVariant ? scaleAnimationDurationInOut : scaleAnimationDurationOut)
                .SetEase(inOutVariant ? scaleAnimationEaseInOut : scaleAnimationEaseOut);
        }
    }
}