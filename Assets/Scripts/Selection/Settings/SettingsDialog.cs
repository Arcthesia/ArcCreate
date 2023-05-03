using System;
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
        [SerializeField] private GameplayData gameplayData;

        [Header("Gameplay")]
        [SerializeField] private Button decreaseSpeedButton;
        [SerializeField] private Button increateSpeedButton;
        [SerializeField] private TMP_Text noteSpeedText;
        [SerializeField] private Toggle earlyLatePerfectToggle;
        [SerializeField] private Button enablePracticeButton;
        [SerializeField] private Button disablePracticeButton;
        [SerializeField] private ScriptedAnimator practiceAnimation;

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
        [SerializeField] private Toggle colorblindModeToggle;
        [SerializeField] private Button changeFrPmDisplayPositionButton;
        [SerializeField] private TMP_Text frPmDisplayPositionText;
        [SerializeField] private Button changeLateEarlyPositionButton;
        [SerializeField] private TMP_Text lateEarlyPositionText;
        [SerializeField] private Toggle maxIndicatorToggle;
        [SerializeField] private Toggle limitFrameRateToggle;
        [SerializeField] private Toggle showFpsToggle;
        [SerializeField] private Toggle showDebug;

        protected override void Awake()
        {
            base.Awake();
            decreaseSpeedButton.onClick.AddListener(OnDecreaseSpeedButton);
            increateSpeedButton.onClick.AddListener(OnIncreateSpeedButton);
            earlyLatePerfectToggle.onValueChanged.AddListener(OnEarlyLatePerfectToggle);
            increaseNoteVolumeButton.onClick.AddListener(OnIncreaseNoteVolumeButton);
            decreaseNoteVolumeButton.onClick.AddListener(OnDecreaseNoteVolumeButton);
            increaseMusicVolumeButton.onClick.AddListener(OnIncreaseMusicVolumeButton);
            decreaseMusicVolumeButton.onClick.AddListener(OnDecreaseMusicVolumeButton);
            increaseOffsetButton.onClick.AddListener(OnIncreaseOffsetButton);
            decreaseOffsetButton.onClick.AddListener(OnDecreaseOffsetButton);
            colorblindModeToggle.onValueChanged.AddListener(OnColorblindModeToggle);
            changeFrPmDisplayPositionButton.onClick.AddListener(OnFrPmDisplayButton);
            changeLateEarlyPositionButton.onClick.AddListener(OnChangeLateEarlyPositionButton);
            limitFrameRateToggle.onValueChanged.AddListener(OnLimitFrameRateToggle);
            showFpsToggle.onValueChanged.AddListener(OnShowFpsToggle);
            showDebug.onValueChanged.AddListener(OnShowDebugToggle);
            maxIndicatorToggle.onValueChanged.AddListener(OnMaxIndicatorToggle);
            enablePracticeButton.onClick.AddListener(EnablePractice);
            disablePracticeButton.onClick.AddListener(DisablePractice);

            Settings.DropRate.OnValueChanged.AddListener(OnDropRateSettings);
            Settings.ShowEarlyLatePerfect.OnValueChanged.AddListener(OnShowEarlyLatePerfectSettings);
            Settings.MusicAudio.OnValueChanged.AddListener(OnMusicAudioSettings);
            Settings.EffectAudio.OnValueChanged.AddListener(OnEffectAudioSettings);
            Settings.GlobalAudioOffset.OnValueChanged.AddListener(OnGlobalAudioOffsetSettings);
            Settings.EnableColorblind.OnValueChanged.AddListener(OnEnableColorblindSettings);
            Settings.FrPmIndicatorPosition.OnValueChanged.AddListener(OnFrPmIndicatorPositionSettings);
            Settings.LateEarlyTextPosition.OnValueChanged.AddListener(OnLateEarlyTextPositionSettings);
            Settings.LimitFrameRate.OnValueChanged.AddListener(OnLimitFrameRateSettings);
            Settings.ShowFPSCounter.OnValueChanged.AddListener(OnShowFPSCounterSettings);
            Settings.ShowGameplayDebug.OnValueChanged.AddListener(OnShowDebugSettings);
            Settings.EnableMaxIndicator.OnValueChanged.AddListener(OnMaxIndicatorSettings);

            OnDropRateSettings(Settings.DropRate.Value);
            OnShowEarlyLatePerfectSettings(Settings.ShowEarlyLatePerfect.Value);
            OnMusicAudioSettings(Settings.MusicAudio.Value);
            OnEffectAudioSettings(Settings.EffectAudio.Value);
            OnGlobalAudioOffsetSettings(Settings.GlobalAudioOffset.Value);
            OnEnableColorblindSettings(Settings.EnableColorblind.Value);
            OnFrPmIndicatorPositionSettings(Settings.FrPmIndicatorPosition.Value);
            OnLateEarlyTextPositionSettings(Settings.LateEarlyTextPosition.Value);
            OnLimitFrameRateSettings(Settings.LimitFrameRate.Value);
            OnShowFPSCounterSettings(Settings.ShowFPSCounter.Value);
            OnShowDebugSettings(Settings.ShowGameplayDebug.Value);
            OnMaxIndicatorSettings(Settings.EnableMaxIndicator.Value);

            // setupOffsetButton.onClick.AddListener(setupOffsetDialog.Show);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            decreaseSpeedButton.onClick.RemoveListener(OnDecreaseSpeedButton);
            increateSpeedButton.onClick.RemoveListener(OnIncreateSpeedButton);
            earlyLatePerfectToggle.onValueChanged.RemoveListener(OnEarlyLatePerfectToggle);
            increaseNoteVolumeButton.onClick.RemoveListener(OnIncreaseNoteVolumeButton);
            decreaseNoteVolumeButton.onClick.RemoveListener(OnDecreaseNoteVolumeButton);
            increaseMusicVolumeButton.onClick.RemoveListener(OnIncreaseMusicVolumeButton);
            decreaseMusicVolumeButton.onClick.RemoveListener(OnDecreaseMusicVolumeButton);
            increaseOffsetButton.onClick.RemoveListener(OnIncreaseOffsetButton);
            decreaseOffsetButton.onClick.RemoveListener(OnDecreaseOffsetButton);
            colorblindModeToggle.onValueChanged.RemoveListener(OnColorblindModeToggle);
            changeFrPmDisplayPositionButton.onClick.RemoveListener(OnFrPmDisplayButton);
            changeLateEarlyPositionButton.onClick.RemoveListener(OnChangeLateEarlyPositionButton);
            limitFrameRateToggle.onValueChanged.RemoveListener(OnLimitFrameRateToggle);
            showFpsToggle.onValueChanged.RemoveListener(OnShowFpsToggle);
            maxIndicatorToggle.onValueChanged.RemoveListener(OnMaxIndicatorToggle);
            enablePracticeButton.onClick.RemoveListener(EnablePractice);
            disablePracticeButton.onClick.AddListener(DisablePractice);

            Settings.DropRate.OnValueChanged.RemoveListener(OnDropRateSettings);
            Settings.ShowEarlyLatePerfect.OnValueChanged.RemoveListener(OnShowEarlyLatePerfectSettings);
            Settings.MusicAudio.OnValueChanged.RemoveListener(OnMusicAudioSettings);
            Settings.EffectAudio.OnValueChanged.RemoveListener(OnEffectAudioSettings);
            Settings.GlobalAudioOffset.OnValueChanged.RemoveListener(OnGlobalAudioOffsetSettings);
            Settings.EnableColorblind.OnValueChanged.RemoveListener(OnEnableColorblindSettings);
            Settings.FrPmIndicatorPosition.OnValueChanged.RemoveListener(OnFrPmIndicatorPositionSettings);
            Settings.LateEarlyTextPosition.OnValueChanged.RemoveListener(OnLateEarlyTextPositionSettings);
            Settings.LimitFrameRate.OnValueChanged.RemoveListener(OnLimitFrameRateSettings);
            Settings.ShowFPSCounter.OnValueChanged.RemoveListener(OnShowFPSCounterSettings);
            Settings.EnableMaxIndicator.OnValueChanged.RemoveListener(OnMaxIndicatorSettings);

            // setupOffsetButton.onClick.RemoveListener(setupOffsetDialog.Show);
        }

        private void OnDropRateSettings(int value)
        {
            noteSpeedText.text = (value / Constants.DropRateScalar).ToString("F1");
        }

        private void OnShowEarlyLatePerfectSettings(bool value)
        {
            earlyLatePerfectToggle.SetIsOnWithoutNotify(value);
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

        private void OnEnableColorblindSettings(bool value)
        {
            colorblindModeToggle.SetIsOnWithoutNotify(value);
        }

        private void OnFrPmIndicatorPositionSettings(int value)
        {
            FrPmPosition position = (FrPmPosition)value;
            frPmDisplayPositionText.text = I18n.S($"Gameplay.Selection.Settings.FrPmPosition.{position.ToString().ToLower()}");
        }

        private void OnLateEarlyTextPositionSettings(int value)
        {
            EarlyLateTextPosition position = (EarlyLateTextPosition)value;
            lateEarlyPositionText.text = I18n.S($"Gameplay.Selection.Settings.EarlyLateTextPosition.{position.ToString().ToLower()}");
        }

        private void OnLimitFrameRateSettings(bool value)
        {
            limitFrameRateToggle.SetIsOnWithoutNotify(value);
        }

        private void OnShowFPSCounterSettings(bool value)
        {
            showFpsToggle.SetIsOnWithoutNotify(value);
        }

        private void OnShowDebugSettings(bool value)
        {
            showDebug.SetIsOnWithoutNotify(value);
        }

        private void OnMaxIndicatorSettings(bool value)
        {
            maxIndicatorToggle.SetIsOnWithoutNotify(value);
        }

        private void OnDecreaseSpeedButton()
        {
            Settings.DropRate.Value = (int)Mathf.Clamp(Settings.DropRate.Value - (Constants.DropRateScalar / 10), Constants.MinDropRate, Constants.MaxDropRate);
        }

        private void OnIncreateSpeedButton()
        {
            Settings.DropRate.Value = (int)Mathf.Clamp(Settings.DropRate.Value + (Constants.DropRateScalar / 10), Constants.MinDropRate, Constants.MaxDropRate);
        }

        private void OnEarlyLatePerfectToggle(bool value)
        {
            Settings.ShowEarlyLatePerfect.Value = value;
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

        private void OnColorblindModeToggle(bool value)
        {
            Settings.EnableColorblind.Value = value;
        }

        private void OnFrPmDisplayButton()
        {
            Settings.FrPmIndicatorPosition.Value = (Settings.FrPmIndicatorPosition.Value + 1) % 3;
        }

        private void OnChangeLateEarlyPositionButton()
        {
            Settings.LateEarlyTextPosition.Value = (Settings.LateEarlyTextPosition.Value + 1) % 3;
        }

        private void OnLimitFrameRateToggle(bool value)
        {
            Settings.LimitFrameRate.Value = value;
        }

        private void OnShowFpsToggle(bool value)
        {
            Settings.ShowFPSCounter.Value = value;
        }

        private void OnShowDebugToggle(bool value)
        {
            Settings.ShowGameplayDebug.Value = value;
        }

        private void OnMaxIndicatorToggle(bool value)
        {
            Settings.EnableMaxIndicator.Value = value;
        }

        private void EnablePractice()
        {
            gameplayData.EnablePracticeMode.Value = true;
            practiceAnimation.Show();
            Hide();
        }

        private void DisablePractice()
        {
            gameplayData.EnablePracticeMode.Value = false;
            practiceAnimation.Hide();
        }
    }
}