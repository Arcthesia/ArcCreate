using System;
using System.Collections.Generic;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Utility.Extension;
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
        private readonly List<int> timingList = new List<int>();

        public int MoveTimingBackward(int sourceTiming)
        {
            int index = timingList.BisectLeft(sourceTiming);
            index -= 1;
            if (index < 0 || index > timingList.Count)
            {
                return sourceTiming;
            }

            return timingList[index];
        }

        public int MoveTimingForward(int sourceTiming)
        {
            int index = timingList.BisectRight(sourceTiming);
            if (index < 0 || index > timingList.Count)
            {
                return sourceTiming;
            }

            return timingList[index];
        }

        public int SnapToTimingGrid(int sourceTiming)
        {
            int indexLeft = timingList.BisectLeft(sourceTiming);
            int indexRight = timingList.BisectRight(sourceTiming);

            indexLeft = Mathf.Clamp(indexRight, 0, timingList.Count);
            indexRight = Mathf.Clamp(indexRight, 0, timingList.Count);

            int timingLeft = timingList[indexLeft];
            int timingRight = timingList[indexRight];

            if (Mathf.Abs(sourceTiming - timingLeft) < Mathf.Abs(sourceTiming - timingRight))
            {
                return timingLeft;
            }
            else
            {
                return timingRight;
            }
        }

        public void SetGridEnabled(bool value)
        {
            gameObject.SetActive(value);
        }

        public void Setup()
        {
            var pool = Pools.New<BeatlineBehaviour>(Values.BeatlinePoolName, beatlinePrefab, beatlineParent, 30);
            beatlineDisplay = new BeatlineDisplay(new EditorBeatlineGenerator(beatlineColorSettings, beatlineFallbackColor), pool);
            Values.EditingTimingGroup.OnValueChange += OnEditingTimingGroupChanged;
            Values.BeatlineDensity.OnValueChange += OnBeatlineDensityChanged;
            gameplayData.OnChartFileLoad += OnChartChange;
            gameplayData.OnChartTimingEdit += OnChartChange;
            gameplayData.OnGameplayUpdate += UpdateBeatlines;
        }

        private void OnDestroy()
        {
            Pools.Destroy<BeatlineBehaviour>(Values.BeatlinePoolName);
            Values.EditingTimingGroup.OnValueChange -= OnEditingTimingGroupChanged;
            gameplayData.OnChartFileLoad -= OnChartChange;
            gameplayData.OnChartTimingEdit -= OnChartChange;
        }

        private void UpdateBeatlines(int currentTiming)
        {
            if (tg != null)
            {
                double fp = tg.GetFloorPosition(currentTiming);
                beatlineDisplay.UpdateBeatlines(fp);
            }
        }

        private void OnChartChange()
        {
            LoadBeatline(Values.EditingTimingGroup.Value);
            tg = Services.Gameplay.Chart.GetTimingGroup(Values.EditingTimingGroup.Value);
        }

        private void OnEditingTimingGroupChanged(int tgNum)
        {
            if (Services.Gameplay?.IsLoaded ?? false)
            {
                LoadBeatline(tgNum);
                tg = Services.Gameplay.Chart.GetTimingGroup(tgNum);
            }
        }

        private void OnBeatlineDensityChanged(float density)
        {
            if (Services.Gameplay?.IsLoaded ?? false)
            {
                LoadBeatline(Values.EditingTimingGroup.Value);
                tg = Services.Gameplay.Chart.GetTimingGroup(Values.EditingTimingGroup.Value);
            }
        }

        private void LoadBeatline(int tg)
        {
            var beatlines = beatlineDisplay.LoadFromTimingGroup(tg);

            timingList.Clear();
            foreach (var beatline in beatlines)
            {
                timingList.Add(beatline.Timing);
            }

            timingList.Sort();
        }
    }
}