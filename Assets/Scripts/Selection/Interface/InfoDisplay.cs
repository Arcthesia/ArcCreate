using System;
using System.Collections.Generic;
using System.Threading;
using ArcCreate.Data;
using ArcCreate.SceneTransition;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility;
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
        [SerializeField] private TMP_Text charter;
        [SerializeField] private Button switchDiffButton;
        [SerializeField] private Button nextDiffButton;
        [SerializeField] private Button nextNextDiffButton;

        [Header("Difficulty")]
        [SerializeField] private TMP_Text currDiffName;
        [SerializeField] private TMP_Text[] diffNumbers;
        [SerializeField] private DifficultyCell[] diffColors;
        [SerializeField] private Color inactiveDiffColor;

        [Header("Images")]
        [SerializeField] private RawImage jacket;

        [Header("Score")]
        [SerializeField] private TMP_Text score;
        [SerializeField] private GradeDisplay gradeDisplay;

        [Header("Transition")]
        [SerializeField] private SpriteSO transitionJacketSprite;
        [SerializeField] private StringSO transitionTitle;
        [SerializeField] private StringSO transitionComposer;
        [SerializeField] private StringSO transitionIllustrator;
        [SerializeField] private StringSO transitionCharter;
        [SerializeField] private StringSO transitionAlias;
        [SerializeField] private StringSO transitionDifficulty;
        [SerializeField] private ColorSO transitionDifficultyColor;
        [SerializeField] private ThemeGroup themeGroup;

        [Header("Exception")]
        [SerializeField] private Dialog exceptionDialog;
        [SerializeField] private TMP_Text exceptionText;
        private Sprite jacketSprite;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private void Awake()
        {
            storage.SelectedChart.OnValueChange += OnChartChange;
            storage.OnStorageChange += OnStorageChange;
            switchDiffButton.onClick.AddListener(SwitchDifficulty);
            nextDiffButton.onClick.AddListener(SwitchDifficulty);
            nextNextDiffButton.onClick.AddListener(SwitchNextDifficulty);
            storage.OnSwitchToGameplaySceneException += OnGameplayException;

            if (storage.IsLoaded)
            {
                OnStorageChange();
            }
        }

        private void OnDestroy()
        {
            storage.SelectedChart.OnValueChange -= OnChartChange;
            storage.OnStorageChange -= OnStorageChange;
            switchDiffButton.onClick.AddListener(SwitchDifficulty);
            nextDiffButton.onClick.RemoveListener(SwitchDifficulty);
            nextNextDiffButton.onClick.RemoveListener(SwitchNextDifficulty);
            storage.OnSwitchToGameplaySceneException -= OnGameplayException;
            cts.Cancel();
        }

        private void OnGameplayException(Exception e)
        {
            exceptionDialog.Show();
            exceptionText.text = I18n.S("Gameplay.Exception.Load", new Dictionary<string, object>()
            {
                { "Identifier", storage.SelectedChart.Value.level?.Identifier ?? "unknown" },
                { "ChartPath", storage.SelectedChart.Value.chart?.ChartPath ?? "unknown" },
                { "Message", e.Message },
                { "StackTrace", e.StackTrace },
            });
        }

        private void OnStorageChange()
        {
            OnChartChange(storage.SelectedChart.Value);
        }

        private void OnChartChange((LevelStorage level, ChartSettings chart) obj)
        {
            var (level, chart) = obj;
            PlayHistory history = PlayHistory.GetHistoryForChart(level.Identifier, chart.ChartPath);
            if (level == null || chart == null)
            {
                return;
            }

            title.text = string.IsNullOrEmpty(chart.Title) ? I18n.S("Gameplay.Selection.Info.Undefined.Title") : chart.Title;
            composer.text = string.IsNullOrEmpty(chart.Composer) ? I18n.S("Gameplay.Selection.Info.Undefined.Composer") : chart.Composer;
            bpm.text = string.IsNullOrEmpty(chart.BpmText) ? "BPM: " + chart.BaseBpm.ToString() : "BPM: " + chart.BpmText;
            charter.text = string.IsNullOrEmpty(chart.Charter) ? I18n.S("Gameplay.Selection.Info.Undefined.Charter") : I18n.S("Gameplay.Selection.Info.Charter", chart.Charter);

            transitionTitle.Value = title.text;
            transitionComposer.Value = composer.text;
            transitionIllustrator.Value = chart.Illustrator;
            transitionCharter.Value = chart.Charter;
            transitionAlias.Value = chart.Alias;
            transitionDifficulty.Value = chart.Difficulty;

            PlayResult playResult = history.BestScorePlayOrDefault;
            score.text = playResult.FormattedScore;
            gradeDisplay.Display(playResult.Grade);
            gradeDisplay.gameObject.SetActive(history.PlayCount > 0);

            string sideString = (chart.Skin?.Side ?? "").ToLower();

            if (jacket.texture != null)
            {
                storage.ReleasePersistent(jacket.texture);
            }

            storage.AssignTexture(jacket, level, chart.JacketPath).ContinueWith(() =>
            {
                if (jacketSprite != null)
                {
                    Destroy(jacketSprite);
                }

                Texture texture = jacket.texture;
                storage.EnsurePersistent(texture);
                jacketSprite = Sprite.Create(texture as Texture2D, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                transitionJacketSprite.Value = jacketSprite;
            });

            switch (sideString)
            {
                case "conflict":
                    themeGroup.Value = Theme.Dark;
                    break;
                case "light":
                case "colorless":
                default:
                    themeGroup.Value = Theme.Light;
                    break;
            }

            (string currDiffName, string currDiffNum) = chart.ParseDifficultyName(maxNumberLength: 3);
            this.currDiffName.text = currDiffName.ToUpper();
            diffNumbers[0].text = InterfaceUtility.AlignedDiffNumber(currDiffNum);

            ColorUtility.TryParseHtmlString(chart.DifficultyColor, out Color currDiffColor);
            diffColors[0].Color = currDiffColor;
            transitionDifficultyColor.Value = currDiffColor;

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

                (string diffName, string diffNum) = otherChart.ParseDifficultyName(maxNumberLength: 3);
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

        private void SwitchDifficulty()
        {
            ChangeDifficulty(distance: 1);
        }

        private void SwitchNextDifficulty()
        {
            ChangeDifficulty(distance: 2);
        }

        private void ChangeDifficulty(int distance)
        {
            var (level, chart) = storage.SelectedChart.Value;
            if (level == null)
            {
                return;
            }

            if (chart == null)
            {
                storage.SelectedChart.Value = (level, level.Settings.Charts[0]);
            }

            int indexOfCurrentChart = 0;
            for (int i = 0; i < level.Settings.Charts.Count; i++)
            {
                ChartSettings otherChart = level.Settings.Charts[i];
                if (otherChart == chart)
                {
                    indexOfCurrentChart = i;
                    break;
                }
            }

            ChartSettings next = level.Settings.Charts[(indexOfCurrentChart + distance) % level.Settings.Charts.Count];
            storage.SelectedChart.Value = (level, next);
        }
    }
}