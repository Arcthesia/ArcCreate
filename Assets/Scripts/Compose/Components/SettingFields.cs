using System;
using System.IO;
using ArcCreate.Data;
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
        [SerializeField] private GameplayData gameplayData;

        [Header("Common")]
        [SerializeField] private TMP_InputField playbackSpeedField;
        [SerializeField] private TMP_InputField densityField;
        [SerializeField] private TimingGroupField groupField;
        [SerializeField] private SettingsDropdown inputModeDropdown;

        [Header("Gameplay")]
        [SerializeField] private TMP_InputField speedField;
        [SerializeField] private TMP_Dropdown aspectRatioDropdown;
        [SerializeField] private SettingsDropdown indicatorPositionDropdown;
        [SerializeField] private SettingsToggle maxIndicatorToggle;
        [SerializeField] private SettingsToggle colorblindModeToggle;
        [SerializeField] private SettingsDropdown scoreDisplayDropdown;

        [Header("Judgement")]
        [SerializeField] private SettingsDropdown lateEarlyPositionDropdown;
        [SerializeField] private SettingsToggle showMsDifferenceToggle;
        [SerializeField] private SettingsToggle showMaxToggle;
        [SerializeField] private SettingsToggle showPerfectToggle;
        [SerializeField] private SettingsToggle showGoodToggle;
        [SerializeField] private SettingsToggle showMissToggle;

        [Header("Audio")]
        [SerializeField] private SettingsInputFieldFloat musicAudioField;
        [SerializeField] private SettingsInputFieldFloat effectAudioField;
        [SerializeField] private SettingsInputFieldInteger globalOffsetField;
        [SerializeField] private SettingsToggle syncToDspTime;

        [Header("Display")]
        [SerializeField] private SettingsInputFieldInteger framerateField;
        [SerializeField] private SettingsToggle vsyncField;
        [SerializeField] private SettingsToggle showFramerateToggle;

        [Header("Input")]
        [SerializeField] private Button reloadHotkeysButton;
        [SerializeField] private Button openHotkeySettingsButton;
        [SerializeField] private SettingsToggle showHotkeyHintsToggle;
        [SerializeField] private SettingsToggle allowCreatingNotesBackwardToggle;
        [SerializeField] private SettingsInputFieldFloat scrollVerticalField;
        [SerializeField] private SettingsInputFieldFloat scrollHorizontalField;
        [SerializeField] private SettingsInputFieldFloat scrollTimelineField;
        [SerializeField] private SettingsInputFieldFloat trackThresholdField;
        [SerializeField] private SettingsInputFieldInteger trackMaxTimingField;

        [Header("Saving")]
        [SerializeField] private SettingsToggle shouldAutosave;
        [SerializeField] private SettingsInputFieldInteger autosaveInterval;
        [SerializeField] private TMP_InputField autosaveIntervalObject;
        [SerializeField] private SettingsToggle shouldSaveBackup;
        [SerializeField] private SettingsInputFieldInteger backupCount;
        [SerializeField] private TMP_InputField backupCountObject;

        [Header("Credits")]
        [SerializeField] private Button openCreditsButton;
        [SerializeField] private Dialog creditsDialog;

        private void Awake()
        {
            aspectRatioDropdown.onValueChanged.AddListener(OnAspectRatioDropdown);
            reloadHotkeysButton.onClick.AddListener(OnReloadHotkeysButton);
            openHotkeySettingsButton.onClick.AddListener(OnOpenHotkeySettingsButton);
            densityField.onEndEdit.AddListener(OnDensityField);
            openCreditsButton.onClick.AddListener(creditsDialog.Open);
            playbackSpeedField.onEndEdit.AddListener(OnPlaybackSpeedField);
            speedField.onEndEdit.AddListener(OnSpeedField);

            Settings.ViewportAspectRatioSetting.OnValueChanged.AddListener(OnAspectRatioSetting);
            Settings.ShouldAutosave.OnValueChanged.AddListener(OnAutosaveSetting);
            Settings.ShouldBackup.OnValueChanged.AddListener(OnBackupSetting);
            Settings.DropRate.OnValueChanged.AddListener(OnDropRateSetting);
            gameplayData.PlaybackSpeed.OnValueChange += OnPlaybackSpeedGameplayChange;
            Values.BeatlineDensity.OnValueChange += OnDensity;

            OnAspectRatioSetting(Settings.ViewportAspectRatioSetting.Value);
            OnAutosaveSetting(Settings.ShouldAutosave.Value);
            OnBackupSetting(Settings.ShouldBackup.Value);
            OnDropRateSetting(Settings.DropRate.Value);
            OnPlaybackSpeedGameplayChange(gameplayData.PlaybackSpeed.Value);
            OnDensity(Values.BeatlineDensity.Value);

            musicAudioField.Setup(Settings.MusicAudio, 2);
            effectAudioField.Setup(Settings.EffectAudio, 2);
            globalOffsetField.Setup(Settings.GlobalAudioOffset);
            framerateField.Setup(Settings.Framerate);
            vsyncField.Setup(Settings.VSync);
            showFramerateToggle.Setup(Settings.ShowFPSCounter);
            scrollVerticalField.Setup(Settings.ScrollSensitivityVertical, 2);
            scrollHorizontalField.Setup(Settings.ScrollSensitivityHorizontal, 2);
            scrollTimelineField.Setup(Settings.ScrollSensitivityTimeline, 2);
            trackThresholdField.Setup(Settings.TrackScrollThreshold, 2);
            trackMaxTimingField.Setup(Settings.TrackScrollMaxMovement);
            maxIndicatorToggle.Setup(Settings.EnableMaxIndicator);
            shouldAutosave.Setup(Settings.ShouldAutosave);
            autosaveInterval.Setup(Settings.AutosaveInterval);
            shouldSaveBackup.Setup(Settings.ShouldBackup);
            backupCount.Setup(Settings.BackupCount);
            syncToDspTime.Setup(Settings.SyncToDSPTime);
            inputModeDropdown.Setup(Settings.InputMode, typeof(InputMode), "Compose.UI.Top.Label.InputModeOptions");
            indicatorPositionDropdown.Setup(Settings.FrPmIndicatorPosition, typeof(FrPmPosition), "Gameplay.Selection.Settings.FrPmPosition");
            showHotkeyHintsToggle.Setup(Settings.EnableKeybindHintDisplay);
            allowCreatingNotesBackwardToggle.Setup(Settings.AllowCreatingNotesBackward);
            colorblindModeToggle.Setup(Settings.EnableColorblind);
            scoreDisplayDropdown.Setup(Settings.ScoreDisplayMode, typeof(ScoreDisplayMode), "Gameplay.Selection.Settings.ScoreDisplay");
            lateEarlyPositionDropdown.Setup(Settings.LateEarlyTextPosition, typeof(EarlyLateTextPosition), "Gameplay.Selection.Settings.EarlyLateTextPosition");
            showMsDifferenceToggle.Setup(Settings.DisplayMsDifference);
            showMaxToggle.Setup(Settings.ShowMaxJudgement);
            showPerfectToggle.Setup(Settings.ShowPerfectJudgement);
            showGoodToggle.Setup(Settings.ShowGoodJudgement);
            showMissToggle.Setup(Settings.ShowMissJudgement);
        }

        private void OnDestroy()
        {
            aspectRatioDropdown.onValueChanged.RemoveListener(OnAspectRatioDropdown);
            reloadHotkeysButton.onClick.RemoveListener(OnReloadHotkeysButton);
            openHotkeySettingsButton.onClick.RemoveListener(OnOpenHotkeySettingsButton);
            densityField.onEndEdit.RemoveListener(OnDensityField);
            openCreditsButton.onClick.RemoveListener(creditsDialog.Open);
            playbackSpeedField.onEndEdit.RemoveListener(OnPlaybackSpeedField);
            speedField.onEndEdit.RemoveListener(OnSpeedField);

            Settings.ViewportAspectRatioSetting.OnValueChanged.RemoveListener(OnAspectRatioSetting);
            Settings.ShouldAutosave.OnValueChanged.RemoveListener(OnAutosaveSetting);
            Settings.ShouldBackup.OnValueChanged.RemoveListener(OnBackupSetting);
            Settings.DropRate.OnValueChanged.RemoveListener(OnDropRateSetting);
            gameplayData.PlaybackSpeed.OnValueChange -= OnPlaybackSpeedGameplayChange;
            Values.BeatlineDensity.OnValueChange -= OnDensity;
        }

        private void OnReloadHotkeysButton()
        {
            Services.Navigation.ReloadHotkeys();
        }

        private void OnOpenHotkeySettingsButton()
        {
            Shell.OpenExplorer(Path.GetDirectoryName(Services.Navigation.ConfigFilePath));
        }

        private void OnPlaybackSpeedField(string str)
        {
            if (Evaluator.TryFloat(str, out float val))
            {
                val = Mathf.Max(val, 0.1f);
                gameplayData.PlaybackSpeed.Value = val;
            }

            playbackSpeedField.SetTextWithoutNotify(gameplayData.PlaybackSpeed.Value.ToString());
        }

        private void OnPlaybackSpeedGameplayChange(float obj)
        {
            playbackSpeedField.SetTextWithoutNotify(gameplayData.PlaybackSpeed.Value.ToString());
        }

        private void OnAspectRatioDropdown(int value)
        {
            Settings.ViewportAspectRatioSetting.Value = value;
        }

        private void OnAspectRatioSetting(int value)
        {
            aspectRatioDropdown.value = value;
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

        private void OnDropRateSetting(int value)
        {
            speedField.SetTextWithoutNotify((value / Constants.DropRateScalar).ToString("F1"));
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

        private void OnBackupSetting(bool on)
        {
            backupCountObject.interactable = on;
        }

        private void OnAutosaveSetting(bool on)
        {
            autosaveIntervalObject.interactable = on;
        }
    }
}