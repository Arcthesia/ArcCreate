using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    public class CameraTable : Table<CameraEvent>
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Button addButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private MarkerRange marker;

        public override CameraEvent Selected
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

            marker.SetTiming(Selected.Timing, Selected.Timing + Selected.Duration);
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
            gameplayData.OnChartEdit += OnChartEdit;
            addButton.onClick.AddListener(OnAddButton);
            removeButton.onClick.AddListener(OnRemoveButton);
            marker.OnDragDebounced += OnMarker;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Values.EditingTimingGroup.OnValueChange -= OnEdittingTimingGroup;
            gameplayData.OnChartFileLoad -= OnChart;
            gameplayData.OnChartEdit -= OnChartEdit;
            addButton.onClick.RemoveListener(OnAddButton);
            removeButton.onClick.RemoveListener(OnRemoveButton);
            marker.OnDragDebounced -= OnMarker;
        }

        private void OnChartEdit()
        {
            Rebuild();
            UpdateMarker();
        }

        private void OnAddButton()
        {
            CameraEvent cam;

            if (Selected == null)
            {
                int t = 0;
                if (Data.Count > 0)
                {
                    t = Data[Data.Count - 1].Timing + 1;
                }

                cam = new CameraEvent()
                {
                    Timing = t,
                    Move = Vector3.zero,
                    Rotate = Vector3.zero,
                    CameraType = Gameplay.Data.CameraType.L,
                    Duration = 1,
                    TimingGroup = Values.EditingTimingGroup.Value,
                };
            }
            else
            {
                int t = Selected.Timing + 1;

                while (true)
                {
                    foreach (CameraEvent ev in Data)
                    {
                        if (ev.Timing == t)
                        {
                            t += 1;
                            continue;
                        }
                    }

                    cam = new CameraEvent()
                    {
                        Timing = t,
                        Move = Vector3.zero,
                        Rotate = Vector3.zero,
                        CameraType = Gameplay.Data.CameraType.L,
                        Duration = 1,
                        TimingGroup = Values.EditingTimingGroup.Value,
                    };
                    break;
                }
            }

            Services.History.AddCommand(new EventCommand(
                add: new List<ArcEvent>() { cam }));
            Selected = cam;
            Rebuild();
            JumpTo(IndexOf(cam));
        }

        private void OnRemoveButton()
        {
            if (Selected == null)
            {
                return;
            }

            int index = IndexOf(Selected);
            Services.History.AddCommand(new EventCommand(
                remove: new List<ArcEvent>() { Selected }));
            Selected = Data[Mathf.Max(index - 1, 0)];
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
            List<CameraEvent> cam = Services.Gameplay.Chart.GetAll<CameraEvent>().Where(c => c.TimingGroup == group).ToList();
            SetData(cam);
        }

        private void OnMarker(int timing, int endTiming)
        {
            if (Selected == null)
            {
                return;
            }

            CameraEvent newValue = Selected.Clone() as CameraEvent;
            newValue.Timing = timing;
            newValue.Duration = endTiming - timing;
            Services.History.AddCommand(new EventCommand(
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