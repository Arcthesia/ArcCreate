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
        [SerializeField] private CrfField crfField;
        [SerializeField] private TMP_InputField fpsField;
        [SerializeField] private TMP_InputField widthField;
        [SerializeField] private TMP_InputField heightField;
        [SerializeField] private TMP_InputField musicVolumeField;
        [SerializeField] private TMP_InputField effectVolumeField;
        [SerializeField] private Toggle showShutterToggle;
        [SerializeField] private Button startButton;
        [SerializeField] private MarkerRange renderRangeMarker;
        [SerializeField] private GameplayViewport gameplayViewport;
        [SerializeField] private StringSO retryCount;
        [SerializeField] private StringSO playCount;
        [SerializeField] private OptionsPanel presetPanel;
        [SerializeField] private Button resetSelectedPreset;

        [Header("Render in progress")]
        [SerializeField] private GameObject renderInProgressIndicator;
        [SerializeField] private RawImage renderPreview;
        [SerializeField] private TMP_Text renderStatusText;
        [SerializeField] private Button cancelButton;

        private CancellationTokenSource cts = new CancellationTokenSource();

        private int from;
        private int to;

        [EditorAction("Start", false)]
        [SubAction("Cancel", false, "<esc>")]
        [KeybindHint(Priority = KeybindPriorities.SubCancel)]
        public async UniTask StartRender(EditorAction editorAction)
        {
            Services.Gameplay.Audio.Pause();
            SubAction cancel = editorAction.GetSubAction("Cancel");
            TransitionSequence transitionSequence = new TransitionSequence()
                .OnShow()
                .AddTransition(new TriangleTileTransition())
                .AddTransition(new DecorationTransition())
                .AddTransition(new InfoTransition())
                .OnHide()
                .AddTransition(new InfoTransition())
                .AddTransitionReversed(new PlayRetryCountTransition())
                .AddTransition(new PlayRetryCountTransition(), 1200)
                .AddTransition(new TriangleTileTransition(), 1200)
                .AddTransition(new DecorationTransition(), 1200)
                .SetWaitDuration(2000);

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
                settings: RenderSetting.Current,
                showTransition: showShutterToggle.isOn,
                audioOffset: gameplayData.AudioOffset.Value,
                songAudio: gameplayData.AudioClip.Value,
                tapAudio: Services.Gameplay.Audio.TapHitsoundClip,
                arcAudio: Services.Gameplay.Audio.ArcHitsoundClip,
                renderStartAudio: TransitionScene.ExternalRenderStartAudio.Value,
                gameplayLoadCompleteAudio: TransitionScene.ExternalGameplayLoadCompleteAudio.Value,
                sfxAudio: Services.Gameplay.Audio.SfxAudioClips,
                transitionSequence: transitionSequence);

            try
            {
                audioRenderer.CreateAudio();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }

            playCount.Value = "AUTOPLAY";
            retryCount.Value = string.Empty;
            cts.Dispose();
            cts = new CancellationTokenSource();
            renderInProgressIndicator.SetActive(true);
            renderPreview.gameObject.SetActive(true);

            UniTask.WaitUntil(() => cancel.WasExecuted, cancellationToken: cts.Token)
                .ContinueWith(cts.Cancel).Forget();

            using (var renderer = new FrameRenderer(
                outputPath: outputPath,
                cameras: Services.Gameplay.Camera.RenderingCameras,
                settings: RenderSetting.Current,
                from: from,
                to: to,
                showShutter: showShutterToggle.isOn,
                audioRenderer: audioRenderer,
                gameplayViewport: gameplayViewport,
                transitionSequence: transitionSequence))
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
            Time.timeScale = 1;

            ffmpegPathField.OnValueChanged += OnFFmpegPath;
            fromTimingField.onEndEdit.AddListener(OnTimingFields);
            toTimingField.onEndEdit.AddListener(OnTimingFields);
            renderRangeMarker.OnEndEdit += OnMarker;
            crfField.OnValueChanged.AddListener(OnQualityField);
            fpsField.onEndEdit.AddListener(OnFpsField);
            widthField.onEndEdit.AddListener(OnWidthField);
            heightField.onEndEdit.AddListener(OnHeightField);
            musicVolumeField.onEndEdit.AddListener(OnMusicVolumeField);
            effectVolumeField.onEndEdit.AddListener(OnEffectVolumeField);
            startButton.onClick.AddListener(OnStartRenderButton);
            gameplayData.AudioClip.OnValueChange += OnClipChange;
            cancelButton.onClick.AddListener(OnCancelRenderButton);

            RenderSetting.LoadSettings();
            ffmpegPathField.SetPathWithoutNotify(Settings.FFmpegPath.Value);
            fromTimingField.SetTextWithoutNotify("0");
            toTimingField.SetTextWithoutNotify("0");
            renderRangeMarker.SetTiming(0, 0);
            UpdateRenderSettingFields();

            ffmpegPathField.AcceptedExtensions =
                Application.platform == RuntimePlatform.WindowsEditor
                || Application.platform == RuntimePlatform.WindowsPlayer ?
                new string[] { "exe" } : new string[0];

            presetPanel.OnSelect += OnPresetSelect;
            resetSelectedPreset.onClick.AddListener(ResetSelectedPanel);
        }

        private void Start()
        {
            presetPanel.SetValueWithoutNotify(RenderSetting.SelectedSetting);
        }

        private void OnDestroy()
        {
            RenderSetting.SaveSettings();
            ffmpegPathField.OnValueChanged += OnFFmpegPath;
            fromTimingField.onEndEdit.RemoveListener(OnTimingFields);
            toTimingField.onEndEdit.RemoveListener(OnTimingFields);
            renderRangeMarker.OnEndEdit -= OnMarker;
            crfField.OnValueChanged.RemoveListener(OnQualityField);
            fpsField.onEndEdit.RemoveListener(OnFpsField);
            widthField.onEndEdit.RemoveListener(OnWidthField);
            heightField.onEndEdit.RemoveListener(OnHeightField);
            musicVolumeField.onEndEdit.RemoveListener(OnMusicVolumeField);
            effectVolumeField.onEndEdit.RemoveListener(OnEffectVolumeField);
            startButton.onClick.RemoveListener(OnStartRenderButton);
            gameplayData.AudioClip.OnValueChange -= OnClipChange;
            cancelButton.onClick.AddListener(OnCancelRenderButton);
            presetPanel.OnSelect -= OnPresetSelect;
            resetSelectedPreset.onClick.RemoveListener(ResetSelectedPanel);
            cts.Dispose();
        }

        private void ResetSelectedPanel()
        {
            RenderSetting.ResetSelectedSetting();
            UpdateRenderSettingFields();
        }

        private void OnPresetSelect(string option)
        {
            RenderSetting.SetSelectedSetting(option);
            UpdateRenderSettingFields();
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

        private void OnQualityField(int crf)
        {
            RenderSetting.Current.Crf = crf;
        }

        private void OnFpsField(string val)
        {
            if (Evaluator.TryFloat(val, out float fps))
            {
                fps = Mathf.Abs(fps);
                RenderSetting.Current.Fps = fps;
            }

            fpsField.text = RenderSetting.Current.Fps.ToString();
        }

        private void OnWidthField(string val)
        {
            if (Evaluator.TryInt(val, out int width))
            {
                width = Mathf.Abs(width);
                RenderSetting.Current.Width = width;
            }

            widthField.text = RenderSetting.Current.Width.ToString();
        }

        private void OnHeightField(string val)
        {
            if (Evaluator.TryInt(val, out int height))
            {
                height = Mathf.Abs(height);
                RenderSetting.Current.Height = height;
            }

            heightField.text = RenderSetting.Current.Height.ToString();
        }

        private void OnMusicVolumeField(string val)
        {
            if (Evaluator.TryFloat(val, out float vol))
            {
                vol = Mathf.Clamp(vol, 0, 1);
                RenderSetting.Current.MusicVolume = vol;
            }

            musicVolumeField.text = RenderSetting.Current.MusicVolume.ToString();
        }

        private void OnEffectVolumeField(string val)
        {
            if (Evaluator.TryFloat(val, out float vol))
            {
                vol = Mathf.Clamp(vol, 0, 1);
                RenderSetting.Current.EffectVolume = vol;
            }

            effectVolumeField.text = RenderSetting.Current.EffectVolume.ToString();
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
            from = -2000;
            to = Mathf.RoundToInt(clip != null ? clip.length * 1000 : 0);
            renderRangeMarker.SetTiming(from, to);
            fromTimingField.text = from.ToString();
            toTimingField.text = to.ToString();
        }

        private void UpdateRenderSettingFields()
        {
            crfField.SetValueWithoutNotify(RenderSetting.Current.Crf);
            fpsField.SetTextWithoutNotify(RenderSetting.Current.Fps.ToString());
            widthField.SetTextWithoutNotify(RenderSetting.Current.Width.ToString());
            heightField.SetTextWithoutNotify(RenderSetting.Current.Height.ToString());
            musicVolumeField.SetTextWithoutNotify(RenderSetting.Current.MusicVolume.ToString());
            effectVolumeField.SetTextWithoutNotify(RenderSetting.Current.EffectVolume.ToString());
        }
    }
}