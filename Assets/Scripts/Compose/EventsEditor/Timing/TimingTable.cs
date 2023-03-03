using System.Collections.Generic;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    public class TimingTable : Table<TimingEvent>
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Button addButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private Marker marker;

        public override TimingEvent Selected
        {
            get => base.Selected;
            set
            {
                base.Selected = value;
                marker.gameObject.SetActive(value != null);
                UpdateMarker();
            }
        }

        public void Rebuild()
        {
            ReloadGroup(Values.EditingTimingGroup.Value);
        }

        public void UpdateMarker()
        {
            if (Selected == null)
            {
                return;
            }

            marker.SetTiming(Selected.Timing);
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void Awake()
        {
            base.Awake();
            Values.EditingTimingGroup.OnValueChange += OnEdittingTimingGroup;
            gameplayData.OnChartFileLoad += OnChart;
            gameplayData.OnChartTimingEdit += OnChartEdit;
            addButton.onClick.AddListener(OnAddButton);
            removeButton.onClick.AddListener(OnRemoveButton);
            marker.OnEndEdit += OnMarker;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Values.EditingTimingGroup.OnValueChange -= OnEdittingTimingGroup;
            gameplayData.OnChartFileLoad -= OnChart;
            gameplayData.OnChartTimingEdit -= OnChartEdit;
            addButton.onClick.RemoveListener(OnAddButton);
            removeButton.onClick.RemoveListener(OnRemoveButton);
            marker.OnEndEdit -= OnMarker;
        }

        private void OnChartEdit()
        {
            Rebuild();
            UpdateMarker();
        }

        private void OnAddButton()
        {
            TimingEvent timing;

            if (Selected == null)
            {
                TimingEvent lastEvent = Data[Data.Count - 1];
                timing = new TimingEvent()
                {
                    Timing = lastEvent.Timing + 1,
                    Bpm = lastEvent.Bpm,
                    Divisor = lastEvent.Divisor,
                    TimingGroup = Values.EditingTimingGroup.Value,
                };
            }
            else
            {
                int t = Selected.Timing + 1;

                while (true)
                {
                    foreach (TimingEvent ev in Data)
                    {
                        if (ev.Timing == t)
                        {
                            t += 1;
                            continue;
                        }
                    }

                    timing = new TimingEvent()
                    {
                        Timing = t,
                        Bpm = Selected.Bpm,
                        Divisor = Selected.Divisor,
                        TimingGroup = Values.EditingTimingGroup.Value,
                    };
                    break;
                }
            }

            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.History.AddTiming"),
                add: new List<ArcEvent>() { timing }));
            Selected = timing;
            Rebuild();
            JumpTo(IndexOf(timing));
        }

        private void OnRemoveButton()
        {
            if (Selected == null)
            {
                return;
            }

            int index = IndexOf(Selected);
            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.RemoveTiming"),
                remove: new List<ArcEvent>() { Selected }));
            Selected = Data.Count == 0 ? null : Data[Mathf.Max(index - 1, 0)];
            Rebuild();
            JumpTo(index - 1);
        }

        private void OnEdittingTimingGroup(int group)
        {
            Selected = null;
            ReloadGroup(group);
        }

        private void OnChart()
        {
            Selected = null;
            ReloadGroup(0);
        }

        private void ReloadGroup(int group)
        {
            TimingGroup tg = Services.Gameplay.Chart.GetTimingGroup(group);
            SetData(tg.Timings);
        }

        private void OnMarker(Marker marker, int timing)
        {
            if (Selected == null)
            {
                return;
            }

            TimingEvent newValue = Selected.Clone() as TimingEvent;
            newValue.Timing = timing;
            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.History.EditTiming"),
                update: new List<(ArcEvent instance, ArcEvent newValue)> { (Selected, newValue) }));
            Rebuild();
            JumpTo(IndexOf(Selected));
        }

        private void OnEnable()
        {
            if (marker != null)
            {
                marker.gameObject.SetActive(Selected != null);
            }
        }

        private void OnDisable()
        {
            if (marker != null)
            {
                marker.gameObject.SetActive(false);
            }
        }
    }
}