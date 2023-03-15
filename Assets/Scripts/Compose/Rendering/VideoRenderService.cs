using System;
using System.IO;
using System.Threading;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using ArcCreate.SceneTransition;
using ArcCreate.Utility;
using ArcCreate.Utility.Parser;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Rendering
{
    [EditorScope("Render")]
    public class VideoRenderService : MonoBehaviour
    {
        public const string TimeSpanFormatString = @"hh\:mm\:ss";

        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private FileSelectField ffmpegPathField;
        [SerializeField] private TMP_InputField fromTimingField;
        [SerializeField] private TMP_InputField toTimingField;
        [SerializeField] private TMP_InputField qualityField;
        [SerializeField] private TMP_InputField fpsField;
        [SerializeField] private TMP_InputField widthField;
        [SerializeField] private TMP_InputField heightField;
        [SerializeField] private Toggle showShutterToggle;
        [SerializeField] private Button startButton;
        [SerializeField] private MarkerRange renderRangeMarker;

        [Header("Render in progress")]
        [SerializeField] private GameObject renderInProgressIndicator;
        [SerializeField] private RawImage renderPreview;
        [SerializeField] private TMP_Text renderStatusText;
        [SerializeField] private Button cancelButton;

        private CancellationTokenSource cts;

        private int from;
        private int to;

        [EditorAction("Start", false)]
        [SubAction("Cancel", false, "<esc>")]
        public async UniTask StartRender(EditorAction editorAction)
        {
            SubAction cancel = editorAction.GetSubAction("Cancel");

            string outputPath = Shell.SaveFileDialog(
                "Video",
                new string[] { "mp4" },
                "Render output",
                Path.GetDirectoryName(Services.Project.CurrentProject.Path),
                "render");

            if (string.IsNullOrEmpty(outputPath))
            {
                return;
            }

            AudioRenderer audioRenderer = new AudioRenderer(
                startTiming: from,
                endTiming: to,
                showShutter: showShutterToggle.isOn,
                audioOffset: gameplayData.AudioOffset.Value,
                songAudio: gameplayData.AudioClip.Value,
                tapAudio: Services.Gameplay.Audio.TapHitsoundClip,
                arcAudio: Services.Gameplay.Audio.ArcHitsoundClip,
                shutterCloseAudio: Shutter.ExternalStartAudio.Value,
                shutterOpenAudio: Shutter.ExternalOpenAudio.Value,
                sfxAudio: Services.Gameplay.Audio.SfxAudioClips);

            try
            {
                audioRenderer.CreateAudio();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }

            cts.Dispose();
            cts = new CancellationTokenSource();
            renderInProgressIndicator.SetActive(true);
            renderPreview.gameObject.SetActive(true);

            UniTask.WaitUntil(() => cancel.WasExecuted, cancellationToken: cts.Token)
                .ContinueWith(cts.Cancel).Forget();

            using (var renderer = new FrameRenderer(
                outputPath: outputPath,
                cameras: Services.Gameplay.Camera.RenderingCameras,
                width: Settings.RenderWidth.Value,
                height: Settings.RenderHeight.Value,
                fps: Settings.FPS.Value,
                crf: Settings.CRF.Value,
                from: from,
                to: to,
                showShutter: showShutterToggle.isOn,
                audioRenderer: audioRenderer))
            {
                renderPreview.texture = renderer.Texture2D;

                await renderer.RenderVideo(
                    cts.Token,
                    (TimeSpan elapsed, TimeSpan remaining)
                        => renderStatusText.text = I18n.S(
                            "Compose.UI.Export.Render.Status",
                            elapsed.ToString(TimeSpanFormatString),
                            remaining.ToString(TimeSpanFormatString)));
            }

            if (cts.IsCancellationRequested)
            {
                Services.Popups.Notify(Popups.Severity.Warning, I18n.S("Compose.Notify.Export.Render.Cancelled"));
            }
            else
            {
                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.Export.Render.Complete"));
            }

            renderInProgressIndicator.SetActive(false);
            renderPreview.gameObject.SetActive(false);
        }

        private void Awake()
        {
            ffmpegPathField.OnValueChanged += OnFFmpegPath;
            fromTimingField.onEndEdit.AddListener(OnTimingFields);
            toTimingField.onEndEdit.AddListener(OnTimingFields);
            renderRangeMarker.OnEndEdit += OnMarker;
            qualityField.onEndEdit.AddListener(OnQualityField);
            fpsField.onEndEdit.AddListener(OnFpsField);
            widthField.onEndEdit.AddListener(OnWidthField);
            heightField.onEndEdit.AddListener(OnHeightField);
            startButton.onClick.AddListener(OnStartRenderButton);
            gameplayData.AudioClip.OnValueChange += OnClipChange;
            cancelButton.onClick.AddListener(OnCancelRenderButton);

            ffmpegPathField.SetPathWithoutNotify(Settings.FFmpegPath.Value);
            fromTimingField.SetTextWithoutNotify("0");
            toTimingField.SetTextWithoutNotify("0");
            renderRangeMarker.SetTiming(0, 0);
            qualityField.SetTextWithoutNotify(Settings.CRF.Value.ToString());
            fpsField.SetTextWithoutNotify(Settings.FPS.Value.ToString());
            widthField.SetTextWithoutNotify(Settings.RenderWidth.Value.ToString());
            heightField.SetTextWithoutNotify(Settings.RenderHeight.Value.ToString());

            ffmpegPathField.AcceptedExtensions =
                Application.platform == RuntimePlatform.WindowsEditor
                || Application.platform == RuntimePlatform.WindowsPlayer ?
                new string[] { "exe" } : new string[0];
        }

        private void OnDestroy()
        {
            ffmpegPathField.OnValueChanged += OnFFmpegPath;
            fromTimingField.onEndEdit.RemoveListener(OnTimingFields);
            toTimingField.onEndEdit.RemoveListener(OnTimingFields);
            renderRangeMarker.OnEndEdit -= OnMarker;
            qualityField.onEndEdit.RemoveListener(OnQualityField);
            fpsField.onEndEdit.RemoveListener(OnFpsField);
            widthField.onEndEdit.RemoveListener(OnWidthField);
            heightField.onEndEdit.RemoveListener(OnHeightField);
            startButton.onClick.RemoveListener(OnStartRenderButton);
            gameplayData.AudioClip.OnValueChange -= OnClipChange;
            cancelButton.onClick.AddListener(OnCancelRenderButton);
            cts.Dispose();
        }

        private void OnEnable()
        {
            renderRangeMarker.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            renderRangeMarker.gameObject.SetActive(false);
        }

        private void OnFFmpegPath(FilePath path)
        {
            Settings.FFmpegPath.Value = path?.FullPath ?? "ffmpeg";
        }

        private void OnTimingFields(string val)
        {
            if (Evaluator.TryInt(fromTimingField.text, out int num1)
            && Evaluator.TryInt(toTimingField.text, out int num2))
            {
                from = Mathf.Min(num1, num2);
                to = Mathf.Max(num1, num2);
                from = Mathf.Max(from, 0);
                to = Mathf.Min(to, Mathf.RoundToInt((gameplayData.AudioClip.Value != null ? gameplayData.AudioClip.Value.length : 0) * 1000));
            }

            renderRangeMarker.SetTiming(from, to);
            fromTimingField.text = from.ToString();
            toTimingField.text = to.ToString();
            showShutterToggle.isOn = from <= 0;
        }

        private void OnMarker(int from, int to)
        {
            this.from = from;
            this.to = to;
            fromTimingField.text = from.ToString();
            toTimingField.text = to.ToString();
            showShutterToggle.isOn = from <= 0;
        }

        private void OnQualityField(string val)
        {
            if (Evaluator.TryInt(val, out int quality))
            {
                quality = Mathf.Clamp(quality, 0, 51);
                Settings.CRF.Value = quality;
            }

            qualityField.text = Settings.CRF.Value.ToString();
        }

        private void OnFpsField(string val)
        {
            if (Evaluator.TryFloat(val, out float fps))
            {
                fps = Mathf.Abs(fps);
                Settings.FPS.Value = fps;
            }

            fpsField.text = Settings.FPS.Value.ToString();
        }

        private void OnWidthField(string val)
        {
            if (Evaluator.TryInt(val, out int width))
            {
                width = Mathf.Abs(width);
                Settings.RenderWidth.Value = width;
            }

            widthField.text = Settings.RenderWidth.Value.ToString();
        }

        private void OnHeightField(string val)
        {
            if (Evaluator.TryInt(val, out int height))
            {
                height = Mathf.Abs(height);
                Settings.RenderHeight.Value = height;
            }

            heightField.text = Settings.RenderHeight.Value.ToString();
        }

        private void OnStartRenderButton()
        {
            Services.Navigation.StartAction("Render.Start");
        }

        private void OnCancelRenderButton()
        {
            cts.Cancel();
        }

        private void OnClipChange(AudioClip clip)
        {
            from = 0;
            to = Mathf.RoundToInt(clip != null ? clip.length * 1000 : 0);
            renderRangeMarker.SetTiming(from, to);
            fromTimingField.text = from.ToString();
            toTimingField.text = to.ToString();
        }
    }
}