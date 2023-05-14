using System;
using UnityEngine;

namespace ArcCreate.Compose.Timeline
{
    public class MarkerRange : MonoBehaviour
    {
        [SerializeField] private Marker marker1;
        [SerializeField] private Marker marker2;
        [SerializeField] private RectTransform area;
        private RectTransform marker1Rect;
        private RectTransform marker2Rect;

        public event Action<int, int> OnValueChanged;

        public event Action<int, int> OnEndEdit;

        public int Timing { get; private set; }

        public int EndTiming { get; private set; }

        public void SetTiming(int timing, int endTiming)
        {
            Timing = timing;
            EndTiming = endTiming;
            marker1.SetTiming(timing);
            marker2.SetTiming(endTiming);
            if (gameObject.activeInHierarchy)
            {
                Update();
            }
        }

        private void Awake()
        {
            marker1.OnValueChanged += OnMarker;
            marker2.OnValueChanged += OnMarker;
            marker1.OnEndEdit += OnMarkerDebounced;
            marker2.OnEndEdit += OnMarkerDebounced;

            marker1Rect = marker1.GetComponent<RectTransform>();
            marker2Rect = marker2.GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            marker1.OnValueChanged -= OnMarker;
            marker2.OnValueChanged -= OnMarker;
            marker1.OnEndEdit -= OnMarkerDebounced;
            marker2.OnEndEdit -= OnMarkerDebounced;
        }

        private void OnMarker(Marker arg1, int arg2)
        {
            UpdateTiming();

            OnValueChanged?.Invoke(Timing, EndTiming);
        }

        private void OnMarkerDebounced(Marker arg1, int arg2)
        {
            UpdateTiming();

            OnEndEdit?.Invoke(Timing, EndTiming);
        }

        private void UpdateTiming()
        {
            int timing1 = marker1.UseChartTiming ? marker1.ChartTiming : marker1.AudioTiming;
            int timing2 = marker2.UseChartTiming ? marker2.ChartTiming : marker2.AudioTiming;

            Timing = Mathf.Min(timing1, timing2);
            EndTiming = Mathf.Max(timing1, timing2);
        }

        private void Update()
        {
            float minX = Mathf.Min(marker1Rect.anchoredPosition.x, marker2Rect.anchoredPosition.x);
            float maxX = Mathf.Max(marker1Rect.anchoredPosition.x, marker2Rect.anchoredPosition.x);

            area.anchoredPosition = new Vector2(
                minX,
                area.anchoredPosition.y);

            area.sizeDelta = new Vector2(
                maxX - minX,
                area.sizeDelta.y);
        }
    }
}