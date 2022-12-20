using System;
using ArcCreate.Compose.Components;
using TMPro;
using UnityEngine;

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

        protected override void ApplyChartSettings(ChartSettings chart)
        {
            title.text = chart.Title ?? "";
            composer.text = chart.Composer ?? "";
            illustrator.text = chart.Illustrator ?? "";
            charter.text = chart.Charter ?? "";
            baseBpm.text = chart.BaseBpm.ToString();
            chartConstant.text = chart.ChartConstant.ToString();
            difficultyName.text = chart.Difficulty.ToString();
            difficultyColor.SetValue(chart.DifficultyColor);
        }

        private new void Awake()
        {
            base.Awake();
            title.onEndEdit.AddListener(OnTitle);
            composer.onEndEdit.AddListener(OnComposer);
            illustrator.onEndEdit.AddListener(OnIllustrator);
            charter.onEndEdit.AddListener(OnCharter);
            baseBpm.onEndEdit.AddListener(OnBaseBpm);
            chartConstant.onEndEdit.AddListener(OnChartConstant);
            difficultyName.onEndEdit.AddListener(OnDifficultyName);
            difficultyColor.OnValueChange += OnDifficultyColor;
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
        }

        private void OnTitle(string arg0)
        {
            throw new NotImplementedException();
        }

        private void OnComposer(string arg0)
        {
            throw new NotImplementedException();
        }

        private void OnIllustrator(string arg0)
        {
            throw new NotImplementedException();
        }

        private void OnCharter(string arg0)
        {
            throw new NotImplementedException();
        }

        private void OnBaseBpm(string arg0)
        {
            throw new NotImplementedException();
        }

        private void OnChartConstant(string arg0)
        {
            throw new NotImplementedException();
        }

        private void OnDifficultyName(string arg0)
        {
            throw new NotImplementedException();
        }

        private void OnDifficultyColor(Color obj)
        {
            throw new NotImplementedException();
        }
    }
}