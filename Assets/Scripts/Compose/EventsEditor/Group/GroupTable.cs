using System.Collections.Generic;
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
        [SerializeField] private TimingGroupField editingTimingGroupField;

        public override TimingGroup Selected
        {
            get => base.Selected;
            set
            {
                base.Selected = value;
                removeButton.interactable = Selected?.GroupProperties.Editable ?? false
                                         && Selected?.GroupNumber > 0;
            }
        }

        public void UpdateEditingGroupField()
        {
            editingTimingGroupField.SetValueWithoutNotify(Selected);
        }

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
            editingTimingGroupField.OnValueChanged += OnEditingTimingGroupField;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            gameplayData.OnChartFileLoad -= OnChart;
            Values.EditingTimingGroup.OnValueChange -= OnEdittingTimingGroup;
            addButton.onClick.RemoveListener(OnAddButton);
            removeButton.onClick.RemoveListener(OnRemoveButton);
            editingTimingGroupField.OnValueChanged -= OnEditingTimingGroupField;
        }

        private void OnEditingTimingGroupField(TimingGroup obj)
        {
            Values.EditingTimingGroup.Value = obj.GroupNumber;
        }

        private void OnEdittingTimingGroup(int group)
        {
            Selected = Services.Gameplay.Chart.GetTimingGroup(group);
            UpdateTable(Services.Gameplay.Chart.TimingGroups);
            editingTimingGroupField.SetValueWithoutNotify(Selected);
        }

        private void OnAddButton()
        {
            int newTgNum = Services.Gameplay.Chart.TimingGroups.Count;
            Services.Gameplay.Chart.GetTimingGroup(newTgNum);

            // Trigger OnEdittingTimingGroup
            Values.EditingTimingGroup.Value = newTgNum;
            JumpTo(Data.Count);

            Values.ProjectModified = true;

            Debug.Log(I18n.S(
                "Compose.Notify.GroupTable.AddGroup", new Dictionary<string, object>
                {
                    { "Number", newTgNum },
                }));
        }

        private void OnRemoveButton()
        {
            if (Selected == null)
            {
                return;
            }

            Services.Popups.CreateTextDialog(
                I18n.S("Compose.Dialog.RemoveGroup.Title"),
                I18n.S("Compose.Dialog.RemoveGroup.Content"),
                new Popups.ButtonSetting
                {
                    Text = I18n.S("Compose.Dialog.RemoveGroup.Yes"),
                    Callback = OnRemoveConfirm,
                    ButtonColor = Popups.ButtonColor.Danger,
                },
                new Popups.ButtonSetting
                {
                    Text = I18n.S("Compose.Dialog.RemoveGroup.No"),
                    Callback = null,
                    ButtonColor = Popups.ButtonColor.Default,
                });
        }

        private void OnRemoveConfirm()
        {
            int index = IndexOf(Selected);
            int num = Selected.GroupNumber;
            Services.Gameplay.Chart.RemoveTimingGroup(Selected);

            // Trigger OnEdittingTimingGroup
            Values.EditingTimingGroup.Value = Mathf.Min(Selected.GroupNumber, Services.Gameplay.Chart.TimingGroups.Count - 1);
            JumpTo(index - 1);

            Values.ProjectModified = true;

            Debug.Log(I18n.S(
                "Compose.Notify.GroupTable.RemoveGroup", new Dictionary<string, object>
                {
                    { "Number", num },
                }));
        }

        private void OnChart()
        {
            UpdateTable(Services.Gameplay.Chart.TimingGroups);
            Selected = Services.Gameplay.Chart.TimingGroups.FirstOrDefault();
            Values.EditingTimingGroup.Value = 0;
        }

        private void UpdateTable(List<TimingGroup> groups)
        {
            SetData(groups);
            removeButton.interactable = groups.Count > 1 && Selected?.GroupNumber > 0;
        }
    }
}