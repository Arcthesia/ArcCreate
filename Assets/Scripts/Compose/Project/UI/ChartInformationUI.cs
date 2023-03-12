using System.Collections.Generic;
using ArcCreate.Compose.Components;
using ArcCreate.Data;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Data;
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
        [SerializeField] private TMP_InputField alias;
        [SerializeField] private TMP_InputField baseBpm;
        [SerializeField] private Toggle syncBaseBpm;
        [SerializeField] private TMP_InputField chartOffset;
        [SerializeField] private TMP_InputField timingPointDensityFactor;
        [SerializeField] private TMP_InputField chartConstant;
        [SerializeField] private TMP_InputField difficultyName;
        [SerializeField] private ColorInputField difficultyColor;
        [SerializeField] private List<Button> diffColorPresets;

        protected override void ApplyChartSettings(ChartSettings chart)
        {
            title.text = chart.Title ?? "";
            composer.text = chart.Composer ?? "";
            illustrator.text = chart.Illustrator ?? "";
            charter.text = chart.Charter ?? "";
            alias.text = chart.Alias ?? "";
            baseBpm.text = chart.BaseBpm.ToString();
            syncBaseBpm.isOn = chart.SyncBaseBpm;
            chartConstant.text = chart.ChartConstant.ToString();
            difficultyName.text = chart.Difficulty ?? "";
            chartOffset.text = GameplayData.AudioOffset.Value.ToString();

            chart.DifficultyColor.ConvertHexToColor(out Color c);
            difficultyColor.SetValue(c);

            if (chart.SyncBaseBpm)
            {
                var baseGroup = Services.Gameplay.Chart.GetTimingGroup(0);
                var baseTiming = baseGroup.Timings[0];
                GameplayData.BaseBpm.Value = baseTiming.Bpm;
                chart.BaseBpm = baseTiming.Bpm;
            }
            else
            {
                GameplayData.BaseBpm.Value = chart.BaseBpm;
            }

            GameplayData.Title.Value = chart.Title ?? "";
            GameplayData.Composer.Value = chart.Composer ?? "";
            GameplayData.Illustrator.Value = chart.Illustrator ?? "";
            GameplayData.Charter.Value = chart.Charter ?? "";
            GameplayData.Alias.Value = chart.Alias ?? "";
            GameplayData.DifficultyName.Value = chart.Difficulty ?? "";
            GameplayData.DifficultyColor.Value = c;
        }

        private new void Start()
        {
            base.Start();
            title.onEndEdit.AddListener(OnTitle);
            composer.onEndEdit.AddListener(OnComposer);
            illustrator.onEndEdit.AddListener(OnIllustrator);
            charter.onEndEdit.AddListener(OnCharter);
            alias.onEndEdit.AddListener(OnAlias);
            baseBpm.onEndEdit.AddListener(OnBaseBpm);
            syncBaseBpm.onValueChanged.AddListener(OnSyncBaseBPM);
            chartConstant.onEndEdit.AddListener(OnChartConstant);
            difficultyName.onEndEdit.AddListener(OnDifficultyName);
            difficultyColor.OnValueChange += OnDifficultyColor;

            GameplayData.AudioOffset.OnValueChange += OnGameplayAudioOffset;
            chartOffset.onEndEdit.AddListener(OnChartOffset);

            GameplayData.TimingPointDensityFactor.OnValueChange += OnGameplayDensityFactor;
            timingPointDensityFactor.onEndEdit.AddListener(OnDensityFactor);

            GameplayData.OnChartTimingEdit += OnChartTimingEdit;
            GameplayData.BaseBpm.OnValueChange += OnGameplayBaseBpm;

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
            alias.onEndEdit.RemoveListener(OnAlias);
            baseBpm.onEndEdit.RemoveListener(OnBaseBpm);
            syncBaseBpm.onValueChanged.RemoveListener(OnSyncBaseBPM);
            chartConstant.onEndEdit.RemoveListener(OnChartConstant);
            difficultyName.onEndEdit.RemoveListener(OnDifficultyName);
            difficultyColor.OnValueChange -= OnDifficultyColor;

            GameplayData.AudioOffset.OnValueChange -= OnGameplayAudioOffset;
            chartOffset.onEndEdit.RemoveListener(OnChartOffset);

            GameplayData.TimingPointDensityFactor.OnValueChange -= OnGameplayDensityFactor;
            timingPointDensityFactor.onEndEdit.RemoveListener(OnDensityFactor);

            GameplayData.OnChartTimingEdit -= OnChartTimingEdit;
            GameplayData.BaseBpm.OnValueChange -= OnGameplayBaseBpm;

            for (int i = 0; i < diffColorPresets.Count; i++)
            {
                Button btn = diffColorPresets[i];
                btn.onClick.RemoveAllListeners();
            }
        }

        private void OnGameplayAudioOffset(int obj)
        {
            chartOffset.SetTextWithoutNotify(obj.ToString());
        }

        private void OnChartOffset(string value)
        {
            if (Evaluator.TryInt(value, out int offset))
            {
                GameplayData.AudioOffset.Value = offset;

                Values.ProjectModified = true;
            }
        }

        private void OnGameplayDensityFactor(float obj)
        {
            timingPointDensityFactor.SetTextWithoutNotify(obj.ToString());
        }

        private void OnDensityFactor(string value)
        {
            if (Evaluator.TryFloat(value, out float factor))
            {
                GameplayData.TimingPointDensityFactor.Value = factor;

                Values.ProjectModified = true;
            }
        }

        private void OnTitle(string value)
        {
            Target.Title = value;
            GameplayData.Title.Value = value;

            Values.ProjectModified = true;
        }

        private void OnComposer(string value)
        {
            Target.Composer = value;
            GameplayData.Composer.Value = value;

            Values.ProjectModified = true;

            if (Settings.EnableEasterEggs.Value)
            {
                if (Target.ChartConstant >= 11 && Target.Composer.Contains("xi"))
                {
                    EasterEggs.TryTrigger("xixi");
                }

                if (Target.Composer.ToLower() == "bts")
                {
                    EasterEggs.TryTrigger("btstan");
                }

                if (Target.Composer.ToLower().Contains("camellia") || Target.Composer.Contains("かめりあ"))
                {
                    EasterEggs.TryTrigger("camellia");
                }
            }
        }

        private void OnIllustrator(string value)
        {
            Target.Illustrator = value;
            GameplayData.Illustrator.Value = value;

            Values.ProjectModified = true;
        }

        private void OnCharter(string value)
        {
            Target.Charter = value;
            GameplayData.Charter.Value = value;

            Values.ProjectModified = true;
        }

        private void OnAlias(string value)
        {
            Target.Alias = value;
            GameplayData.Alias.Value = value;

            Values.ProjectModified = true;
        }

        private void OnGameplayBaseBpm(float bpm)
        {
            Target.BaseBpm = bpm;
            baseBpm.SetTextWithoutNotify(bpm.ToString());

            Values.ProjectModified = true;

            if (Target.SyncBaseBpm)
            {
                MirrorBaseBpmToTimingList();
            }
        }

        private void OnBaseBpm(string value)
        {
            if (Evaluator.TryFloat(value, out float bpm))
            {
                Target.BaseBpm = bpm;
                GameplayData.BaseBpm.Value = bpm;

                Values.ProjectModified = true;
            }

            if (Settings.EnableEasterEggs.Value && Target.BaseBpm >= 300)
            {
                EasterEggs.TryTrigger("sdvx");
            }
        }

        private void OnSyncBaseBPM(bool value)
        {
            Target.SyncBaseBpm = value;

            Values.ProjectModified = true;

            if (value)
            {
                MirrorBaseBpmToTimingList();
            }
        }

        private void OnChartConstant(string value)
        {
            if (Evaluator.TryFloat(value, out float cc))
            {
                Target.ChartConstant = cc;

                Values.ProjectModified = true;
            }

            if (Settings.EnableEasterEggs.Value)
            {
                if (Target.ChartConstant >= 11 && Target.Composer.Contains("xi"))
                {
                    EasterEggs.TryTrigger("xixi");
                }

                if (Target.ChartConstant >= 13)
                {
                    EasterEggs.TryTrigger("overshart");
                }
            }
        }

        private void OnDifficultyName(string value)
        {
            Target.Difficulty = value;
            GameplayData.DifficultyName.Value = value;

            Values.ProjectModified = true;
        }

        private void OnDifficultyColor(Color color)
        {
            Target.DifficultyColor = color.ConvertToHexCode();
            GameplayData.DifficultyColor.Value = color;

            Values.ProjectModified = true;
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

            Values.ProjectModified = true;
        }

        private void OnChartTimingEdit()
        {
            if (Target.SyncBaseBpm)
            {
                var baseGroup = Services.Gameplay.Chart.GetTimingGroup(0);
                var baseTiming = baseGroup.Timings[0];
                GameplayData.BaseBpm.Value = baseTiming.Bpm;

                Values.ProjectModified = true;
            }
        }

        private void MirrorBaseBpmToTimingList()
        {
            float bpm = Target.BaseBpm;

            var baseGroup = Services.Gameplay.Chart.GetTimingGroup(0);
            TimingEvent baseTiming = baseGroup.Timings[0];

            baseTiming.Bpm = bpm;
            GameplayData.OnChartTimingEdit -= OnChartTimingEdit;
            Services.Gameplay.Chart.UpdateEvents(new List<TimingEvent> { baseTiming });
            GameplayData.OnChartTimingEdit += OnChartTimingEdit;
        }
    }
}