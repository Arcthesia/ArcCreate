using System;
using ArcCreate.Gameplay;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;

namespace ArcCreate.Compose.Components
{
    public class SettingFields : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private TMP_InputField speedField;
        [SerializeField] private TMP_InputField densityField;
        [SerializeField] private TMP_InputField groupField;
        [SerializeField] private TMP_Dropdown inputModeDropdown;

        private void Awake()
        {
            speedField.onEndEdit.AddListener(OnSpeedField);
            densityField.onEndEdit.AddListener(OnDensityField);
            groupField.onEndEdit.AddListener(OnGroupField);
            inputModeDropdown.onValueChanged.AddListener(OnInputModeDropdown);

            Settings.DropRate.OnValueChanged.AddListener(OnSettingDropRate);
            Settings.InputMode.OnValueChanged.AddListener(OnSettingInputMode);
            Values.EditingTimingGroup.OnValueChange += OnGroup;
            Values.BeatlineDensity.OnValueChange += OnDensity;

            speedField.text = (Settings.DropRate.Value / Values.DropRateScalar).ToString();
            inputModeDropdown.value = Settings.InputMode.Value;
            densityField.SetTextWithoutNotify(Values.BeatlineDensity.Value.ToString());
        }

        private void OnDestroy()
        {
            speedField.onEndEdit.RemoveListener(OnSpeedField);
            densityField.onEndEdit.RemoveListener(OnDensityField);
            groupField.onEndEdit.RemoveListener(OnGroupField);
            inputModeDropdown.onValueChanged.RemoveListener(OnInputModeDropdown);

            Settings.DropRate.OnValueChanged.RemoveListener(OnSettingDropRate);
            Settings.InputMode.OnValueChanged.RemoveListener(OnSettingInputMode);
            Values.EditingTimingGroup.OnValueChange -= OnGroup;
            Values.BeatlineDensity.OnValueChange -= OnDensity;
        }

        private void OnDensity(float density)
        {
            densityField.SetTextWithoutNotify(Values.BeatlineDensity.Value.ToString());
        }

        private void OnInputModeDropdown(int mode)
        {
            Settings.InputMode.Value = mode;
        }

        private void OnSettingInputMode(int mode)
        {
            inputModeDropdown.SetValueWithoutNotify(mode);
        }

        private void OnSettingDropRate(int dropRate)
        {
            speedField.text = (dropRate / Values.DropRateScalar).ToString();
        }

        private void OnGroup(int group)
        {
            groupField.text = group.ToString();
        }

        private void OnSpeedField(string value)
        {
            if (Evaluator.TryFloat(value, out float speed))
            {
                Settings.DropRate.Value = Mathf.RoundToInt(speed * Values.DropRateScalar);
            }
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
        }

        private void OnGroupField(string value)
        {
            if (Evaluator.TryInt(value, out int group))
            {
                var tg = Services.Gameplay.Chart.GetTimingGroup(group);
                Values.EditingTimingGroup.Value = tg.GroupNumber;
            }

            groupField.SetTextWithoutNotify(Values.EditingTimingGroup.Value.ToString());
        }
    }
}