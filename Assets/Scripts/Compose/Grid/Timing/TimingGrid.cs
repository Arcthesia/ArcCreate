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
        [SerializeField] private MeshCollider laneCollider;
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
            int indexMid = timingList.BisectLeft(sourceTiming);
            int indexLeft = indexMid - 1;
            int indexRight = timingList.BisectRight(sourceTiming);

            indexLeft = Mathf.Clamp(indexLeft, 0, timingList.Count);
            indexMid = Mathf.Clamp(indexMid, 0, timingList.Count);
            indexRight = Mathf.Clamp(indexRight, 0, timingList.Count);

            int timingLeft = timingList[indexLeft];
            int timingMid = timingList[indexMid];
            int timingRight = timingList[indexRight];

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

            Destroy(laneCollider.sharedMesh);
            float worldFrom = ArcFormula.LaneToWorldX(laneFrom) + (Gameplay.Values.LaneWidth / 2f);
            float worldTo = ArcFormula.LaneToWorldX(laneTo) - (Gameplay.Values.LaneWidth / 2f);

            Values.LaneFromX = worldFrom;
            Values.LaneToX = worldTo;

            beatlineParent.localPosition = new Vector3((worldTo + worldFrom) / 2, 0, 0);
            beatlineParent.localScale = new Vector3(Mathf.Abs(worldTo - worldFrom) / (4 * Gameplay.Values.LaneWidth), 1, 1);

            laneCollider.sharedMesh = MeshBuilder.BuildQuadMeshLane(worldFrom, worldTo);
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

            laneCollider.sharedMesh = Instantiate(laneCollider.sharedMesh);
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