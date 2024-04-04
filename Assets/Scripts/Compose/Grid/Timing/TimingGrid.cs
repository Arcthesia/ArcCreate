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
        [SerializeField] private Color beatlineBarColor;
        private BeatlineDisplay beatlineDisplay;
        private TimingGroup tg = null;
        private readonly List<Beatline> beatlineList = new List<Beatline>();

        public int MoveTimingBackward(int sourceTiming)
        {
            int index = beatlineList.BisectLeft(sourceTiming, b => b.Timing);
            index -= 1;
            if (index < 0 || index >= beatlineList.Count)
            {
                return sourceTiming;
            }

            return beatlineList[index].Timing;
        }

        public int MoveTimingForward(int sourceTiming)
        {
            int index = beatlineList.BisectRight(sourceTiming, b => b.Timing);
            if (index < 0 || index >= beatlineList.Count)
            {
                return sourceTiming;
            }

            return beatlineList[index].Timing;
        }

        public int MoveTimingBackwardByBeat(int sourceTiming)
        {
            int index = beatlineList.BisectLeft(sourceTiming, b => b.Timing);
            index -= 1;
            if (index < 0 || index >= beatlineList.Count)
            {
                return sourceTiming;
            }

            Color match = default;
            foreach (var settings in beatlineColorSettings)
            {
                if (settings.Density == 1)
                {
                    match = settings.Color;
                }
            }

            while (index > 0 && beatlineList[index].Color != match)
            {
                index -= 1;
            }

            if (beatlineList[index].Color == match)
            {
                return beatlineList[index].Timing;
            }
            else
            {
                return sourceTiming;
            }
        }

        public int MoveTimingForwardByBeat(int sourceTiming)
        {
            int index = beatlineList.BisectRight(sourceTiming, b => b.Timing);
            if (index < 0 || index >= beatlineList.Count)
            {
                return sourceTiming;
            }

            Color match = default;
            foreach (var settings in beatlineColorSettings)
            {
                if (settings.Density == 1)
                {
                    match = settings.Color;
                }
            }

            while (index > 0 && beatlineList[index].Color != match)
            {
                index += 1;
            }

            if (beatlineList[index].Color == match)
            {
                return beatlineList[index].Timing;
            }
            else
            {
                return sourceTiming;
            }
        }

        public int SnapToTimingGrid(int sourceTiming)
        {
            int indexMid = beatlineList.BisectLeft(sourceTiming, b => b.Timing);
            int indexLeft = indexMid - 1;
            int indexRight = beatlineList.BisectRight(sourceTiming, b => b.Timing);

            indexLeft = Mathf.Clamp(indexLeft, 0, beatlineList.Count);
            indexMid = Mathf.Clamp(indexMid, 0, beatlineList.Count);
            indexRight = Mathf.Clamp(indexRight, 0, beatlineList.Count);

            int timingLeft = beatlineList[indexLeft].Timing;
            int timingMid = beatlineList[indexMid].Timing;
            int timingRight = beatlineList[indexRight].Timing;

            int diffLeft = Mathf.Abs(sourceTiming - timingLeft);
            int diffMid = Mathf.Abs(sourceTiming - timingMid);
            int diffRight = Mathf.Abs(sourceTiming - timingRight);

            if (diffLeft <= diffMid && diffLeft <= diffRight)
            {
                return timingLeft;
            }
            else if (diffMid <= diffLeft && diffMid <= diffRight)
            {
                return timingMid;
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

        public void LoadGridSettings(int laneFrom, int laneTo)
        {
            if (laneFrom > laneTo)
            {
                (laneFrom, laneTo) = (laneTo, laneFrom);
            }

            float worldFrom = ArcFormula.LaneToWorldX(laneFrom) + (Gameplay.Values.LaneWidth / 2f);
            float worldTo = ArcFormula.LaneToWorldX(laneTo) - (Gameplay.Values.LaneWidth / 2f);
            if (worldFrom > worldTo)
            {
                (worldFrom, worldTo) = (worldTo, worldFrom);
            }

            Values.LaneFromX = worldFrom;
            Values.LaneToX = worldTo;

            beatlineParent.localPosition = new Vector3((worldTo + worldFrom) / 2, 0, 0);
            beatlineParent.localScale = new Vector3(Mathf.Abs(worldTo - worldFrom) / (4 * Gameplay.Values.LaneWidth), 1, 1);
        }

        public void Setup()
        {
            var pool = Pools.New<BeatlineBehaviour>(Values.BeatlinePoolName, beatlinePrefab, beatlineParent, 30);
            beatlineDisplay = new BeatlineDisplay(new EditorBeatlineGenerator(beatlineColorSettings, beatlineFallbackColor, beatlineBarColor), pool);
            Values.EditingTimingGroup.OnValueChange += OnEditingTimingGroupChanged;
            Values.BeatlineDensity.OnValueChange += OnBeatlineDensityChanged;
            gameplayData.OnChartFileLoad += OnChartChange;
            gameplayData.AudioClip.OnValueChange += OnAudioChange;
            gameplayData.OnChartTimingEdit += OnChartChange;
            gameplayData.OnGameplayUpdate += UpdateBeatlines;
        }

        public (float fromX, float fromZ, float toX, float toZ) GetBounds()
        {
            return (Values.LaneFromX, -Gameplay.Values.TrackLengthForward, Values.LaneToX, Gameplay.Values.TrackLengthBackward);
        }

        private void OnDestroy()
        {
            Pools.Destroy<BeatlineBehaviour>(Values.BeatlinePoolName);
            Values.EditingTimingGroup.OnValueChange -= OnEditingTimingGroupChanged;
            gameplayData.OnChartFileLoad -= OnChartChange;
            gameplayData.AudioClip.OnValueChange += OnAudioChange;
            gameplayData.OnChartTimingEdit -= OnChartChange;
            gameplayData.OnGameplayUpdate -= UpdateBeatlines;
        }

        private void UpdateBeatlines(int currentTiming)
        {
            if (tg != null)
            {
                double fp = tg.GetFloorPosition(currentTiming);
                beatlineDisplay.UpdateBeatlines(fp);
            }
        }

        private void OnAudioChange(AudioClip obj)
        {
            OnChartChange();
        }

        private void OnChartChange()
        {
            LoadBeatline(Values.EditingTimingGroup.Value, gameplayData.AudioClip.Value);
            tg = Services.Gameplay.Chart.GetTimingGroup(Values.EditingTimingGroup.Value);
        }

        private void OnEditingTimingGroupChanged(int tgNum)
        {
            if (Services.Gameplay?.IsLoaded ?? false)
            {
                LoadBeatline(tgNum, gameplayData.AudioClip.Value);
                tg = Services.Gameplay.Chart.GetTimingGroup(tgNum);
            }
        }

        private void OnBeatlineDensityChanged(float density)
        {
            if (Services.Gameplay?.IsLoaded ?? false)
            {
                LoadBeatline(Values.EditingTimingGroup.Value, gameplayData.AudioClip.Value);
                tg = Services.Gameplay.Chart.GetTimingGroup(Values.EditingTimingGroup.Value);
            }
        }

        private void LoadBeatline(int tg, AudioClip clip)
        {
            int length = 0;
            if (clip != null)
            {
                length = Mathf.RoundToInt(clip.length * 1000);
            }

            var beatlines = beatlineDisplay.LoadFromTimingGroup(tg, length);

            beatlineList.Clear();
            foreach (var beatline in beatlines)
            {
                beatlineList.Add(beatline);
            }

            beatlineList.Sort((a, b) => a.Timing.CompareTo(b.Timing));
        }
    }
}