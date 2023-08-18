using ArcCreate.Data;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class HistoryDisplay : MonoBehaviour
    {
        [SerializeField] private StorageData storageData;
        [SerializeField] private PlayResultList bestList;
        [SerializeField] private PlayResultList recentList;
        [SerializeField] private Button showBestButton;
        [SerializeField] private Button showRecentButton;
        [SerializeField] private ScriptedAnimator bestListAnimator;
        [SerializeField] private ScriptedAnimator recentListAnimator;
        [SerializeField] private Color enabledColor;
        [SerializeField] private Color disabledColor;

        private void Awake()
        {
            storageData.SelectedChart.OnValueChange += OnStorageChange;
            showBestButton.onClick.AddListener(ShowBest);
            showRecentButton.onClick.AddListener(ShowRecent);
            OnStorageChange(storageData.SelectedChart.Value);
            ShowBest();
        }

        private void OnDestroy()
        {
            storageData.SelectedChart.OnValueChange -= OnStorageChange;
            showBestButton.onClick.RemoveListener(ShowBest);
            showRecentButton.onClick.RemoveListener(ShowRecent);
        }

        private void ShowBest()
        {
            bestListAnimator.Show();
            recentListAnimator.Hide();
            showBestButton.targetGraphic.color = enabledColor;
            showRecentButton.targetGraphic.color = disabledColor;
        }

        private void ShowRecent()
        {
            bestListAnimator.Hide();
            recentListAnimator.Show();
            showBestButton.targetGraphic.color = disabledColor;
            showRecentButton.targetGraphic.color = enabledColor;
        }

        private void OnStorageChange((LevelStorage level, ChartSettings chart) tuple)
        {
            var (level, chart) = tuple;
            if (level == null || chart == null)
            {
                return;
            }

            PlayHistory history = PlayHistory.GetHistoryForChart(level.Identifier, chart.ChartPath);
            bestList.Display(level, chart, history.TopScorePlays);
            recentList.Display(level, chart, history.RecentPlays);
        }
    }
}