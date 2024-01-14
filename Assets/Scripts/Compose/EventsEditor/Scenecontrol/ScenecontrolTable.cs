using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    public class ScenecontrolTable : Table<ScenecontrolEvent>
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private TMP_Dropdown typenameDropdown;
        [SerializeField] private Toggle autoRebuild;
        [SerializeField] private Button rebuildButton;
        [SerializeField] private ParametersRow parametersRow;
        [SerializeField] private Scrollbar horizontalScrollbar;
        [SerializeField] private Button addButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private Button generateEmmyButton;
        [SerializeField] private Marker marker;
        [SerializeField] private int maxNumVisibleFields = 4;
        private ScenecontrolLuaEnvironment luaEnvironment;
        private string currentTypename;
        private readonly SortedDictionary<string, string[]> argNames = new SortedDictionary<string, string[]>();

        public int ArgCount
        {
            get
            {
                if (string.IsNullOrEmpty(currentTypename))
                {
                    return 0;
                }

                return argNames[currentTypename].Length;
            }
        }

        public int MaxNumVisibleFields => maxNumVisibleFields;

        public override ScenecontrolEvent Selected
        {
            get => base.Selected;
            set
            {
                base.Selected = value;
                marker.gameObject.SetActive(value != null);
                UpdateMarker();
            }
        }

        public void UpdateMarker()
        {
            if (Selected == null)
            {
                return;
            }

            marker.SetTiming(Selected.Timing);
        }

        public void SetArgument(string name, string[] args)
        {
            if (argNames.ContainsKey(name))
            {
                argNames[name] = args;
            }
            else
            {
                argNames.Add(name, args);
            }

            typenameDropdown.options = argNames.Keys.Select(typename => new TMP_Dropdown.OptionData(typename)).ToList();
        }

        public void ClearTypes()
        {
            argNames.Clear();
            typenameDropdown.options.Clear();
            typenameDropdown.value = 0;
        }

        public void OnHorizontalScroll(float value)
        {
            int argCount;
            if (currentTypename == null)
            {
                argCount = 0;
            }
            else
            {
                argCount = argNames[currentTypename].Length;
            }

            foreach (var row in Rows)
            {
                (row as ScenecontrolRow).SetFieldOffsetX(value);
            }

            parametersRow.SetFieldOffsetX(value);
        }

        public void UpdateHorizontalScrollbar()
        {
            int argCount;
            if (currentTypename == null)
            {
                argCount = 0;
            }
            else
            {
                argCount = argNames[currentTypename].Length;
            }

            float size = Mathf.Min((float)maxNumVisibleFields / argCount, 1);
            horizontalScrollbar.size = size;
            horizontalScrollbar.gameObject.SetActive(size < 1);
            OnHorizontalScroll(horizontalScrollbar.value);
        }

        public void Rebuild()
        {
            List<ScenecontrolEvent> scs;
            if (typenameDropdown.options.Count == 0)
            {
                currentTypename = null;
                scs = new List<ScenecontrolEvent>();
                UpdateHorizontalScrollbar();
                parametersRow.SetupFields(new string[0], maxNumVisibleFields);
            }
            else
            {
                if (argNames.Count == 0)
                {
                    return;
                }

                if (currentTypename == null || !argNames.Keys.Contains(currentTypename))
                {
                    currentTypename = argNames.Keys.First();
                    typenameDropdown.value = 0;
                }

                UpdateHorizontalScrollbar();
                parametersRow.SetupFields(argNames[currentTypename], maxNumVisibleFields);

                scs = Services.Gameplay.Chart.GetAll<ScenecontrolEvent>()
                    .Where(sc => sc.TimingGroup == Values.EditingTimingGroup.Value
                        && sc.Typename == currentTypename)
                    .ToList();
            }

            SetData(scs);
            foreach (var row in Rows)
            {
                (row as ScenecontrolRow).SetReference(row.Reference);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            luaEnvironment = new ScenecontrolLuaEnvironment(this);
            gameplayData.OnChartScenecontrolEdit += OnChartEdit;
            gameplayData.OnChartFileLoad += OnChart;
            autoRebuild.isOn = Settings.ScenecontrolAutoRebuild.Value;
            autoRebuild.onValueChanged.AddListener(OnAutoRebuildToggle);
            rebuildButton.onClick.AddListener(RebuildLua);
            addButton.onClick.AddListener(OnAddButton);
            removeButton.onClick.AddListener(OnRemoveButton);
            typenameDropdown.onValueChanged.AddListener(OnTypenameChange);
            horizontalScrollbar.onValueChanged.AddListener(OnHorizontalScroll);
            generateEmmyButton.onClick.AddListener(GenerateEmmy);
            Values.EditingTimingGroup.OnValueChange += OnEdittingTimingGroup;
        }

        protected override void OnDestroy()
        {
            base.Awake();
            gameplayData.OnChartScenecontrolEdit -= OnChartEdit;
            gameplayData.OnChartFileLoad -= OnChart;
            autoRebuild.onValueChanged.RemoveListener(OnAutoRebuildToggle);
            rebuildButton.onClick.RemoveListener(RebuildLua);
            addButton.onClick.RemoveListener(OnAddButton);
            removeButton.onClick.RemoveListener(OnRemoveButton);
            typenameDropdown.onValueChanged.RemoveListener(OnTypenameChange);
            horizontalScrollbar.onValueChanged.RemoveListener(OnHorizontalScroll);
            generateEmmyButton.onClick.AddListener(GenerateEmmy);
            Values.EditingTimingGroup.OnValueChange -= OnEdittingTimingGroup;
        }

        private void RebuildLua()
        {
            luaEnvironment.Rebuild();
        }

        private void OnChartEdit()
        {
            Selected = null;
            if (Settings.ScenecontrolAutoRebuild.Value)
            {
                luaEnvironment.Rebuild();
            }

            Rebuild();
        }

        private void OnEdittingTimingGroup(int group)
        {
            Selected = null;
            Rebuild();
        }

        private void OnChart()
        {
            Selected = null;
            luaEnvironment.Rebuild();
            Rebuild();
        }

        private void OnTypenameChange(int value)
        {
            Selected = null;
            currentTypename = argNames.Keys.ElementAt(value);
            horizontalScrollbar.value = 0;
            Rebuild();
        }

        private void OnAutoRebuildToggle(bool isOn)
        {
            Settings.ScenecontrolAutoRebuild.Value = isOn;
        }

        private void OnAddButton()
        {
            ScenecontrolEvent sc;

            if (Selected == null)
            {
                int t = 0;
                if (Data.Count > 0)
                {
                    t = Data[Data.Count - 1].Timing + 1;
                }

                int argCount = argNames[currentTypename].Length;
                sc = new ScenecontrolEvent()
                {
                    Timing = t,
                    Typename = currentTypename,
                    Arguments = Enumerable.Repeat((object)0f, argCount).ToList(),
                    TimingGroup = Values.EditingTimingGroup.Value,
                };
            }
            else
            {
                int t = Selected.Timing + 1;

                while (true)
                {
                    foreach (ScenecontrolEvent ev in Data)
                    {
                        if (ev.Timing == t)
                        {
                            t += 1;
                            continue;
                        }
                    }

                    sc = Selected.Clone() as ScenecontrolEvent;
                    sc.Timing = t;
                    break;
                }
            }

            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.History.AddScenecontrol"),
                add: new List<ArcEvent>() { sc }));
            Selected = sc;
            Rebuild();
            JumpTo(IndexOf(sc));
        }

        private void OnRemoveButton()
        {
            if (Selected == null)
            {
                return;
            }

            int index = IndexOf(Selected);
            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.RemoveScenecontrol"),
                remove: new List<ArcEvent>() { Selected }));
            Selected = Data.Count == 0 ? null : Data[Mathf.Max(index - 1, 0)];
            Rebuild();
            JumpTo(index - 1);
        }

        private void OnEnable()
        {
            Rebuild();
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

        private void GenerateEmmy()
        {
            luaEnvironment.GenerateEmmyLua();
            Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.EmmyLuaGenerated.Scenecontrol"));
        }
    }
}