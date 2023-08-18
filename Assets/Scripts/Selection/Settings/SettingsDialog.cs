using System;
using System.Collections.Generic;
using ArcCreate.Data;
using ArcCreate.Gameplay;
using ArcCreate.Utility;
using ArcCreate.Utility.Animation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class SettingsDialog : Dialog
    {
        [SerializeField] private ThemeGroup themeGroup;
        [SerializeField] private GameplayData gameplayData;

        [Header("Gameplay")]
        [SerializeField] private Button decreaseSpeedButton;
        [SerializeField] private Button increateSpeedButton;
        [SerializeField] private TMP_Text noteSpeedText;
        [SerializeField] private SettingsToggle earlyLatePerfectSetting;
        [SerializeField] private Button openCreditButton;
        [SerializeField] private Button closeCreditButton;
        [SerializeField] private ScriptedAnimator creditAnimation;

        [Header("Audio")]
        [SerializeField] private Button increaseNoteVolumeButton;
        [SerializeField] private Button decreaseNoteVolumeButton;
        [SerializeField] private TMP_Text noteVolumeText;
        [SerializeField] private Button increaseMusicVolumeButton;
        [SerializeField] private Button decreaseMusicVolumeButton;
        [SerializeField] private TMP_Text musicVolumeText;
        [SerializeField] private Button increaseOffsetButton;
        [SerializeField] private Button decreaseOffsetButton;
        [SerializeField] private TMP_Text offsetText;
        [SerializeField] private Button setupOffsetButton;
        [SerializeField] private Dialog setupOffsetDialog;

        [Header("Visual")]
        [SerializeField] private SettingsToggle colorblindModeSetting;
        [SerializeField] private SettingsEnum frPmDisplayPositionSetting;
        [SerializeField] private SettingsEnum lateEarlyPositionSetting;
        [SerializeField] private SettingsToggle maxIndicatorSetting;
        [SerializeField] private SettingsToggle limitFrameRateSetting;
        [SerializeField] private SettingsEnum scoreDisplaySetting;
        [SerializeField] private List<ScoreDisplayPreviewItem> scoreDisplayPreviews;

        [Header("Judgement")]
        [SerializeField] private SettingsToggle showMsDifferenceSetting;
        [SerializeField] private SettingsToggle showMaxSetting;
        [SerializeField] private SettingsToggle showPerfectSetting;
        [SerializeField] private SettingsToggle showGoodSetting;
        [SerializeField] private SettingsToggle showMissSetting;

        [Header("Interface")]
        [SerializeField] private SettingsEnum forceUIThemeSetting;
        [SerializeField] private SettingsToggle switchResumeAndRetrySetting;
        [SerializeField] private SettingsToggle showFpsSetting;
        [SerializeField] private SettingsToggle showDebugSetting;

        protected override void Awake()
        {
            base.Awake();
            decreaseSpeedButton.onClick.AddListener(OnDecreaseSpeedButton);
            increateSpeedButton.onClick.AddListener(OnIncreateSpeedButton);
            increaseNoteVolumeButton.onClick.AddListener(OnIncreaseNoteVolumeButton);
            decreaseNoteVolumeButton.onClick.AddListener(OnDecreaseNoteVolumeButton);
            increaseMusicVolumeButton.onClick.AddListener(OnIncreaseMusicVolumeButton);
            decreaseMusicVolumeButton.onClick.AddListener(OnDecreaseMusicVolumeButton);
            increaseOffsetButton.onClick.AddListener(OnIncreaseOffsetButton);
            decreaseOffsetButton.onClick.AddListener(OnDecreaseOffsetButton);

            Settings.DropRate.OnValueChanged.AddListener(OnDropRateSettings);
            Settings.MusicAudio.OnValueChanged.AddListener(OnMusicAudioSettings);
            Settings.EffectAudio.OnValueChanged.AddListener(OnEffectAudioSettings);
            Settings.GlobalAudioOffset.OnValueChanged.AddListener(OnGlobalAudioOffsetSettings);
            Settings.ScoreDisplayMode.OnValueChanged.AddListener(ChangeScoreDisplayPreview);
            Settings.ForceTheme.OnValueChanged.AddListener(OnForceThemeSettings);

            OnDropRateSettings(Settings.DropRate.Value);
            OnMusicAudioSettings(Settings.MusicAudio.Value);
            OnEffectAudioSettings(Settings.EffectAudio.Value);
            OnGlobalAudioOffsetSettings(Settings.GlobalAudioOffset.Value);
            ChangeScoreDisplayPreview(Settings.ScoreDisplayMode.Value);
            OnForceThemeSettings(Settings.ForceTheme.Value);

            earlyLatePerfectSetting.Setup(Settings.ShowEarlyLatePerfect);
            colorblindModeSetting.Setup(Settings.EnableColorblind);
            frPmDisplayPositionSetting.Setup(Settings.FrPmIndicatorPosition, typeof(FrPmPosition), "Gameplay.Selection.Settings.FrPmPosition");
            earlyLatePerfectSetting.Setup(Settings.ShowEarlyLatePerfect);
            lateEarlyPositionSetting.Setup(Settings.LateEarlyTextPosition, typeof(EarlyLateTextPosition), "Gameplay.Selection.Settings.EarlyLateTextPosition");
            limitFrameRateSetting.Setup(Settings.LimitFrameRate);
            showFpsSetting.Setup(Settings.ShowFPSCounter);
            showDebugSetting.Setup(Settings.ShowGameplayDebug);
            maxIndicatorSetting.Setup(Settings.EnableMaxIndicator);
            scoreDisplaySetting.Setup(Settings.ScoreDisplayMode, typeof(ScoreDisplayMode), "Gameplay.Selection.Settings.ScoreDisplay");
            showMsDifferenceSetting.Setup(Settings.DisplayMsDifference);
            showMaxSetting.Setup(Settings.ShowMaxJudgement);
            showPerfectSetting.Setup(Settings.ShowPerfectJudgement);
            showGoodSetting.Setup(Settings.ShowGoodJudgement);
            showMissSetting.Setup(Settings.ShowMissJudgement);
            forceUIThemeSetting.Setup(Settings.ForceTheme, typeof(ForceUIThemeMode), "Gameplay.Selection.Settings.ForceTheme");
            switchResumeAndRetrySetting.Setup(Settings.SwitchResumeAndRetryPosition);

            openCreditButton.onClick.AddListener(creditAnimation.Show);
            closeCreditButton.onClick.AddListener(creditAnimation.Hide);

            // setupOffsetButton.onClick.AddListener(setupOffsetDialog.Show);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            decreaseSpeedButton.onClick.RemoveListener(OnDecreaseSpeedButton);
            increateSpeedButton.onClick.RemoveListener(OnIncreateSpeedButton);
            increaseNoteVolumeButton.onClick.RemoveListener(OnIncreaseNoteVolumeButton);
            decreaseNoteVolumeButton.onClick.RemoveListener(OnDecreaseNoteVolumeButton);
            increaseMusicVolumeButton.onClick.RemoveListener(OnIncreaseMusicVolumeButton);
            decreaseMusicVolumeButton.onClick.RemoveListener(OnDecreaseMusicVolumeButton);
            increaseOffsetButton.onClick.RemoveListener(OnIncreaseOffsetButton);
            decreaseOffsetButton.onClick.RemoveListener(OnDecreaseOffsetButton);

            Settings.DropRate.OnValueChanged.RemoveListener(OnDropRateSettings);
            Settings.MusicAudio.OnValueChanged.RemoveListener(OnMusicAudioSettings);
            Settings.EffectAudio.OnValueChanged.RemoveListener(OnEffectAudioSettings);
            Settings.GlobalAudioOffset.OnValueChanged.RemoveListener(OnGlobalAudioOffsetSettings);
            Settings.ScoreDisplayMode.OnValueChanged.RemoveListener(ChangeScoreDisplayPreview);
            Settings.ForceTheme.OnValueChanged.RemoveListener(OnForceThemeSettings);

            openCreditButton.onClick.RemoveListener(creditAnimation.Show);
            closeCreditButton.onClick.RemoveListener(creditAnimation.Hide);

            // setupOffsetButton.onClick.RemoveListener(setupOffsetDialog.Show);
        }

        private void OnDropRateSettings(int value)
        {
            noteSpeedText.text = (value / Constants.DropRateScalar).ToString("F1");
        }

        private void OnMusicAudioSettings(float value)
        {
            musicVolumeText.text = Mathf.RoundToInt(value * 100).ToString() + "%";
        }

        private void OnEffectAudioSettings(float value)
        {
            noteVolumeText.text = Mathf.RoundToInt(value * 100).ToString() + "%";
        }

        private void OnGlobalAudioOffsetSettings(int value)
        {
            offsetText.text = value.ToString();
        }

        private void OnDecreaseSpeedButton()
        {
            Settings.DropRate.Value = (int)Mathf.Clamp(Settings.DropRate.Value - (Constants.DropRateScalar / 10), Constants.MinDropRate, Constants.MaxDropRate);
        }

        private void OnIncreateSpeedButton()
        {
            Settings.DropRate.Value = (int)Mathf.Clamp(Settings.DropRate.Value + (Constants.DropRateScalar / 10), Constants.MinDropRate, Constants.MaxDropRate);
        }

        private void OnIncreaseNoteVolumeButton()
        {
            Settings.EffectAudio.Value = Mathf.Clamp(Settings.EffectAudio.Value + 0.1f, 0, 1);
        }

        private void OnDecreaseNoteVolumeButton()
        {
            Settings.EffectAudio.Value = Mathf.Clamp(Settings.EffectAudio.Value - 0.1f, 0, 1);
        }

        private void OnIncreaseMusicVolumeButton()
        {
            Settings.MusicAudio.Value = Mathf.Clamp(Settings.MusicAudio.Value + 0.1f, 0, 1);
        }

        private void OnDecreaseMusicVolumeButton()
        {
            Settings.MusicAudio.Value = Mathf.Clamp(Settings.MusicAudio.Value - 0.1f, 0, 1);
        }

        private void OnIncreaseOffsetButton()
        {
            Settings.GlobalAudioOffset.Value = Settings.GlobalAudioOffset.Value + 1;
        }

        private void OnDecreaseOffsetButton()
        {
            Settings.GlobalAudioOffset.Value = Settings.GlobalAudioOffset.Value - 1;
        }

        private void ChangeScoreDisplayPreview(int val)
        {
            ScoreDisplayMode mode = (ScoreDisplayMode)val;
            foreach (var item in scoreDisplayPreviews)
            {
                item.GameObject.SetActive(mode == item.Mode);
            }
        }

        private void OnForceThemeSettings(int value)
        {
            switch ((ForceUIThemeMode)Settings.ForceTheme.Value)
            {
                case ForceUIThemeMode.Light:
                    themeGroup.OverrideValue = Theme.Light;
                    break;
                case ForceUIThemeMode.Dark:
                    themeGroup.OverrideValue = Theme.Dark;
                    break;
                default:
                    themeGroup.OverrideValue = Option<Theme>.None();
                    break;
            }
        }

        [Serializable]
        private struct ScoreDisplayPreviewItem
        {
            public ScoreDisplayMode Mode;

            public GameObject GameObject;
        }
    }
}