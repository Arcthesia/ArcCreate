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
        [SerializeField] private Marker marker;
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

        public void OnHorizontalScroll()
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

            float totalWidth = Mathf.Max(300, argCount * 75);
            float viewWidth = 300;

            float x = Mathf.Lerp(0, -totalWidth + viewWidth, horizontalScrollbar.value);
            foreach (var row in Rows)
            {
                (row as ScenecontrolRow).SetFieldOffsetX(100 + x);
            }

            parametersRow.SetFieldOffsetX(x);
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

            float totalWidth = Mathf.Max(300, argCount * 75);
            float viewWidth = 300;
            horizontalScrollbar.size = viewWidth / totalWidth;
            OnHorizontalScroll();
        }

        public void OnTypenameChange(Dropdown d)
        {
            currentTypename = argNames.Keys.ElementAt(d.value);
            horizontalScrollbar.value = 0;
            Rebuild();
        }

        public void Rebuild()
        {
            List<ScenecontrolEvent> scs;
            if (typenameDropdown.options.Count == 0)
            {
                currentTypename = null;
                scs = new List<ScenecontrolEvent>();
                UpdateHorizontalScrollbar();
                parametersRow.SetupFields(new string[0]);
            }
            else
            {
                if (currentTypename == null || !argNames.Keys.Contains(currentTypename))
                {
                    currentTypename = argNames.Keys.First();
                    typenameDropdown.value = 0;
                }

                UpdateHorizontalScrollbar();
                parametersRow.SetupFields(argNames[currentTypename]);

                scs = Services.Gameplay.Chart.GetAll<ScenecontrolEvent>()
                    .Where(sc => sc.TimingGroup == Values.EditingTimingGroup.Value
                        && sc.Typename == currentTypename)
                    .ToList();
            }

            SetData(scs);
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

        private void OnAutoRebuildToggle(bool isOn)
        {
            Settings.ScenecontrolAutoRebuild.Value = isOn;
        }

        private void OnAddButton()
        {
            ScenecontrolEvent sc;

            if (Selected == null)
            {
                ScenecontrolEvent lastEvent = Data[Data.Count - 1];
                int argCount = argNames[currentTypename].Length;
                sc = new ScenecontrolEvent()
                {
                    Timing = lastEvent.Timing + 1,
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
            Selected = Data[Mathf.Max(index - 1, 0)];
            Rebuild();
            JumpTo(index - 1);
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