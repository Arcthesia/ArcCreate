using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Navigation;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    [EditorScope("Groups")]
    public class GroupTable : Table<TimingGroup>
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Button addButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private Button selectButton;
        [SerializeField] private TimingGroupField editingTimingGroupField;

        public override TimingGroup Selected
        {
            get => base.Selected;
            set
            {
                base.Selected = value;
                removeButton.interactable = Selected?.GroupProperties.Editable ?? false
                                         && Selected?.GroupNumber > 0;
                selectButton.interactable = Selected?.GroupProperties.Editable ?? false;
            }
        }

        [EditorAction("ChangeGroup", false, "<c-g>")]
        [KeybindHint(Exclude = true)]
        public void OpenPicker()
        {
            editingTimingGroupField.Open(I18n.S("Compose.Dialog.GroupPicker.SelectEditingGroup"));
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
            gameplayData.OnChartEdit += OnChartEdit;
            gameplayData.OnChartFileLoad += OnChart;
            Values.EditingTimingGroup.OnValueChange += OnEdittingTimingGroup;
            addButton.onClick.AddListener(OnAddButton);
            removeButton.onClick.AddListener(OnRemoveButton);
            selectButton.onClick.AddListener(OnSelectButton);
            editingTimingGroupField.OnValueChanged += OnEditingTimingGroupField;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            gameplayData.OnChartEdit -= OnChartEdit;
            gameplayData.OnChartFileLoad -= OnChart;
            Values.EditingTimingGroup.OnValueChange -= OnEdittingTimingGroup;
            addButton.onClick.RemoveListener(OnAddButton);
            removeButton.onClick.RemoveListener(OnRemoveButton);
            selectButton.onClick.RemoveListener(OnSelectButton);
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
            TimingGroup group = new TimingGroup(newTgNum);
            group.Load();

            ICommand cmd = new AddTimingGroupCommand(I18n.S(
                "Compose.Notify.GroupTable.AddGroup", new Dictionary<string, object>
                {
                    { "Number", newTgNum },
                }), group);

            Services.History.AddCommand(cmd);

            // Trigger OnEdittingTimingGroup
            Values.EditingTimingGroup.Value = newTgNum;
            JumpTo(Data.Count);

            Values.ProjectModified = true;
            Values.OnEditAction?.Invoke();
        }

        private void OnRemoveButton()
        {
            if (Selected == null)
            {
                return;
            }

            int index = IndexOf(Selected);
            int num = Selected.GroupNumber;

            List<ArcEvent> scCamEvents = new List<ArcEvent>();
            scCamEvents.AddRange(Services.Gameplay.Chart.GetAll<CameraEvent>().Where(c => c.TimingGroup == num));
            scCamEvents.AddRange(Services.Gameplay.Chart.GetAll<ScenecontrolEvent>().Where(s => s.TimingGroup == num));
            ICommand tgCmd = new RemoveTimingGroupCommand("", Selected);
            ICommand evCmd = new EventCommand("", remove: scCamEvents);
            ICommand combination = new CombinedCommand(I18n.S(
                "Compose.Notify.GroupTable.RemoveGroup", new Dictionary<string, object>
                {
                    { "Number", num },
                }), tgCmd, evCmd);
            Services.History.AddCommand(combination);

            // Trigger OnEdittingTimingGroup
            Values.EditingTimingGroup.Value = Mathf.Min(Selected.GroupNumber, Services.Gameplay.Chart.TimingGroups.Count - 1);
            JumpTo(index - 1);

            Values.ProjectModified = true;
            Values.OnEditAction?.Invoke();
        }

        private void OnSelectButton()
        {
            if (Selected == null)
            {
                return;
            }

            Services.Selection.SetSelection(
                Selected.GetEventType<Tap>().Cast<Note>()
                    .Concat(Selected.GetEventType<Hold>().Cast<Note>())
                    .Concat(Selected.GetEventType<Arc>().Cast<Note>())
                    .Concat(Selected.GetEventType<ArcTap>().Cast<Note>()));
        }

        private void OnChart()
        {
            UpdateTable(Services.Gameplay.Chart.TimingGroups);
            Selected = Services.Gameplay.Chart.TimingGroups.FirstOrDefault();
            Values.EditingTimingGroup.Value = 0;
        }

        private void OnChartEdit()
        {
            RebuildRows();
        }

        private void UpdateTable(List<TimingGroup> groups)
        {
            SetData(groups);
            removeButton.interactable = groups.Count > 1 && Selected?.GroupNumber > 0;
        }
    }
}