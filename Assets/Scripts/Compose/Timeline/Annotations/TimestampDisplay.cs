using System.Collections.Generic;
using ArcCreate.Data;
using ArcCreate.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Timeline
{
    public class TimestampDisplay : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private GameObject timestampPrefab;
        [SerializeField] private RectTransform timestampParent;
        [SerializeField] private Button showTimestampButton;
        [SerializeField] private Button hideTimestampButton;
        [SerializeField] private GameObject timestampDetail;
        [SerializeField] private Button newTimestamp;
        [SerializeField] private Button deleteTimestamp;
        [SerializeField] private Button jumpToNext;
        [SerializeField] private Button jumpToPrevious;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private int timestampCapacity;

        private Pool<TimestampMarker> pool;

        public void JumpToNext()
        {
            if (!TryGetTimestampList(out List<Timestamp> timestamps))
            {
                return;
            }

            if (timestamps.Count <= 0)
            {
                return;
            }

            int currentTiming = Services.Gameplay.Audio.AudioTiming;
            for (int i = 0; i < timestamps.Count - 1; i++)
            {
                Timestamp curr = timestamps[i];
                Timestamp next = timestamps[i + 1];
                if (curr.Timing <= currentTiming && currentTiming < next.Timing)
                {
                    Services.Gameplay.Audio.AudioTiming = next.Timing;
                    return;
                }
            }

            Services.Gameplay.Audio.AudioTiming = timestamps[0].Timing;
        }

        public void JumpToPrevious()
        {
            if (!TryGetTimestampList(out List<Timestamp> timestamps))
            {
                return;
            }

            if (timestamps.Count <= 0)
            {
                return;
            }

            int currentTiming = Services.Gameplay.Audio.AudioTiming;
            int offset = Services.Gameplay.Audio.IsPlaying ? 1000 : 0;
            for (int i = 0; i < timestamps.Count - 1; i++)
            {
                Timestamp curr = timestamps[i];
                Timestamp next = timestamps[i + 1];
                if (curr.Timing < currentTiming + offset && currentTiming <= next.Timing + offset)
                {
                    Services.Gameplay.Audio.AudioTiming = curr.Timing;
                    return;
                }
            }

            Services.Gameplay.Audio.AudioTiming = timestamps[timestamps.Count - 1].Timing;
        }

        public void AddTimestamp()
        {
            ProjectSettings proj = Services.Project.CurrentProject;
            ChartSettings chart = Services.Project.CurrentChart;
            if (proj == null || chart == null)
            {
                return;
            }

            proj.EditorSettings = proj.EditorSettings ?? new EditorProjectSettings();
            proj.EditorSettings.Timestamps = proj.EditorSettings.Timestamps ?? new Dictionary<string, List<Timestamp>>();
            if (!proj.EditorSettings.Timestamps.TryGetValue(chart.ChartPath, out List<Timestamp> timestamps))
            {
                timestamps = new List<Timestamp>();
                proj.EditorSettings.Timestamps.Add(chart.ChartPath, timestamps);
            }

            Timestamp t = new Timestamp()
            {
                Timing = Services.Gameplay.Audio.AudioTiming,
                Message = I18n.S("Compose.UI.Timeline.Timestamp.DefaultMessage", timestamps.Count),
            };

            timestamps.Add(t);
            timestamps.Sort((a, b) => a.Timing.CompareTo(b.Timing));
            TimestampMarker marker = pool.Get();
            marker.SetContent(t);
        }

        public void DeleteTimestamp()
        {
            if (!TryGetTimestampList(out List<Timestamp> timestamps))
            {
                return;
            }

            foreach (TimestampMarker marker in pool.CurrentlyOccupied)
            {
                if (marker.IsFocused)
                {
                    timestamps.Remove(marker.Timestamp);
                    Rebuild();
                    return;
                }
            }

            int currentTiming = Services.Gameplay.Audio.AudioTiming;
            Timestamp closest = null;
            int closestDifference = int.MaxValue;
            foreach (var t in timestamps)
            {
                int diff = Mathf.Abs(t.Timing - currentTiming);
                if (diff < closestDifference)
                {
                    closest = t;
                    closestDifference = diff;
                }
            }

            if (closest != null)
            {
                timestamps.Remove(closest);
                Rebuild();
            }
        }

        private void Rebuild()
        {
            Clear();
            if (!TryGetTimestampList(out List<Timestamp> timestamps))
            {
                return;
            }

            timestamps.Sort((a, b) => a.Timing.CompareTo(b.Timing));
            foreach (var timestamp in timestamps)
            {
                TimestampMarker t = pool.Get();
                t.SetContent(timestamp);
            }
        }

        private bool TryGetTimestampList(out List<Timestamp> timestamps)
        {
            timestamps = null;
            ProjectSettings proj = Services.Project.CurrentProject;
            ChartSettings chart = Services.Project.CurrentChart;
            if (proj.EditorSettings == null
             || proj.EditorSettings.Timestamps == null
             || !proj.EditorSettings.Timestamps.TryGetValue(chart.ChartPath, out timestamps))
            {
                return false;
            }

            return true;
        }

        private void Clear()
        {
            pool.ReturnAll();
        }

        private void Awake()
        {
            pool = Pools.New<TimestampMarker>(Values.TimestampPoolName, timestampPrefab, timestampParent, timestampCapacity);
            gameplayData.OnChartFileLoad += OnChartChange;

            jumpToNext.onClick.AddListener(JumpToNext);
            jumpToPrevious.onClick.AddListener(JumpToPrevious);
            newTimestamp.onClick.AddListener(AddTimestamp);
            deleteTimestamp.onClick.AddListener(DeleteTimestamp);
            showTimestampButton.onClick.AddListener(ShowTimestampDetails);
            hideTimestampButton.onClick.AddListener(HideTimestampDetails);
            HideTimestampDetails();
        }

        private void OnDestroy()
        {
            gameplayData.OnChartFileLoad -= OnChartChange;

            jumpToNext.onClick.RemoveListener(JumpToNext);
            jumpToPrevious.onClick.RemoveListener(JumpToPrevious);
            newTimestamp.onClick.RemoveListener(AddTimestamp);
            deleteTimestamp.onClick.RemoveListener(DeleteTimestamp);
            showTimestampButton.onClick.RemoveListener(ShowTimestampDetails);
            hideTimestampButton.onClick.RemoveListener(HideTimestampDetails);
        }

        private void ShowTimestampDetails()
        {
            Values.LockTimestampEditing = false;
            timestampDetail.SetActive(true);
            showTimestampButton.gameObject.SetActive(false);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        private void HideTimestampDetails()
        {
            Values.LockTimestampEditing = true;
            timestampDetail.SetActive(false);
            showTimestampButton.gameObject.SetActive(true);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void OnChartChange()
        {
            Rebuild();
        }
    }
}