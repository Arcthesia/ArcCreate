using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Grid;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Selection;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcCreate.Compose.Editing
{
    [EditorScope("Dragging")]
    public class Dragging : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Marker shortMarker;
        [SerializeField] private MarkerRange longMarker;
        [SerializeField] private float dragPosCombineThreshold;
        private Note targetShort;
        private LongNote targetLong;

        [EditorAction("Timing", false, "<a-mouse1>")]
        [SubAction("Confirm", false, "<a-u-mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [SelectionService.RequireSelection]
        [WhitelistScopes(typeof(GridService), typeof(TimelineService))]
        public async UniTask DragTiming(EditorAction action)
        {
            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Cancel");
            HashSet<Note> selection = Services.Selection.SelectedNotes;

            Services.Cursor.EnableLaneCursor = true;
            Services.Cursor.ForceUpdateLaneCursor();
            if (!Services.Cursor.IsHittingLane)
            {
                return;
            }

            int cursorTiming = Services.Cursor.CursorTiming;
            int closestTiming = 0;
            int closestDist = int.MaxValue;

            foreach (var note in selection)
            {
                if (Mathf.Abs(note.Timing - cursorTiming) < closestDist)
                {
                    closestTiming = note.Timing;
                    closestDist = Mathf.Abs(note.Timing - cursorTiming);
                }

                if (note is LongNote l && Mathf.Abs(l.EndTiming - cursorTiming) < closestDist)
                {
                    closestTiming = l.EndTiming;
                    closestDist = Mathf.Abs(l.EndTiming - cursorTiming);
                }
            }

            List<(ArcEvent instance, ArcEvent newValue)> events = new List<(ArcEvent, ArcEvent)>();
            List<Note> dragStart = new List<Note>();
            List<LongNote> dragEnd = new List<LongNote>();
            int limitLower = int.MinValue;
            int limitUpper = int.MaxValue;

            if (limitUpper < limitLower)
            {
                Services.Popups.Notify(Popups.Severity.Warning, I18n.S("Compose.Notify.NoDragRange"));
                return;
            }

            foreach (var note in selection)
            {
                if (Mathf.Abs(note.Timing - closestTiming) <= 1)
                {
                    bool alsoDraggingEnd = note is LongNote longNote
                     && Mathf.Abs(longNote.EndTiming - closestTiming) <= 1;
                    if (!alsoDraggingEnd || cursorTiming <= closestTiming)
                    {
                        Note clone = note.Clone() as Note;
                        events.Add((note, clone));
                        dragStart.Add(clone);

                        if (note is ArcTap arctap)
                        {
                            limitLower = Mathf.Max(limitLower, arctap.Arc.Timing);
                            limitUpper = Mathf.Min(limitUpper, arctap.Arc.EndTiming);
                        }

                        if (note is Hold hold)
                        {
                            limitUpper = Mathf.Min(limitUpper, hold.EndTiming - 1);
                        }

                        if (note is Arc arc)
                        {
                            limitUpper = Mathf.Min(limitUpper, arc.EndTiming);
                            foreach (var at in Services.Gameplay.Chart.GetAll<ArcTap>().Where(at => at.Arc == arc))
                            {
                                limitUpper = Mathf.Min(limitUpper, at.Timing);
                            }
                        }
                    }
                }

                if (note is LongNote l && Mathf.Abs(l.EndTiming - closestTiming) <= 1)
                {
                    bool alsoDraggingStart = Mathf.Abs(l.Timing - closestTiming) <= 1;
                    if (!alsoDraggingStart || cursorTiming > closestTiming)
                    {
                        LongNote clone = l.Clone() as LongNote;
                        events.Add((l, clone));
                        dragEnd.Add(clone);

                        limitLower = Mathf.Max(limitLower, l.Timing + (note is Hold ? 1 : 0));
                    }

                    if (note is Arc arc)
                    {
                        foreach (var at in Services.Gameplay.Chart.GetAll<ArcTap>().Where(at => at.Arc == arc))
                        {
                            limitLower = Mathf.Max(limitLower, at.Timing);
                        }
                    }
                }
            }

            var command = new EventCommand(
                name: I18n.S("Compose.Notify.History.Drag.Timing"),
                update: events);
            Services.Gameplay.Chart.EnableColliderGeneration = false;
            var (success, timing) = await Services.Cursor.RequestTimingSelection(
                confirm,
                cancel,
                update: t =>
                {
                    t = Mathf.Clamp(t, limitLower, limitUpper);
                    foreach (var note in dragStart)
                    {
                        note.Timing = t;
                    }

                    foreach (var note in dragEnd)
                    {
                        note.EndTiming = t;
                    }

                    command.Execute();
                });

            Services.Gameplay.Chart.EnableColliderGeneration = true;
            if (!success)
            {
                command.Undo();
            }
            else
            {
                Services.History.AddCommand(command);
            }
        }

        [EditorAction("Position", false, "<s-mouse1>")]
        [SubAction("Confirm", false, "<s-u-mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [SelectionService.RequireSelection]
        [WhitelistScopes(typeof(GridService), typeof(TimelineService))]
        public async UniTask DragPosition(EditorAction action)
        {
            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Cancel");
            HashSet<Note> selection = Services.Selection.SelectedNotes;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            int closestTiming = 0;
            float closestDist = float.MaxValue;
            foreach (var note in selection)
            {
                if (TryGetDistance(note, mousePos, out float dist))
                {
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestTiming = note.Timing;
                    }
                }

                if (note is Arc a && TryGetDistanceToArcEnd(a, mousePos, out float endDist))
                {
                    if (endDist < closestDist)
                    {
                        closestDist = endDist;
                        closestTiming = a.EndTiming;
                    }
                }
            }

            List<Note> dragTap = new List<Note>();
            List<Note> dragHold = new List<Note>();
            List<Arc> dragArcStart = new List<Arc>();
            List<Arc> dragArcEnd = new List<Arc>();
            List<ArcTap> dragArcTap = new List<ArcTap>();

            foreach (var note in selection)
            {
                bool draggingArcStart = false;
                if (TryGetDistance(note, mousePos, out float dist)
                 && Mathf.Abs(dist - closestDist) <= dragPosCombineThreshold
                 && Mathf.Abs(note.Timing - closestTiming) <= 1)
                {
                    switch (note)
                    {
                        case Tap t:
                            dragTap.Add(t);
                            break;
                        case Hold h:
                            dragHold.Add(h);
                            break;
                        case Arc a:
                            dragArcStart.Add(a);
                            draggingArcStart = true;
                            break;
                        case ArcTap at:
                            dragArcTap.Add(at);
                            break;
                    }
                }

                if (!draggingArcStart
                 && note is Arc arc
                 && TryGetDistanceToArcEnd(arc, mousePos, out float endDist)
                 && Mathf.Abs(endDist - closestDist) <= dragPosCombineThreshold
                 && Mathf.Abs(arc.EndTiming - closestTiming) <= 1)
                {
                    dragArcEnd.Add(arc);
                }
            }

            if (dragArcStart.Count > 0 || dragArcEnd.Count > 0)
            {
                await DragArcs(closestTiming, dragArcStart, dragArcEnd, confirm, cancel);
                return;
            }

            if (dragTap.Count > 0)
            {
                await DragLanes(dragTap, confirm, cancel);
                return;
            }

            if (dragHold.Count > 0)
            {
                await DragLanes(dragHold, confirm, cancel);
                return;
            }

            if (dragArcTap.Count > 0)
            {
                await DragArctaps(closestTiming, dragArcTap, confirm, cancel);
                return;
            }
        }

        private async UniTask DragArcs(int timing, List<Arc> dragArcStart, List<Arc> dragArcEnd, SubAction confirm, SubAction cancel)
        {
            List<(ArcEvent instance, ArcEvent newValue)> events = new List<(ArcEvent, ArcEvent)>();
            for (int i = 0; i < dragArcStart.Count; i++)
            {
                Arc arc = dragArcStart[i];
                Arc clone = arc.Clone() as Arc;
                events.Add((arc, clone));
                dragArcStart[i] = clone;
            }

            for (int i = 0; i < dragArcEnd.Count; i++)
            {
                Arc arc = dragArcEnd[i];
                Arc clone = arc.Clone() as Arc;
                events.Add((arc, clone));
                dragArcEnd[i] = clone;
            }

            var command = new EventCommand(
                name: I18n.S("Compose.Notify.History.Drag.Position"),
                update: events);

            Services.Gameplay.Chart.EnableColliderGeneration = false;
            var (success, position) = await Services.Cursor.RequestVerticalSelection(
                confirm,
                cancel,
                timing,
                update: pos =>
                {
                    foreach (var arc in dragArcStart)
                    {
                        arc.XStart = pos.x;
                        arc.YStart = pos.y;
                    }

                    foreach (var arc in dragArcEnd)
                    {
                        arc.XEnd = pos.x;
                        arc.YEnd = pos.y;
                    }

                    command.Execute();
                });

            Services.Gameplay.Chart.EnableColliderGeneration = true;
            if (!success)
            {
                command.Undo();
            }
            else
            {
                Services.History.AddCommand(command);
            }
        }

        private async UniTask DragLanes(List<Note> dragNotes, SubAction confirm, SubAction cancel)
        {
            List<(ArcEvent instance, ArcEvent newValue)> events = new List<(ArcEvent, ArcEvent)>();
            for (int i = 0; i < dragNotes.Count; i++)
            {
                Note tap = dragNotes[i];
                Note clone = tap.Clone() as Note;
                events.Add((tap, clone));
                dragNotes[i] = clone;
            }

            var command = new EventCommand(
                name: I18n.S("Compose.Notify.History.Drag.Position"),
                update: events);

            var (success, resultLane) = await Services.Cursor.RequestLaneSelection(
                confirm,
                cancel,
                update: lane =>
                {
                    foreach (var note in dragNotes)
                    {
                        if (note is Tap t)
                        {
                            t.Lane = lane;
                        }
                        else if (note is Hold h)
                        {
                            h.Lane = lane;
                        }
                    }

                    command.Execute();
                });

            if (!success)
            {
                command.Undo();
            }
            else
            {
                Services.History.AddCommand(command);
            }
        }

        private async UniTask DragArctaps(int timing, List<ArcTap> dragArctap, SubAction confirm, SubAction cancel)
        {
            List<(ArcEvent instance, ArcEvent newValue)> events = new List<(ArcEvent, ArcEvent)>();
            for (int i = 0; i < dragArctap.Count; i++)
            {
                ArcTap arcTap = dragArctap[i];
                ArcTap clone = arcTap.Clone() as ArcTap;
                events.Add((arcTap, clone));
                dragArctap[i] = clone;
            }

            List<Arc> snappableArcs = Services.Gameplay.Chart
                .GetAll<Arc>()
                .Where(a => a.IsTrace && a.Timing <= timing && timing <= a.EndTiming)
                .ToList();

            if (snappableArcs.Count <= 1)
            {
                return;
            }

            var command = new EventCommand(
                name: I18n.S("Compose.Notify.History.Drag.Position"),
                update: events);

            var (success, position) = await Services.Cursor.RequestVerticalSelection(
                confirm,
                cancel,
                timing,
                update: pos =>
                {
                    Arc closestArc = snappableArcs[0];
                    float closestDist = float.MaxValue;
                    for (int i = 0; i < snappableArcs.Count; i++)
                    {
                        Arc arc = snappableArcs[i];
                        float dist = (new Vector2(arc.ArcXAt(timing), arc.ArcYAt(timing)) - pos).sqrMagnitude;
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestArc = arc;
                        }
                    }

                    for (int i = 0; i < dragArctap.Count; i++)
                    {
                        ArcTap arctap = dragArctap[i];
                        arctap.Arc = closestArc;
                    }

                    command.Execute();
                });

            if (!success)
            {
                command.Undo();
            }
            else
            {
                Services.History.AddCommand(command);
            }
        }

        private void Awake()
        {
            Services.Selection.OnSelectionChange += OnSelectionChange;
            gameplayData.OnChartEdit += OnChartEdit;
        }

        private void OnDestroy()
        {
            Services.Selection.OnSelectionChange -= OnSelectionChange;
            gameplayData.OnChartEdit -= OnChartEdit;
        }

        private void OnChartEdit()
        {
            OnSelectionChange(Services.Selection.SelectedNotes);
        }

        private void OnSelectionChange(HashSet<Note> selection)
        {
            if (selection.Count == 1)
            {
                Note note = selection.First();
                if (note is LongNote longNote)
                {
                    longMarker.gameObject.SetActive(true);
                    shortMarker.gameObject.SetActive(false);
                    targetLong = longNote;
                    targetShort = null;
                    longMarker.SetTiming(targetLong.Timing, targetLong.EndTiming);
                }
                else
                {
                    targetShort = note;
                    targetLong = null;
                    shortMarker.gameObject.SetActive(true);
                    longMarker.gameObject.SetActive(false);
                    shortMarker.SetTiming(targetShort.Timing);
                }
            }
            else
            {
                targetShort = null;
                targetLong = null;
                shortMarker.gameObject.SetActive(false);
                longMarker.gameObject.SetActive(false);
            }
        }

        private bool TryGetDistance(Note note, Vector2 mousePos, out float distance)
        {
            if (note is Hold hold)
            {
                int timing = Services.Gameplay.Audio.ChartTiming;
                double fp = note.TimingGroupInstance.GetFloorPosition(timing);
                float z = hold.ZPos(fp);
                float endZ = hold.EndZPos(fp);

                if ((z < -Gameplay.Values.TrackLengthForward && endZ < -Gameplay.Values.TrackLengthForward)
                 || (z > Gameplay.Values.TrackLengthBackward && endZ > Gameplay.Values.TrackLengthBackward))
                {
                    distance = float.MaxValue;
                    return false;
                }

                // very hacky. should really write a better helper class lol
                Vector3 startWorldPosition = new Vector3(Gameplay.ArcFormula.LaneToWorldX(hold.Lane), 0, z);
                Vector3 endWorldPosition = new Vector3(Gameplay.ArcFormula.LaneToWorldX(hold.Lane), 0, endZ);
                Vector2 startScreen = Services.Gameplay.Camera.GameplayCamera.WorldToScreenPoint(startWorldPosition);
                Vector2 endScreen = Services.Gameplay.Camera.GameplayCamera.WorldToScreenPoint(endWorldPosition);

                Line line = new Line()
                {
                    Start = startScreen,
                    End = endScreen,
                };
                Vector2 closestPoint = VerticalGridHelper.ClosestPointToLine(line, mousePos);
                distance = (mousePos - closestPoint).sqrMagnitude;
                return true;
            }

            if (TryGetScreenPosition(note, out Vector2 screenPos))
            {
                distance = (mousePos - screenPos).sqrMagnitude;
                return true;
            }

            distance = float.MaxValue;
            return false;
        }

        private bool TryGetDistanceToArcEnd(Arc arc, Vector2 mousePos, out float distance)
        {
            if (TryGetScreenEndPosition(arc, out Vector2 screenPos))
            {
                distance = (mousePos - screenPos).sqrMagnitude;
                return true;
            }

            distance = float.MaxValue;
            return false;
        }

        private bool TryGetScreenPosition(Note note, out Vector2 screenPos)
        {
            Vector3 worldPosition = default;
            screenPos = default;
            int timing = Services.Gameplay.Audio.ChartTiming;
            float z = note.ZPos(note.TimingGroupInstance.GetFloorPosition(timing));
            if (z < -Gameplay.Values.TrackLengthForward || z > Gameplay.Values.TrackLengthBackward)
            {
                return false;
            }

            switch (note)
            {
                case ArcTap t:
                    worldPosition = new Vector3(t.WorldX, t.WorldY, z);
                    break;
                case Tap t:
                    worldPosition = new Vector3(Gameplay.ArcFormula.LaneToWorldX(t.Lane), 0, z);
                    break;
                case Arc t:
                    worldPosition = new Vector3(t.WorldXAt(t.Timing), t.WorldYAt(t.Timing), z);
                    break;
                default:
                    return false;
            }

            screenPos = Services.Gameplay.Camera.GameplayCamera.WorldToScreenPoint(worldPosition);
            return true;
        }

        private bool TryGetScreenEndPosition(Arc note, out Vector2 screenPos)
        {
            screenPos = default;
            int timing = Services.Gameplay.Audio.ChartTiming;
            float z = note.EndZPos(note.TimingGroupInstance.GetFloorPosition(timing));
            if (z < -Gameplay.Values.TrackLengthForward || z > Gameplay.Values.TrackLengthBackward)
            {
                return false;
            }

            Vector3 worldPosition = new Vector3(note.WorldXAt(note.EndTiming + 1), note.WorldYAt(note.EndTiming + 1), z);

            screenPos = Services.Gameplay.Camera.GameplayCamera.WorldToScreenPoint(worldPosition);
            return true;
        }
    }
}