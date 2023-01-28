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

        private void Awake()
        {
            speedField.onEndEdit.AddListener(OnSpeedField);
            densityField.onEndEdit.AddListener(OnDensityField);
            groupField.onEndEdit.AddListener(OnGroupField);

            Settings.DropRate.OnValueChanged.AddListener(OnSettingDropRate);
            Values.EditingTimingGroup.OnValueChange += OnGroup;

            speedField.text = (Settings.DropRate.Value / Values.DropRateScalar).ToString();
        }

        private void OnDestroy()
        {
            speedField.onEndEdit.RemoveListener(OnSpeedField);
            densityField.onEndEdit.RemoveListener(OnDensityField);
            groupField.onEndEdit.RemoveListener(OnGroupField);

            Settings.DropRate.OnValueChanged.RemoveListener(OnSettingDropRate);
            Values.EditingTimingGroup.OnValueChange -= OnGroup;
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
            EasterEggs.TryTrigger(value);

            // TODO: Implement density grid lol
            densityField.text = "4";
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