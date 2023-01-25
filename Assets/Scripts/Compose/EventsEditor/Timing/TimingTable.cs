using System.Collections.Generic;
using ArcCreate.Compose.Components;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    public class TimingTable : Table<TimingEvent>
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Button addButton;
        [SerializeField] private Button removeButton;

        public void Rebuild()
        {
            ReloadGroup(Values.EditingTimingGroup.Value);
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
            addButton.onClick.AddListener(OnAddButton);
            removeButton.onClick.AddListener(OnRemoveButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Values.EditingTimingGroup.OnValueChange -= OnEdittingTimingGroup;
            gameplayData.OnChartFileLoad -= OnChart;
            addButton.onClick.RemoveListener(OnAddButton);
            removeButton.onClick.RemoveListener(OnRemoveButton);
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

            Services.Gameplay.Chart.AddEvents(new List<ArcEvent> { timing });
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
            Services.Gameplay.Chart.RemoveEvents(new List<ArcEvent> { Selected });
            Selected = Data[Mathf.Max(index - 1, 0)];
            Rebuild();
            JumpTo(index - 1);
        }

        private void OnEdittingTimingGroup(int group)
        {
            ReloadGroup(group);
        }

        private void OnChart()
        {
            ReloadGroup(0);
        }

        private void ReloadGroup(int group)
        {
            TimingGroup tg = Services.Gameplay.Chart.GetTimingGroup(group);
            SetData(tg.Timings);
        }
    }
}