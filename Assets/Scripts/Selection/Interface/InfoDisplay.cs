using System;
using System.Threading;
using ArcCreate.Data;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class InfoDisplay : MonoBehaviour
    {
        [SerializeField] private StorageData storage;

        [Header("Info")]
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text composer;
        [SerializeField] private TMP_Text bpm;
        [SerializeField] private TMP_Text score;
        [SerializeField] private TMP_Text charter;

        [Header("Difficulty")]
        [SerializeField] private TMP_Text currDiffName;
        [SerializeField] private TMP_Text[] diffNumbers;
        [SerializeField] private DifficultyCell[] diffColors;
        [SerializeField] private Color inactiveDiffColor;

        [Header("Images")]
        [SerializeField] private RawImage jacket;
        [SerializeField] private Image side;
        [SerializeField] private Sprite[] sideSprites;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private void Awake()
        {
            storage.SelectedChart.OnValueChange += OnChartChange;
            storage.OnStorageChange += OnStorageChange;
        }

        private void OnDestroy()
        {
            storage.SelectedChart.OnValueChange -= OnChartChange;
            storage.OnStorageChange -= OnStorageChange;
            cts.Cancel();
        }

        private void OnStorageChange()
        {
            OnChartChange(storage.SelectedChart.Value);
        }

        private void OnChartChange((LevelStorage level, ChartSettings chart) obj)
        {
            var (level, chart) = obj;
            if (level == null || chart == null)
            {
                return;
            }

            title.text = string.IsNullOrEmpty(chart.Title) ? I18n.S("Gameplay.Selection.Info.UndefinedTitle") : chart.Title;
            composer.text = string.IsNullOrEmpty(chart.Composer) ? I18n.S("Gameplay.Selection.Info.UndefinedComposer") : chart.Composer;
            bpm.text = string.IsNullOrEmpty(chart.BpmText) ? "BPM: " + chart.BaseBpm.ToString() : chart.BpmText;
            score.text = "Coming soon";
            charter.text = string.IsNullOrEmpty(chart.Charter) ? I18n.S("Gameplay.Undefined.Charter") : I18n.S("Gameplay.Selection.Info.Charter", chart.Charter);

            storage.AssignTexture(jacket, level, chart.JacketPath, cts.Token).Forget();

            switch ((chart.Skin?.Side ?? "").ToLower())
            {
                case "light":
                    side.sprite = sideSprites[0];
                    break;
                case "conflict":
                    side.sprite = sideSprites[1];
                    break;
                case "colorless":
                    side.sprite = sideSprites[2];
                    break;
                default:
                    side.sprite = sideSprites[0];
                    break;
            }

            (string currDiffName, string currDiffNum) = chart.ParseDifficultyName(maxNumberLength: 3);
            this.currDiffName.text = currDiffName.ToUpper();
            diffNumbers[0].text = InterfaceUtility.AlignedDiffNumber(currDiffNum);

            ColorUtility.TryParseHtmlString(chart.DifficultyColor, out Color currDiffColor);
            diffColors[0].Color = currDiffColor;

            int i = 1;

            int indexOfCurrentChart = 0;
            for (int j = 0; j < level.Settings.Charts.Count; j++)
            {
                ChartSettings otherChart = level.Settings.Charts[j];
                if (otherChart == chart)
                {
                    indexOfCurrentChart = j;
                    break;
                }
            }

            for (int j = 1; j < level.Settings.Charts.Count; j++)
            {
                ChartSettings otherChart = level.Settings.Charts[(j + indexOfCurrentChart) % level.Settings.Charts.Count];
                if (otherChart == chart)
                {
                    break;
                }

                (string diffName, string diffNum) = chart.ParseDifficultyName(maxNumberLength: 3);
                ColorUtility.TryParseHtmlString(otherChart.DifficultyColor, out Color diffColor);

                diffNumbers[i].gameObject.SetActive(true);
                diffNumbers[i].text = InterfaceUtility.AlignedDiffNumber(diffNum);
                diffColors[i].Color = diffColor;

                i += 1;
                if (i >= diffNumbers.Length)
                {
                    break;
                }
            }

            for (; i < diffNumbers.Length; i++)
            {
                diffNumbers[i].gameObject.SetActive(false);
                diffColors[i].Color = inactiveDiffColor;
            }
        }
    }
}