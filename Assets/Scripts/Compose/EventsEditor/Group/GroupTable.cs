using System.Linq;
using ArcCreate.Compose.Components;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Chart;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    public class GroupTable : Table<TimingGroup>
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Button addButton;
        [SerializeField] private Button removeButton;

        protected override void Update()
        {
            base.Update();
        }

        protected override void Awake()
        {
            base.Awake();
            gameplayData.OnChartFileLoad += OnChart;
            Values.EditingTimingGroup.OnValueChange += OnEdittingTimingGroup;
            addButton.onClick.AddListener(OnAddButton);
            removeButton.onClick.AddListener(OnRemoveButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            gameplayData.OnChartFileLoad -= OnChart;
            Values.EditingTimingGroup.OnValueChange -= OnEdittingTimingGroup;
            addButton.onClick.RemoveListener(OnAddButton);
            removeButton.onClick.RemoveListener(OnRemoveButton);
        }

        private void OnEdittingTimingGroup(int group)
        {
            Selected = Services.Gameplay.Chart.GetTimingGroup(group);
            SetData(Services.Gameplay.Chart.TimingGroups);
        }

        private void OnAddButton()
        {
            int newTgNum = Services.Gameplay.Chart.TimingGroups.Count;
            Services.Gameplay.Chart.GetTimingGroup(newTgNum);

            // Trigger OnEdittingTimingGroup
            Values.EditingTimingGroup.Value = newTgNum;
            JumpTo(Data.Count);
        }

        private void OnRemoveButton()
        {
            if (Selected == null)
            {
                return;
            }

            // TODO: Confirmation dialog + undo / redo
            int index = IndexOf(Selected);
            Services.Gameplay.Chart.RemoveTimingGroup(Selected);

            // Trigger OnEdittingTimingGroup
            Values.EditingTimingGroup.Value = Mathf.Min(Selected.GroupNumber, Services.Gameplay.Chart.TimingGroups.Count - 1);
            JumpTo(index - 1);
        }

        private void OnChart()
        {
            SetData(Services.Gameplay.Chart.TimingGroups);
            Selected = Services.Gameplay.Chart.TimingGroups.FirstOrDefault();
            Values.EditingTimingGroup.Value = 0;
        }
    }
}