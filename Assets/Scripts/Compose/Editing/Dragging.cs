using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Grid;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

        private readonly List<(Arc, Vector2)> snappableArcs = new List<(Arc, Vector2)>();
        private readonly List<Arc> pairedArcs = new List<Arc>(32);
        private readonly List<ArcTap> pairedArcTaps = new List<ArcTap>(32);
        private readonly List<(Arc, ArcTap, float)> pairings = new List<(Arc, ArcTap, float)>(32);

        private enum PositionArctapMode
        {
            SnapToExisting,
            Free,
            Connected,
        }

        [EditorAction("Timing", false, "<a-mouse1>")]
        [SubAction("Confirm", false, "<u-mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [KeybindHint(Priority = KeybindPriorities.Dragging + 1)]
        [KeybindHint("Confirm", Priority = KeybindPriorities.SubConfirm)]
        [KeybindHint("Cancel", Priority = KeybindPriorities.SubCancel)]
        [WhitelistScopes(typeof(GridService), typeof(TimelineService))]
        public async UniTask DragTiming(EditorAction action)
        {
            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Cancel");

            List<Note> selection = GetSelection().ToList();

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
            List<ArcTap> arcTaps = new List<ArcTap>();
            List<Arc> oldParentArcs = new List<Arc>();
            List<Note> dragStart = new List<Note>();
            List<LongNote> dragEnd = new List<LongNote>();
            List<LongNote> dragBoth = new List<LongNote>();
            int limitLower = int.MinValue;
            int limitUpper = int.MaxValue;

            for (int i = 0; i < selection.Count; i++)
            {
                Note note = selection[i];
                if (Mathf.Abs(note.Timing - closestTiming) <= 1)
                {
                    bool alsoDraggingEnd = note is LongNote longNote
                                         && Mathf.Abs(longNote.EndTiming - closestTiming) <= 1;
                    switch (note)
                    {
                        case Tap tap:
                            Note clonetap = note.Clone() as Note;
                            events.Add((note, clonetap));
                            dragStart.Add(clonetap);
                            break;
                        case Hold hold:
                            if (!alsoDraggingEnd || cursorTiming < closestTiming)
                            {
                                Note clonehold = note.Clone() as Note;
                                events.Add((note, clonehold));
                                dragStart.Add(clonehold);
                                limitUpper = Mathf.Min(limitUpper, hold.EndTiming - 1);
                            }

                            break;
                        case ArcTap arcTap:
                            ArcTap cloneArctap = arcTap.Clone() as ArcTap;
                            events.Add((note, cloneArctap));
                            arcTaps.Add(cloneArctap);
                            if (!oldParentArcs.Contains(arcTap.Arc))
                            {
                                oldParentArcs.Add(arcTap.Arc);
                            }

                            break;
                        case Arc arc:
                            bool sameStartAndEnd = arc.XStart == arc.XEnd
                                                && arc.YStart == arc.YEnd;

                            if (!alsoDraggingEnd
                             || (arc.EndTiming != arc.Timing && cursorTiming < closestTiming)
                             || (arc.EndTiming == arc.Timing && cursorTiming < closestTiming - 1))
                            {
                                Arc clonearc = arc.Clone() as Arc;
                                events.Add((note, clonearc));
                                dragStart.Add(clonearc);
                                limitUpper = Mathf.Min(limitUpper, sameStartAndEnd ? arc.EndTiming - 1 : arc.EndTiming);
                            }
                            else if (arc.EndTiming == arc.Timing)
                            {
                                Arc clonearc = arc.Clone() as Arc;
                                events.Add((note, clonearc));
                                dragBoth.Add(clonearc);
                            }

                            foreach (var at in Services.Gameplay.Chart.GetAll<ArcTap>()
                                .Where(at => at.Arc == arc && !selection.Contains(at)))
                            {
                                limitUpper = Mathf.Min(limitUpper, at.Timing);
                            }

                            break;
                    }
                }

                if (note is LongNote l && Mathf.Abs(l.EndTiming - closestTiming) <= 1)
                {
                    bool alsoDraggingStart = Mathf.Abs(l.Timing - closestTiming) <= 1;
                    switch (l)
                    {
                        case Hold hold:
                            if (!alsoDraggingStart || cursorTiming >= closestTiming)
                            {
                                LongNote clonehold = hold.Clone() as LongNote;
                                events.Add((note, clonehold));
                                dragEnd.Add(clonehold);
                                limitLower = Mathf.Max(limitLower, hold.Timing + 1);
                            }

                            break;

                        case Arc arc:
                            bool sameStartAndEnd = arc.XStart == arc.XEnd
                                                && arc.YStart == arc.YEnd;

                            if (!alsoDraggingStart
                             || (arc.EndTiming != arc.Timing && cursorTiming >= closestTiming)
                             || (arc.EndTiming == arc.Timing && cursorTiming > closestTiming + 1))
                            {
                                Arc clonearc = arc.Clone() as Arc;
                                events.Add((note, clonearc));
                                dragEnd.Add(clonearc);
                                limitLower = Mathf.Max(limitLower, sameStartAndEnd ? arc.Timing + 1 : arc.Timing);
                            }

                            foreach (var at in Services.Gameplay.Chart.GetAll<ArcTap>()
                                .Where(at => at.Arc == arc && !selection.Contains(at)))
                            {
                                limitLower = Mathf.Max(limitLower, at.Timing);
                            }

                            break;
                    }
                }
            }

            Debug.Log($"{limitUpper} - {limitLower}");

            var command = new EventCommand(
                name: string.Empty,
                update: events);
            var (success, timing) = await Services.Cursor.RequestTimingSelection(
                confirm,
                cancel,
                update: t =>
                {
                    t = Mathf.Clamp(t, limitLower, limitUpper);

                    if (arcTaps.Count > 0)
                    {
                        snappableArcs.Clear();
                        pairedArcs.Clear();
                        pairedArcTaps.Clear();
                        pairings.Clear();
                        snappableArcs.AddRange(Services.Gameplay.Chart
                            .FindEventsWithinRange<Arc>(t, t, false)
                            .Where(a => a.IsTrace)
                            .Select(a => (a, new Vector2(a.WorldXAt(t), a.WorldYAt(t)))));
                    }

                    for (int i = 0; i < arcTaps.Count; i++)
                    {
                        ArcTap at = arcTaps[i];
                        Arc arc = at.Arc;

                        Vector2 pos = new Vector2(arc.WorldXAt(t), arc.WorldYAt(t));
                        bool hasDraggedToThisPos = false;
                        for (int j = 0; j < i; j++)
                        {
                            ArcTap prev = arcTaps[j];
                            Vector2 prevPos = new Vector2(prev.WorldX, prev.WorldY);
                            if (prev.Timing == t
                             && (prevPos - pos).sqrMagnitude <= Values.TapOverlapWarningThreshold)
                            {
                                hasDraggedToThisPos = true;
                                break;
                            }
                        }

                        if (!hasDraggedToThisPos)
                        {
                            at.Timing = Mathf.Clamp(t, arc.Timing, arc.EndTiming);
                            pairedArcs.Add(arc);
                        }

                        if (t > arc.EndTiming || t < arc.Timing)
                        {
                            foreach (var a in snappableArcs)
                            {
                                if (a.Item1.TimingGroup != at.TimingGroup)
                                {
                                    continue;
                                }

                                float dist = (pos - a.Item2).sqrMagnitude;
                                pairings.Add((a.Item1, at, dist));
                            }
                        }
                    }

                    if (pairings.Count > 0)
                    {
                        pairings.Sort((a, b) => a.Item3.CompareTo(b.Item3));
                        for (int i = 0; i < pairings.Count; i++)
                        {
                            var (arc, arcTap, _) = pairings[i];
                            if (pairedArcs.Contains(arc) || pairedArcTaps.Contains(arcTap))
                            {
                                continue;
                            }

                            arcTap.Arc = arc;
                            pairedArcs.Add(arc);
                            pairedArcTaps.Add(arcTap);
                        }
                    }

                    foreach (var note in dragStart)
                    {
                        note.Timing = t;
                    }

                    foreach (var note in dragEnd)
                    {
                        note.EndTiming = t;
                    }

                    foreach (var note in dragBoth)
                    {
                        note.Timing = t;
                        note.EndTiming = t;
                    }

                    command.Execute();
                });

            if (!success)
            {
                command.Undo();
            }
            else
            {
                List<ArcEvent> remove = null;
                foreach (var arc in oldParentArcs)
                {
                    if (arc != null
                     && arc.Timing >= arc.EndTiming - 1
                     && arc.XStart == arc.XEnd
                     && arc.YStart == arc.YEnd
                     && Services.Gameplay.Chart.GetAll<ArcTap>().Where(a => a.Arc == arc).Count() == 0)
                    {
                        remove = remove ?? new List<ArcEvent>();
                        remove.Add(arc);
                    }
                }

                var removecmd = new EventCommand(
                    name: string.Empty,
                    remove: remove);

                Services.History.AddCommand(new CombinedCommand(
                    I18n.S("Compose.Notify.History.Drag.Timing"),
                    command,
                    removecmd));
            }
        }

        [EditorAction("Position", false, "<s-mouse1>")]
        [SubAction("CycleSkyNoteMode", false, "<s>")]
        [SubAction("Confirm", false, "<u-mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [KeybindHint(Priority = KeybindPriorities.Dragging)]
        [KeybindHint("CycleSkyNoteMode", Priority = KeybindPriorities.SubConfirm + 1)]
        [KeybindHint("Confirm", Priority = KeybindPriorities.SubConfirm)]
        [KeybindHint("Cancel", Priority = KeybindPriorities.SubCancel)]
        [WhitelistScopes(typeof(GridService), typeof(TimelineService))]
        public async UniTask DragPosition(EditorAction action)
        {
            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Cancel");
            SubAction cycleSky = action.GetSubAction("CycleSkyNoteMode");

            IEnumerable<Note> selection = GetSelection();

            Vector2 mousePos = Input.mousePosition;
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
                await DragArctaps(closestTiming, dragArcTap, confirm, cancel, cycleSky);
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

        private async UniTask DragArctaps(int timing, List<ArcTap> dragArctap, SubAction confirm, SubAction cancel, SubAction cycleMode)
        {
            // this is a mess.
            PositionArctapMode mode = PositionArctapMode.SnapToExisting;

            List<Arc> oldParentArcs = new List<Arc>();
            Vector2 oldPosition = Vector2.zero;
            List<(ArcEvent instance, ArcEvent newValue)> events = new List<(ArcEvent, ArcEvent)>();
            for (int i = 0; i < dragArctap.Count; i++)
            {
                ArcTap arcTap = dragArctap[i];
                ArcTap clone = arcTap.Clone() as ArcTap;
                events.Add((arcTap, clone));
                dragArctap[i] = clone;

                if (!oldParentArcs.Contains(arcTap.Arc))
                {
                    oldParentArcs.Add(arcTap.Arc);
                    oldPosition = new Vector2(arcTap.Arc.ArcXAt(timing), arcTap.Arc.ArcYAt(timing));
                }
            }

            List<Arc> snappableArcs = Services.Gameplay.Chart
                .GetAll<Arc>()
                .Where(a => a.IsTrace && a.Timing <= timing && timing <= a.EndTiming)
                .ToList();

            Arc freeArc = new Arc
            {
                Timing = timing,
                EndTiming = timing + 1,
                LineType = Values.CreateArcTypeMode.Value,
                Color = Values.CreateArcColorMode.Value,
                Sfx = "none",
                IsTrace = true,
                TimingGroup = Values.EditingTimingGroup.Value,
            };

            var command = new EventCommand(
                name: string.Empty,
                update: events);

            var freeArcEvents = new ArcEvent[] { freeArc };
            var addcmd = new EventCommand(
                name: string.Empty,
                add: freeArcEvents);

            void UpdateNotes(Vector2 pos)
            {
                if (mode == PositionArctapMode.SnapToExisting)
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

                    for (int i = 0; i < events.Count; i++)
                    {
                        ArcTap arctap = dragArctap[i];
                        arctap.Arc = closestArc;
                        if (arctap.TimingGroup != closestArc.TimingGroup)
                        {
                            arctap.TimingGroup = closestArc.TimingGroup;
                        }
                    }
                }
                else if (mode == PositionArctapMode.Free)
                {
                    freeArc.XStart = pos.x;
                    freeArc.XEnd = pos.x;
                    freeArc.YStart = pos.y;
                    freeArc.YEnd = pos.y;
                    for (int i = 0; i < dragArctap.Count; i++)
                    {
                        ArcTap arctap = dragArctap[i];
                        arctap.Arc = freeArc;
                        if (arctap.TimingGroup != freeArc.TimingGroup)
                        {
                            arctap.TimingGroup = freeArc.TimingGroup;
                        }
                    }

                    Services.Gameplay.Chart.UpdateEvents(freeArcEvents);
                }
                else if (mode == PositionArctapMode.Connected)
                {
                    freeArc.XStart = pos.x;
                    freeArc.XEnd = oldPosition.x;
                    freeArc.YStart = pos.y;
                    freeArc.YEnd = oldPosition.y;
                    for (int i = 0; i < dragArctap.Count; i++)
                    {
                        ArcTap arctap = dragArctap[i];
                        arctap.Arc = freeArc;
                        arctap.Timing = timing;
                        if (arctap.TimingGroup != freeArc.TimingGroup)
                        {
                            arctap.TimingGroup = freeArc.TimingGroup;
                        }
                    }

                    Services.Gameplay.Chart.UpdateEvents(freeArcEvents);
                }
            }

            var (success, position) = await Services.Cursor.RequestVerticalSelection(
                confirm,
                cancel,
                timing,
                update: pos =>
                {
                    if (cycleMode.WasExecuted)
                    {
                        bool neededNew = mode != PositionArctapMode.SnapToExisting;
                        mode = CycleArctapMode(mode);
                        bool needNew = mode != PositionArctapMode.SnapToExisting;
                        if (!neededNew && needNew)
                        {
                            addcmd.Execute();
                        }
                        else if (neededNew & !needNew)
                        {
                            addcmd.Undo();
                        }
                    }

                    UpdateNotes(pos);
                    command.Execute();
                });

            if (!success)
            {
                command.Undo();
                return;
            }

            IEnumerable<ArcEvent> add = mode == PositionArctapMode.SnapToExisting ? null : new ArcEvent[] { freeArc };
            List<ArcEvent> remove = null;

            UpdateNotes(position);

            foreach (var arc in oldParentArcs)
            {
                if (arc != null
                 && arc.Timing >= arc.EndTiming - 1
                 && arc.XStart == arc.XEnd
                 && arc.YStart == arc.YEnd
                 && Services.Gameplay.Chart.GetAll<ArcTap>().Where(a => a.Arc == arc).Count() == 0)
                {
                    remove = remove ?? new List<ArcEvent>();
                    remove.Add(arc);
                }
            }

            var removecmd = new EventCommand(
                name: string.Empty,
                remove: remove);
            command.Execute();
            removecmd.Execute();
            Services.History.AddCommandWithoutExecuting(
                new CombinedCommand(
                    name: I18n.S("Compose.Notify.History.Drag.Position"),
                    command,
                    addcmd,
                    removecmd));
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

        private IEnumerable<Note> GetSelection()
        {
            if (Services.Selection.SelectedNotes?.Count > 0)
            {
                return Services.Selection.SelectedNotes;
            }
            else
            {
                return Services.Gameplay.Chart.GetRenderingNotes();
            }
        }

        private PositionArctapMode CycleArctapMode(PositionArctapMode mode)
        {
            switch (mode)
            {
                case PositionArctapMode.SnapToExisting:
                    return PositionArctapMode.Free;
                case PositionArctapMode.Free:
                    return PositionArctapMode.Connected;
                case PositionArctapMode.Connected:
                    return PositionArctapMode.SnapToExisting;
            }

            return PositionArctapMode.SnapToExisting;
        }
    }
}