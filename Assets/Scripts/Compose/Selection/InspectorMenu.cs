using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.History;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Selection
{
    public class InspectorMenu : MonoBehaviour
    {
        private const string Mixed = "-";

        [SerializeField] private RectTransform rect;
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private GameObject timingFields;
        [SerializeField] private GameObject rangeTimingFields;
        [SerializeField] private GameObject laneFields;
        [SerializeField] private GameObject positionFields;
        [SerializeField] private GameObject arcSettingsFields;
        [SerializeField] private TMP_InputField timingField;
        [SerializeField] private TMP_InputField startTimingField;
        [SerializeField] private TMP_InputField endTimingField;
        [SerializeField] private TMP_InputField laneField;
        [SerializeField] private TMP_InputField startXField;
        [SerializeField] private TMP_InputField startYField;
        [SerializeField] private TMP_InputField endXField;
        [SerializeField] private TMP_InputField endYField;
        [SerializeField] private ArcTypeSelector arcTypeField;
        [SerializeField] private ArcColorSelector arcColorField;
        [SerializeField] private Button arcOrTraceButton;
        [SerializeField] private TMP_InputField sfxField;
        [SerializeField] private TimingGroupField groupField;
        [SerializeField] private Button selectArcButton;
        [SerializeField] private Button selectArcTapButton;

        [SerializeField] private TMP_Text arcOrTraceIndicatorText;
        [SerializeField] private Image arcOrTraceIndicatorIcon;
        [SerializeField] private Sprite arcIcon;
        [SerializeField] private Sprite traceIcon;

        private HashSet<Note> selected;

        public bool IsCursorHovering => RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null);

        public void ApplySelection(HashSet<Note> selected)
        {
            this.selected = selected;
            Rebuild();
        }

        private void Rebuild()
        {
            bool includeTap = false;
            bool includeHold = false;
            bool includeArctap = false;
            bool includeArc = false;
            bool includeTrace = false;

            foreach (var note in selected)
            {
                includeTap = includeTap || note is Tap;
                includeHold = includeHold || note is Hold;
                includeArctap = includeArctap || note is ArcTap;
                includeArc = includeArc || (note is Arc a && !a.IsTrace);
                includeTrace = includeTrace || (note is Arc t && t.IsTrace);
            }

            bool includeLong = includeArc || includeTrace || includeHold;
            bool includeShort = includeTap || includeArctap;
            bool includeArclike = includeArc || includeTrace;
            bool showArcSettings = includeArclike && !includeTap && !includeHold && !includeArctap;

            timingFields.SetActive(includeShort);
            rangeTimingFields.SetActive(!includeShort && includeLong);
            laneFields.SetActive((includeTap || includeHold) && !includeArclike && !includeArctap);
            positionFields.SetActive(showArcSettings);
            arcSettingsFields.SetActive(showArcSettings);
            sfxField.gameObject.SetActive(showArcSettings);
            groupField.gameObject.SetActive(selected.Any(n => !(n is ArcTap)));
            selectArcButton.gameObject.SetActive(includeArctap);
            selectArcTapButton.gameObject.SetActive(true);

            // help
            timingField.text = ExtractCommonProperty<Note, int>(n => n.Timing, out int timing) ? timing.ToString() : Mixed;
            startTimingField.text = ExtractCommonProperty<LongNote, int>(n => n.Timing, out int startTiming) ? startTiming.ToString() : Mixed;
            endTimingField.text = ExtractCommonProperty<LongNote, int>(n => n.EndTiming, out int endTiming) ? endTiming.ToString() : Mixed;

            if (includeTap && includeHold)
            {
                laneField.text = ExtractCommonProperty<Tap, int>(n => n.Lane, out int tapLane)
                              && ExtractCommonProperty<Hold, int>(n => n.Lane, out int holdLane)
                              && tapLane == holdLane ? tapLane.ToString() : Mixed;
            }
            else if (includeTap)
            {
                laneField.text = ExtractCommonProperty<Tap, int>(n => n.Lane, out int tapLane) ? tapLane.ToString() : Mixed;
            }
            else
            {
                laneField.text = ExtractCommonProperty<Hold, int>(n => n.Lane, out int holdLane) ? holdLane.ToString() : Mixed;
            }

            startXField.text = ExtractCommonProperty<Arc, float>(n => n.XStart, out float startX) ? startX.ToString() : Mixed;
            startYField.text = ExtractCommonProperty<Arc, float>(n => n.YStart, out float startY) ? startY.ToString() : Mixed;
            endXField.text = ExtractCommonProperty<Arc, float>(n => n.XEnd, out float endX) ? endX.ToString() : Mixed;
            endYField.text = ExtractCommonProperty<Arc, float>(n => n.YEnd, out float endY) ? endY.ToString() : Mixed;
            arcTypeField.SetValueWithoutNotify(ExtractCommonProperty<Arc, int>(n => (int)n.LineType, out int lineTypeNum) ? (ArcLineType)lineTypeNum : ArcLineType.Unknown);
            arcColorField.SetValueWithoutNotify(ExtractCommonProperty<Arc, int>(n => n.Color, out int color) ? color : int.MinValue);
            sfxField.text = ExtractCommonProperty<Arc, string>(n => n.Sfx, out string sfx) ? sfx : Mixed;

            bool tg = ExtractCommonProperty<Note, int>(n => n.TimingGroup, out int group);
            groupField.SetValueWithoutNotify(null);
            if (tg)
            {
                TimingGroup timingGroup = Services.Gameplay.Chart.GetTimingGroup(group);
                if (timingGroup.GroupProperties.Editable)
                {
                    groupField.SetValueWithoutNotify(timingGroup);
                }
            }

            bool areAllTraceOrAllArc = ExtractCommonProperty<Arc, bool>(n => n.IsTrace, out bool areAllTrace);
            if (areAllTraceOrAllArc && areAllTrace)
            {
                arcOrTraceIndicatorText.text = I18n.S("Compose.UI.Inspector.Trace");
                arcOrTraceIndicatorIcon.sprite = traceIcon;
                arcOrTraceIndicatorIcon.color = Color.white;
            }
            else if (areAllTraceOrAllArc && !areAllTrace)
            {
                arcOrTraceIndicatorText.text = I18n.S("Compose.UI.Inspector.Arc");
                arcOrTraceIndicatorIcon.sprite = arcIcon;
                arcOrTraceIndicatorIcon.color = arcColorField.PreviewColor;
            }
            else
            {
                arcOrTraceIndicatorText.text = I18n.S("Compose.UI.Inspector.Mixed");
                arcOrTraceIndicatorIcon.sprite = null;
                arcOrTraceIndicatorIcon.color = Color.clear;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y + 1);
        }

        private bool ExtractCommonProperty<NoteType, T>(Func<NoteType, T> extractor, out T commonValue)
            where NoteType : Note
            where T : IComparable<T>
        {
            commonValue = default;
            bool foundFirstValue = false;
            foreach (var n in selected)
            {
                if (n is NoteType note)
                {
                    T value = extractor.Invoke(note);
                    if (!foundFirstValue)
                    {
                        commonValue = value;
                        foundFirstValue = true;
                    }
                    else
                    {
                        if (commonValue.CompareTo(value) != 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void Awake()
        {
            timingField.onEndEdit.AddListener(OnTimingField);
            startTimingField.onEndEdit.AddListener(OnStartTimingField);
            endTimingField.onEndEdit.AddListener(OnEndTimingField);
            laneField.onEndEdit.AddListener(OnLaneField);
            startXField.onEndEdit.AddListener(OnStartXField);
            startYField.onEndEdit.AddListener(OnStartYField);
            endXField.onEndEdit.AddListener(OnEndXField);
            endYField.onEndEdit.AddListener(OnEndYField);
            arcTypeField.OnTypeChanged += OnArcTypeField;
            arcColorField.OnColorChanged += OnArcColorField;
            arcOrTraceButton.onClick.AddListener(OnArcOrTraceButton);
            sfxField.onEndEdit.AddListener(OnSfxField);
            groupField.OnValueChanged += OnGroupField;
            selectArcButton.onClick.AddListener(OnSelectArcButton);
            selectArcTapButton.onClick.AddListener(OnSelectArcTapButton);
        }

        private void OnDestroy()
        {
            timingField.onEndEdit.RemoveListener(OnTimingField);
            startTimingField.onEndEdit.RemoveListener(OnStartTimingField);
            endTimingField.onEndEdit.RemoveListener(OnEndTimingField);
            laneField.onEndEdit.RemoveListener(OnLaneField);
            startXField.onEndEdit.RemoveListener(OnStartXField);
            startYField.onEndEdit.RemoveListener(OnStartYField);
            endXField.onEndEdit.RemoveListener(OnEndXField);
            endYField.onEndEdit.RemoveListener(OnEndYField);
            arcTypeField.OnTypeChanged -= OnArcTypeField;
            arcColorField.OnColorChanged -= OnArcColorField;
            arcOrTraceButton.onClick.RemoveListener(OnArcOrTraceButton);
            sfxField.onEndEdit.RemoveListener(OnSfxField);
            groupField.OnValueChanged -= OnGroupField;
            selectArcButton.onClick.RemoveListener(OnSelectArcButton);
            selectArcTapButton.onClick.RemoveListener(OnSelectArcTapButton);
        }

        private void OnTimingField(string value)
        {
            if (value == Mixed)
            {
                return;
            }

            if (Evaluator.TryInt(value, out int timing))
            {
                ModifyNotes<Note>(
                    n => n.Timing != timing,
                    n => n.Timing = timing);
            }
        }

        private void OnStartTimingField(string value)
        {
            if (value == Mixed)
            {
                return;
            }

            if (Evaluator.TryInt(value, out int startTiming))
            {
                ModifyNotes<LongNote>(
                    l => l.Timing != startTiming,
                    l => l.Timing = startTiming,
                    l =>
                    {
                        if (l is Arc a)
                        {
                            return l.EndTiming >= startTiming;
                        }
                        else
                        {
                            return l.EndTiming > startTiming;
                        }
                    });
            }
        }

        private void OnEndTimingField(string value)
        {
            if (value == Mixed)
            {
                return;
            }

            if (Evaluator.TryInt(value, out int endTiming))
            {
                ModifyNotes<LongNote>(
                    l => l.EndTiming != endTiming,
                    l => l.EndTiming = endTiming,
                    l =>
                    {
                        if (l is Arc a)
                        {
                            return l.Timing <= endTiming;
                        }
                        else
                        {
                            return l.Timing < endTiming;
                        }
                    });
            }
        }

        private void OnLaneField(string value)
        {
            if (value == Mixed)
            {
                return;
            }

            if (Evaluator.TryInt(value, out int lane))
            {
                ModifyNotes<Note>(
                    ev => (ev is Tap t && t.Lane != lane) || (ev is Hold h && h.Lane != lane),
                    ev =>
                    {
                        if (ev is Tap t)
                        {
                            t.Lane = lane;
                        }
                        else if (ev is Hold h)
                        {
                            h.Lane = lane;
                        }
                    });
            }
        }

        private void OnStartXField(string value)
        {
            if (value == Mixed)
            {
                return;
            }

            if (TryParseXY(value, out float x, out float y))
            {
                ModifyNotes<Arc>(
                    a => a.XStart != x || a.YStart != y,
                    a =>
                    {
                        a.XStart = x;
                        a.YStart = y;
                    });
                return;
            }

            if (Evaluator.TryFloat(value, out x))
            {
                ModifyNotes<Arc>(
                    a => a.XStart != x,
                    a => a.XStart = x);
            }
        }

        private void OnStartYField(string value)
        {
            if (value == Mixed)
            {
                return;
            }

            if (TryParseXY(value, out float x, out float y))
            {
                ModifyNotes<Arc>(
                    a => a.XStart != x || a.YStart != y,
                    a =>
                    {
                        a.XStart = x;
                        a.YStart = y;
                    });
                return;
            }

            if (Evaluator.TryFloat(value, out y))
            {
                ModifyNotes<Arc>(
                    a => a.YStart != y,
                    a => a.YStart = y);
            }
        }

        private void OnEndXField(string value)
        {
            if (value == Mixed)
            {
                return;
            }

            if (TryParseXY(value, out float x, out float y))
            {
                ModifyNotes<Arc>(
                    a => a.XEnd != x || a.YEnd != y,
                    a =>
                    {
                        a.XEnd = x;
                        a.YEnd = y;
                    });
                return;
            }

            if (Evaluator.TryFloat(value, out x))
            {
                ModifyNotes<Arc>(
                    a => a.XEnd != x,
                    a => a.XEnd = x);
            }
        }

        private void OnEndYField(string value)
        {
            if (value == Mixed)
            {
                return;
            }

            if (TryParseXY(value, out float x, out float y))
            {
                ModifyNotes<Arc>(
                    a => a.XEnd != x || a.YEnd != y,
                    a =>
                    {
                        a.XEnd = x;
                        a.YEnd = y;
                    });
                return;
            }

            if (Evaluator.TryFloat(value, out y))
            {
                ModifyNotes<Arc>(
                    a => a.YEnd != y,
                    a => a.YEnd = y);
            }
        }

        private bool TryParseXY(string value, out float x, out float y)
        {
            x = 0;
            y = 0;
            string[] split = value.Split(',');
            if (split.Length != 2)
            {
                return false;
            }

            string xstr = split[0];
            string ystr = split[1];
            return Evaluator.TryFloat(xstr, out x) && Evaluator.TryFloat(ystr, out y);
        }

        private void OnArcTypeField(ArcLineType type)
        {
            ModifyNotes<Arc>(
                a => a.LineType != type,
                a => a.LineType = type);
        }

        private void OnArcColorField(int c)
        {
            ModifyNotes<Arc>(
                a => a.Color != c,
                a => a.Color = c);
        }

        private void OnArcOrTraceButton()
        {
            ModifyNotes<Arc>(
                a => true,
                a => a.IsTrace = !a.IsTrace);
        }

        private void OnSfxField(string value)
        {
            if (value == Mixed)
            {
                return;
            }

            ModifyNotes<Arc>(
                a => a.Sfx != value,
                a => a.Sfx = value);
        }

        private void OnGroupField(TimingGroup tg)
        {
            if (tg.GroupProperties.Editable)
            {
                HashSet<Note> notes = new HashSet<Note>();
                foreach (var note in selected)
                {
                    if (note is Arc arc)
                    {
                        foreach (var at in Services.Gameplay.Chart.GetAll<ArcTap>().Where(at => at.Arc == arc))
                        {
                            notes.Add(at);
                        }

                        notes.Add(note);
                    }
                    else if (note is ArcTap)
                    {
                        continue;
                    }
                    else
                    {
                        notes.Add(note);
                    }
                }

                ModifyNotes<Note>(
                    ev => ev.TimingGroup != tg.GroupNumber,
                    ev =>
                    {
                        ev.TimingGroup = tg.GroupNumber;
                    }, customList: notes);
            }

            groupField.SetValueWithoutNotify(selected.First().TimingGroupInstance);
        }

        private void ModifyNotes<T>(Func<T, bool> include, Action<T> modifier, Func<T, bool> requirement = null, HashSet<Note> customList = null)
            where T : ArcEvent
        {
            List<(ArcEvent instance, ArcEvent newValue)> batch = new List<(ArcEvent instance, ArcEvent newValue)>();
            foreach (Note n in customList ?? selected)
            {
                if (n is T note && include.Invoke(note))
                {
                    if (!(requirement?.Invoke(note) ?? true))
                    {
                        Rebuild();
                        Services.Popups.Notify(Popups.Severity.Warning, I18n.S("Compose.Notify.Inspector.InvalidParameter"));
                        return;
                    }

                    ArcEvent newValue = n.Clone();
                    modifier.Invoke(newValue as T);
                    batch.Add((n, newValue));
                }
            }

            if (batch.Count > 0)
            {
                Services.History.AddCommand(new EventCommand(
                    name: I18n.S("Compose.Notify.History.EditValue"),
                    update: batch));
            }

            Rebuild();
        }

        private void OnSelectArcButton()
        {
            HashSet<Note> arcs = new HashSet<Note>();
            foreach (var n in selected)
            {
                if (n is ArcTap at)
                {
                    arcs.Add(at.Arc);
                }
            }

            Services.Selection.SetSelection(arcs);
        }

        private void OnSelectArcTapButton()
        {
            HashSet<Note> arctaps = new HashSet<Note>();
            foreach (var n in selected)
            {
                if (n is Arc a)
                {
                    var ats = Services.Gameplay.Chart.GetAll<ArcTap>().Where(at => at.Arc == a);
                    foreach (var at in ats)
                    {
                        arctaps.Add(at);
                    }
                }
            }

            Services.Selection.SetSelection(arctaps);
        }
    }
}