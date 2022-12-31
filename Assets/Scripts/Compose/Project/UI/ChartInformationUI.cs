using System.Collections.Generic;
using ArcCreate.Compose.Components;
using ArcCreate.Utility.Extension;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class ChartInformationUI : ChartMetadataUI
    {
        [SerializeField] private TMP_InputField title;
        [SerializeField] private TMP_InputField composer;
        [SerializeField] private TMP_InputField illustrator;
        [SerializeField] private TMP_InputField charter;
        [SerializeField] private TMP_InputField baseBpm;
        [SerializeField] private TMP_InputField chartConstant;
        [SerializeField] private TMP_InputField difficultyName;
        [SerializeField] private ColorInputField difficultyColor;

        [SerializeField] private StringSO titleSO;
        [SerializeField] private StringSO composerSO;
        [SerializeField] private StringSO illustratorSO;
        [SerializeField] private StringSO charterSO;
        [SerializeField] private StringSO difficultyNameSO;
        [SerializeField] private ColorSO difficultyColorSO;

        [SerializeField] private List<Button> diffColorPresets;

        protected override void ApplyChartSettings(ChartSettings chart)
        {
            title.text = chart.Title ?? "";
            composer.text = chart.Composer ?? "";
            illustrator.text = chart.Illustrator ?? "";
            charter.text = chart.Charter ?? "";
            baseBpm.text = chart.BaseBpm.ToString();
            chartConstant.text = chart.ChartConstant.ToString();
            difficultyName.text = chart.Difficulty?.ToString() ?? "";

            chart.DifficultyColor.ConvertHexToColor(out Color c);
            difficultyColor.SetValue(c);
        }

        private new void Start()
        {
            base.Start();
            title.onEndEdit.AddListener(OnTitle);
            composer.onEndEdit.AddListener(OnComposer);
            illustrator.onEndEdit.AddListener(OnIllustrator);
            charter.onEndEdit.AddListener(OnCharter);
            baseBpm.onEndEdit.AddListener(OnBaseBpm);
            chartConstant.onEndEdit.AddListener(OnChartConstant);
            difficultyName.onEndEdit.AddListener(OnDifficultyName);
            difficultyColor.OnValueChange += OnDifficultyColor;

            for (int i = 0; i < diffColorPresets.Count; i++)
            {
                Button btn = diffColorPresets[i];
                btn.onClick.AddListener(() =>
                {
                    OnDiffColorPreset(btn);
                });
            }
        }

        private void OnDestroy()
        {
            title.onEndEdit.RemoveListener(OnTitle);
            composer.onEndEdit.RemoveListener(OnComposer);
            illustrator.onEndEdit.RemoveListener(OnIllustrator);
            charter.onEndEdit.RemoveListener(OnCharter);
            baseBpm.onEndEdit.RemoveListener(OnBaseBpm);
            chartConstant.onEndEdit.RemoveListener(OnChartConstant);
            difficultyName.onEndEdit.RemoveListener(OnDifficultyName);
            difficultyColor.OnValueChange -= OnDifficultyColor;

            for (int i = 0; i < diffColorPresets.Count; i++)
            {
                Button btn = diffColorPresets[i];
                btn.onClick.RemoveAllListeners();
            }
        }

        private void OnTitle(string value)
        {
            Target.Title = value;
            titleSO.Value = value;
        }

        private void OnComposer(string value)
        {
            Target.Composer = value;
            composerSO.Value = value;
        }

        private void OnIllustrator(string value)
        {
            Target.Illustrator = value;
            illustratorSO.Value = value;
        }

        private void OnCharter(string value)
        {
            Target.Charter = value;
            charterSO.Value = value;
        }

        private void OnBaseBpm(string value)
        {
            if (Evaluator.TryFloat(value, out float bpm))
            {
                Target.BaseBpm = bpm;
                Services.Gameplay.Chart.BaseBpm = bpm;
            }
        }

        private void OnChartConstant(string value)
        {
            if (Evaluator.TryFloat(value, out float cc))
            {
                Target.ChartConstant = cc;
            }
        }

        private void OnDifficultyName(string value)
        {
            Target.Difficulty = value;
            difficultyNameSO.Value = value;
        }

        private void OnDifficultyColor(Color color)
        {
            Target.DifficultyColor = color.ConvertToHexCode();
            difficultyColorSO.Value = color;
        }

        private void OnDiffColorPreset(Button button)
        {
            int index = 0;
            for (int i = 0; i < diffColorPresets.Count; i++)
            {
                if (diffColorPresets[i] == button)
                {
                    index = i;
                    break;
                }
            }

            Color c = Services.Project.DefaultDifficultyColors[index];
            OnDifficultyColor(c);
            difficultyColor.SetValueWithoutNotify(c);
        }
    }
}