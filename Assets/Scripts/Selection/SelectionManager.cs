using System;
using ArcCreate.Data;
using ArcCreate.Gameplay;
using ArcCreate.SceneTransition;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using UnityEngine;

namespace ArcCreate.Selection
{
    public class SelectionManager : SceneRepresentative
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private StorageData storageData;
        [SerializeField] private Camera selectionCamera;

        protected override void OnSceneLoad()
        {
            storageData.SelectedPack.OnValueChange += OnPackChange;
            storageData.SelectedChart.OnValueChange += OnChartChange;
            TransitionScene.Instance.TriangleTileGameObject.SetActive(true);
            TransitionScene.Instance.UpdateCameraStatus();
            TransitionScene.Instance.EnsureDefaultTriangleScale();
        }

        private void OnDestroy()
        {
            storageData.SelectedPack.OnValueChange -= OnPackChange;
            storageData.SelectedChart.OnValueChange -= OnChartChange;
        }

        private void OnPackChange(PackStorage pack)
        {
            PlayerPrefs.SetString("Selection.LastPack", pack?.Identifier);
        }

        private void OnChartChange((LevelStorage level, ChartSettings chart) obj)
        {
            var (level, chart) = obj;
            if (level != null && chart != null)
            {
                PlayerPrefs.SetString($"Selection.LastLevel.{storageData.SelectedPack.Value?.Identifier ?? "all"}", level.Identifier);
                PlayerPrefs.SetString("Selection.LastChartPath", chart.ChartPath);
                PlayerPrefs.SetString("Selection.LastDifficultyName", chart.Difficulty);
                PlayerPrefs.SetFloat("Selection.LastCc", (float)chart.ChartConstant);
            }
        }
    }
}