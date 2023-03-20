using System;
using System.Collections.Generic;
using System.Threading;
using ArcCreate.Data;
using ArcCreate.SceneTransition;
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
        [SerializeField] private Button switchDiffButton;

        [Header("Difficulty")]
        [SerializeField] private TMP_Text currDiffName;
        [SerializeField] private TMP_Text[] diffNumbers;
        [SerializeField] private DifficultyCell[] diffColors;
        [SerializeField] private Color inactiveDiffColor;

        [Header("Images")]
        [SerializeField] private RawImage jacket;
        [SerializeField] private RawImage[] backgrounds;
        [SerializeField] private Image side;
        [SerializeField] private Sprite[] sideSprites;
        [SerializeField] private Texture[] defaultBackgrounds;

        [Header("Shutter")]
        [SerializeField] private SpriteSO shutterJacketSprite;
        [SerializeField] private StringSO shutterTitle;
        [SerializeField] private StringSO shutterComposer;
        [SerializeField] private StringSO shutterIllustrator;
        [SerializeField] private StringSO shutterCharter;
        [SerializeField] private StringSO shutterAlias;
        [SerializeField] private SpriteSO shutterJacketShadowSprite;
        [SerializeField] private Sprite[] jacketShadowSprites;

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
            storage.OnSwitchToGameplaySceneException += OnGameplayException;
        }

        private void OnDestroy()
        {
            storage.SelectedChart.OnValueChange -= OnChartChange;
            storage.OnStorageChange -= OnStorageChange;
            switchDiffButton.onClick.AddListener(SwitchDifficulty);
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
            if (level == null || chart == null)
            {
                return;
            }

            title.text = string.IsNullOrEmpty(chart.Title) ? I18n.S("Gameplay.Selection.Info.Undefined.Title") : chart.Title;
            composer.text = string.IsNullOrEmpty(chart.Composer) ? I18n.S("Gameplay.Selection.Info.Undefined.Composer") : chart.Composer;
            bpm.text = string.IsNullOrEmpty(chart.BpmText) ? "BPM: " + chart.BaseBpm.ToString() : chart.BpmText;
            score.text = "Coming soon";
            charter.text = string.IsNullOrEmpty(chart.Charter) ? I18n.S("Gameplay.Selection.Info.Undefined.Charter") : I18n.S("Gameplay.Selection.Info.Charter", chart.Charter);

            shutterTitle.Value = title.text;
            shutterComposer.Value = composer.text;
            shutterIllustrator.Value = chart.Illustrator;
            shutterCharter.Value = chart.Charter;
            shutterAlias.Value = chart.Alias;

            string sideString = (chart.Skin?.Side ?? "").ToLower();
            storage.AssignTexture(jacket, level, chart.JacketPath).ContinueWith(() =>
            {
                if (jacketSprite != null)
                {
                    Destroy(jacketSprite);
                }

                Texture texture = jacket.texture;
                jacketSprite = Sprite.Create(texture as Texture2D, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                shutterJacketSprite.Value = jacketSprite;
            });
            foreach (var bg in backgrounds)
            {
                switch (sideString)
                {
                    case "light":
                        bg.texture = defaultBackgrounds[0];
                        break;
                    case "conflict":
                        bg.texture = defaultBackgrounds[1];
                        break;
                    case "colorless":
                        bg.texture = defaultBackgrounds[2];
                        break;
                    default:
                        bg.texture = defaultBackgrounds[0];
                        break;
                }
            }

            switch (sideString)
            {
                case "light":
                    side.sprite = sideSprites[0];
                    shutterJacketShadowSprite.Value = jacketShadowSprites[0];
                    break;
                case "conflict":
                    side.sprite = sideSprites[1];
                    shutterJacketShadowSprite.Value = jacketShadowSprites[1];
                    break;
                case "colorless":
                    side.sprite = sideSprites[2];
                    shutterJacketShadowSprite.Value = jacketShadowSprites[2];
                    break;
                default:
                    side.sprite = sideSprites[0];
                    shutterJacketShadowSprite.Value = jacketShadowSprites[0];
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

            ChartSettings next = level.Settings.Charts[(indexOfCurrentChart + 1) % level.Settings.Charts.Count];
            storage.SelectedChart.Value = (level, next);
        }
    }
}