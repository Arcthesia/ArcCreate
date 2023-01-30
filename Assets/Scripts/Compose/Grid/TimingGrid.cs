using System.Collections.Generic;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Chart;
using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    public class TimingGrid : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private GameObject beatlinePrefab;
        [SerializeField] private Transform beatlineParent;
        [SerializeField] private List<BeatlineColorSetting> beatlineColorSettings;
        [SerializeField] private Color beatlineFallbackColor;
        private BeatlineDisplay beatlineDisplay;
        private TimingGroup tg = null;

        private void UpdateBeatlines(int currentTiming)
        {
            if (tg != null)
            {
                double fp = tg.GetFloorPosition(currentTiming);
                beatlineDisplay.UpdateBeatlines(fp);
            }
        }

        private void Awake()
        {
            var pool = Pools.New<BeatlineBehaviour>(Values.BeatlinePoolName, beatlinePrefab, beatlineParent, 30);
            beatlineDisplay = new BeatlineDisplay(new EditorBeatlineGenerator(beatlineColorSettings, beatlineFallbackColor), pool);
            Values.EditingTimingGroup.OnValueChange += OnEditingTimingGroupChanged;
            Values.BeatlineDensity.OnValueChange += OnBeatlineDensityChanged;
            gameplayData.OnChartFileLoad += OnChartChange;
            gameplayData.OnChartEdit += OnChartChange;
            gameplayData.OnGameplayUpdate += UpdateBeatlines;
        }

        private void OnDestroy()
        {
            Pools.Destroy<BeatlineBehaviour>(Values.BeatlinePoolName);
            Values.EditingTimingGroup.OnValueChange -= OnEditingTimingGroupChanged;
            gameplayData.OnChartFileLoad -= OnChartChange;
            gameplayData.OnChartEdit -= OnChartChange;
        }

        private void OnChartChange()
        {
            beatlineDisplay.LoadFromTimingList(Values.EditingTimingGroup.Value);
            tg = Services.Gameplay.Chart.GetTimingGroup(Values.EditingTimingGroup.Value);
        }

        private void OnEditingTimingGroupChanged(int tgNum)
        {
            if (Services.Gameplay?.IsLoaded ?? false)
            {
                beatlineDisplay.LoadFromTimingList(tgNum);
                tg = Services.Gameplay.Chart.GetTimingGroup(tgNum);
            }
        }

        private void OnBeatlineDensityChanged(float density)
        {
            if (Services.Gameplay?.IsLoaded ?? false)
            {
                beatlineDisplay.LoadFromTimingList(Values.EditingTimingGroup.Value);
                tg = Services.Gameplay.Chart.GetTimingGroup(Values.EditingTimingGroup.Value);
            }
        }
    }
}