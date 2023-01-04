using ArcCreate.Gameplay;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;

namespace ArcCreate.Compose.Timeline
{
    public class TimelineFields : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private TMP_InputField offsetField;
        [SerializeField] private TMP_InputField speedField;
        [SerializeField] private TMP_InputField groupField;

        private void Awake()
        {
            speedField.onEndEdit.AddListener(OnSpeedField);
            groupField.onEndEdit.AddListener(OnGroupField);
            offsetField.onEndEdit.AddListener(OnOffsetField);
            speedField.onEndEdit.AddListener(OnSpeedField);
            groupField.onEndEdit.AddListener(OnGroupField);

            gameplayData.AudioOffset.OnValueChange += OnGameplayOffset;
            Settings.DropRate.OnValueChanged.AddListener(OnSettingDropRate);
            Values.EditingTimingGroup.OnValueChange += OnGroup;

            speedField.text = (Settings.DropRate.Value / Values.DropRateScalar).ToString();
        }

        private void OnDestroy()
        {
            offsetField.onEndEdit.RemoveListener(OnOffsetField);
            speedField.onEndEdit.RemoveListener(OnSpeedField);
            groupField.onEndEdit.RemoveListener(OnGroupField);

            gameplayData.AudioOffset.OnValueChange -= OnGameplayOffset;
            Settings.DropRate.OnValueChanged.RemoveListener(OnSettingDropRate);
            Values.EditingTimingGroup.OnValueChange -= OnGroup;
        }

        private void OnGameplayOffset(int offset)
        {
            offsetField.text = offset.ToString();
        }

        private void OnSettingDropRate(int dropRate)
        {
            speedField.text = (dropRate / Values.DropRateScalar).ToString();
        }

        private void OnGroup(int group)
        {
            groupField.text = group.ToString();
        }

        private void OnOffsetField(string value)
        {
            if (Evaluator.TryFloat(value, out float offset))
            {
                gameplayData.AudioOffset.Value = Mathf.RoundToInt(offset);
            }
        }

        private void OnSpeedField(string value)
        {
            if (Evaluator.TryFloat(value, out float speed))
            {
                Settings.DropRate.Value = Mathf.RoundToInt(speed * Values.DropRateScalar);
            }
        }

        private void OnGroupField(string value)
        {
            if (Evaluator.TryInt(value, out int group))
            {
                Values.EditingTimingGroup.Value = group;
            }
        }
    }
}