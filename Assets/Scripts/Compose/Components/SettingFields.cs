using System.IO;
using ArcCreate.Gameplay;
using ArcCreate.Utility;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class SettingFields : MonoBehaviour
    {
        // NOTE! If any other means of changing a setting is added, then you should add a listener for that settings on this class as well
        [SerializeField] private GameplayData gameplayData;

        [Header("Common")]
        [SerializeField] private TMP_InputField densityField;
        [SerializeField] private TimingGroupField groupField;
        [SerializeField] private TMP_Dropdown inputModeDropdown;

        [Header("Gameplay")]
        [SerializeField] private TMP_InputField speedField;
        [SerializeField] private TMP_Dropdown aspectRatioDropdown;

        [Header("Audio")]
        [SerializeField] private TMP_InputField musicAudioField;
        [SerializeField] private TMP_InputField effectAudioField;
        [SerializeField] private TMP_InputField globalOffsetField;

        [Header("Display")]
        [SerializeField] private TMP_InputField framerateField;
        [SerializeField] private Toggle vsyncField;
        [SerializeField] private Toggle showFramerateToggle;

        [Header("Input")]
        [SerializeField] private Button reloadHotkeysButton;
        [SerializeField] private Button openHotkeySettingsButton;
        [SerializeField] private TMP_InputField scrollVerticalField;
        [SerializeField] private TMP_InputField scrollHorizontalField;
        [SerializeField] private TMP_InputField scrollTimelineField;
        [SerializeField] private TMP_InputField trackThresholdField;
        [SerializeField] private TMP_InputField trackMaxTimingField;

        [Header("Credits")]
        [SerializeField] private Button openCreditsButton;
        [SerializeField] private Dialog creditsDialog;

        private void Awake()
        {
            aspectRatioDropdown.onValueChanged.AddListener(OnAspectRatioDropdown);
            musicAudioField.onEndEdit.AddListener(OnMusicAudioField);
            effectAudioField.onEndEdit.AddListener(OnEffectAudioField);
            globalOffsetField.onEndEdit.AddListener(OnGlobalOffsetField);
            framerateField.onEndEdit.AddListener(OnFramerateField);
            vsyncField.onValueChanged.AddListener(OnVsyncField);
            showFramerateToggle.onValueChanged.AddListener(OnShowFramerateToggle);
            reloadHotkeysButton.onClick.AddListener(OnReloadHotkeysButton);
            openHotkeySettingsButton.onClick.AddListener(OnOpenHotkeySettingsButton);
            scrollVerticalField.onEndEdit.AddListener(OnScrollVerticalField);
            scrollHorizontalField.onEndEdit.AddListener(OnScrollHorizontalField);
            scrollTimelineField.onEndEdit.AddListener(OnScrollTimelineField);
            trackThresholdField.onEndEdit.AddListener(OnTrackThresholdField);
            trackMaxTimingField.onEndEdit.AddListener(OnTrackMaxTimingField);
            inputModeDropdown.onValueChanged.AddListener(OnInputModeDropdown);
            speedField.onEndEdit.AddListener(OnSpeedField);
            densityField.onEndEdit.AddListener(OnDensityField);
            openCreditsButton.onClick.AddListener(creditsDialog.Open);

            Settings.InputMode.OnValueChanged.AddListener(OnSettingInputMode);
            Values.BeatlineDensity.OnValueChange += OnDensity;

            musicAudioField.SetTextWithoutNotify(Settings.MusicAudio.Value.ToString());
            effectAudioField.SetTextWithoutNotify(Settings.EffectAudio.Value.ToString());
            globalOffsetField.SetTextWithoutNotify(Settings.GlobalAudioOffset.Value.ToString());
            framerateField.SetTextWithoutNotify(Settings.Framerate.Value.ToString());
            vsyncField.SetIsOnWithoutNotify(Settings.VSync.Value == 1);
            showFramerateToggle.SetIsOnWithoutNotify(Settings.ShowFPSCounter.Value);
            scrollVerticalField.SetTextWithoutNotify(Settings.ScrollSensitivityVertical.Value.ToString());
            scrollHorizontalField.SetTextWithoutNotify(Settings.ScrollSensitivityHorizontal.Value.ToString());
            scrollTimelineField.SetTextWithoutNotify(Settings.ScrollSensitivityTimeline.Value.ToString());
            trackThresholdField.SetTextWithoutNotify(Settings.TrackScrollThreshold.Value.ToString());
            trackMaxTimingField.SetTextWithoutNotify(Settings.TrackScrollMaxMovement.Value.ToString());
            inputModeDropdown.SetValueWithoutNotify(Settings.InputMode.Value);
            speedField.SetTextWithoutNotify((Settings.DropRate.Value / Constants.DropRateScalar).ToString("F1"));
            densityField.SetTextWithoutNotify(Values.BeatlineDensity.Value.ToString());
        }

        private void OnDestroy()
        {
            aspectRatioDropdown.onValueChanged.RemoveListener(OnAspectRatioDropdown);
            musicAudioField.onEndEdit.RemoveListener(OnMusicAudioField);
            effectAudioField.onEndEdit.RemoveListener(OnEffectAudioField);
            globalOffsetField.onEndEdit.RemoveListener(OnGlobalOffsetField);
            framerateField.onEndEdit.RemoveListener(OnFramerateField);
            vsyncField.onValueChanged.RemoveListener(OnVsyncField);
            showFramerateToggle.onValueChanged.RemoveListener(OnShowFramerateToggle);
            reloadHotkeysButton.onClick.RemoveListener(OnReloadHotkeysButton);
            openHotkeySettingsButton.onClick.RemoveListener(OnOpenHotkeySettingsButton);
            scrollVerticalField.onEndEdit.RemoveListener(OnScrollVerticalField);
            scrollHorizontalField.onEndEdit.RemoveListener(OnScrollHorizontalField);
            scrollTimelineField.onEndEdit.RemoveListener(OnScrollTimelineField);
            trackThresholdField.onEndEdit.RemoveListener(OnTrackThresholdField);
            trackMaxTimingField.onEndEdit.RemoveListener(OnTrackMaxTimingField);
            inputModeDropdown.onValueChanged.RemoveListener(OnInputModeDropdown);
            speedField.onEndEdit.RemoveListener(OnSpeedField);
            densityField.onEndEdit.RemoveListener(OnDensityField);
            openCreditsButton.onClick.RemoveListener(creditsDialog.Open);

            Settings.InputMode.OnValueChanged.RemoveListener(OnSettingInputMode);
            Values.BeatlineDensity.OnValueChange -= OnDensity;
        }

        private void OnAspectRatioDropdown(int value)
        {
            Settings.ViewportAspectRatioSetting.Value = value;
        }

        private void OnMusicAudioField(string value)
        {
            if (Evaluator.TryFloat(value, out float volume))
            {
                Settings.MusicAudio.Value = Mathf.Clamp(volume, 0, 1);
            }

            musicAudioField.SetTextWithoutNotify(Settings.MusicAudio.Value.ToString());
        }

        private void OnEffectAudioField(string value)
        {
            if (Evaluator.TryFloat(value, out float volume))
            {
                Settings.EffectAudio.Value = Mathf.Clamp(volume, 0, 1);
            }

            effectAudioField.SetTextWithoutNotify(Settings.EffectAudio.Value.ToString());
        }

        private void OnGlobalOffsetField(string value)
        {
            if (Evaluator.TryInt(value, out int offset))
            {
                Settings.GlobalAudioOffset.Value = offset;
            }

            globalOffsetField.SetTextWithoutNotify(Settings.GlobalAudioOffset.Value.ToString());
        }

        private void OnFramerateField(string value)
        {
            if (Evaluator.TryInt(value, out int framerate))
            {
                Settings.Framerate.Value = framerate;
            }

            framerateField.SetTextWithoutNotify(Settings.Framerate.Value.ToString());
        }

        private void OnVsyncField(bool value)
        {
            Settings.VSync.Value = value ? 1 : 0;
        }

        private void OnShowFramerateToggle(bool value)
        {
            Settings.ShowFPSCounter.Value = value;
        }

        private void OnReloadHotkeysButton()
        {
            Services.Navigation.ReloadHotkeys();
        }

        private void OnOpenHotkeySettingsButton()
        {
            Shell.OpenExplorer(Path.GetDirectoryName(Services.Navigation.ConfigFilePath));
        }

        private void OnScrollVerticalField(string value)
        {
            if (Evaluator.TryFloat(value, out float scr))
            {
                Settings.ScrollSensitivityVertical.Value = scr;
            }

            scrollVerticalField.SetTextWithoutNotify(Settings.ScrollSensitivityVertical.Value.ToString());
        }

        private void OnScrollHorizontalField(string value)
        {
            if (Evaluator.TryFloat(value, out float scr))
            {
                Settings.ScrollSensitivityHorizontal.Value = scr;
            }

            scrollHorizontalField.SetTextWithoutNotify(Settings.ScrollSensitivityHorizontal.Value.ToString());
        }

        private void OnScrollTimelineField(string value)
        {
            if (Evaluator.TryFloat(value, out float scr))
            {
                Settings.ScrollSensitivityTimeline.Value = scr;
            }

            scrollTimelineField.SetTextWithoutNotify(Settings.ScrollSensitivityTimeline.Value.ToString());
        }

        private void OnTrackThresholdField(string value)
        {
            if (Evaluator.TryFloat(value, out float scr))
            {
                Settings.TrackScrollThreshold.Value = scr;
            }

            trackThresholdField.SetTextWithoutNotify(Settings.TrackScrollThreshold.Value.ToString());
        }

        private void OnTrackMaxTimingField(string value)
        {
            if (Evaluator.TryInt(value, out int max))
            {
                Settings.TrackScrollMaxMovement.Value = max;
            }

            trackMaxTimingField.SetTextWithoutNotify(Settings.TrackScrollMaxMovement.Value.ToString());
        }

        private void OnInputModeDropdown(int mode)
        {
            Settings.InputMode.Value = mode;
        }

        private void OnSettingInputMode(int mode)
        {
            inputModeDropdown.SetValueWithoutNotify(mode);
        }

        private void OnSpeedField(string value)
        {
            if (Evaluator.TryFloat(value, out float speed))
            {
                speed = Mathf.Max(speed, 0.1f);
                Settings.DropRate.Value = (int)System.Math.Round(speed * Constants.DropRateScalar);
            }

            speedField.SetTextWithoutNotify((Settings.DropRate.Value / Constants.DropRateScalar).ToString());
        }

        private void OnDensityField(string value)
        {
            // if there's ever a command interface then this will be moved there
            if (EasterEggs.TryTrigger(value))
            {
                densityField.SetTextWithoutNotify(Values.BeatlineDensity.Value.ToString());
                return;
            }

            if (Evaluator.TryFloat(value, out float density))
            {
                Values.BeatlineDensity.Value = density;
            }

            densityField.SetTextWithoutNotify(Values.BeatlineDensity.Value.ToString());
        }

        private void OnDensity(float density)
        {
            densityField.SetTextWithoutNotify(Values.BeatlineDensity.Value.ToString());
        }
    }
}