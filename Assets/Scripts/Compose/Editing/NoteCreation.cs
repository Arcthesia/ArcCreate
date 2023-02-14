using System.Collections.Generic;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Navigation;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.Compose.Editing
{
    [EditorScope("NoteCreation")]
    public class NoteCreation
    {
        [EditorAction("Start", false, "<u-mouse1>")]
        [SubAction("Confirm", false, "<u-mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [RequireGameplayLoaded]
        [WhitelistScopes(typeof(Timeline.TimelineService), typeof(Grid.GridService))]
        public async UniTask StartCreatingNote(EditorAction action)
        {
            if (!Services.Cursor.IsHittingLane)
            {
                return;
            }

            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Cancel");

            switch (Values.CreateNoteMode.Value)
            {
                case CreateNoteMode.Idle:
                    return;
                case CreateNoteMode.Tap:
                    CreateTap();
                    return;
                case CreateNoteMode.Hold:
                    await CreateHold(confirm, cancel);
                    return;
                case CreateNoteMode.Arc:
                    await CreateArc(isArc: true, confirm, cancel);
                    return;
                case CreateNoteMode.Trace:
                    await CreateArc(isArc: false, confirm, cancel);
                    return;
                case CreateNoteMode.ArcTap:
                    await CreateArcTap(confirm, cancel);
                    return;
            }
        }

        private void CreateTap()
        {
            Tap tap = new Tap()
            {
                Timing = Services.Cursor.CursorTiming,
                Lane = Services.Cursor.CursorLane,
                TimingGroup = Values.EditingTimingGroup.Value,
            };

            Services.History.AddCommand(new EventCommand(
                I18n.S("Compose.Notify.History.CreateNote.Tap"),
                add: new ArcEvent[] { tap }));
        }

        private async UniTask CreateHold(SubAction confirm, SubAction cancel)
        {
            Hold hold = new Hold()
            {
                Timing = Services.Cursor.CursorTiming,
                EndTiming = Services.Cursor.CursorTiming + 1,
                Lane = Services.Cursor.CursorLane,
                TimingGroup = Values.EditingTimingGroup.Value,
            };

            IEnumerable<ArcEvent> events = new ArcEvent[] { hold };
            Services.Gameplay.Chart.AddEvents(events);

            var (success, endTiming) = await Services.Cursor.RequestTimingSelection(
                confirm,
                cancel,
                update: t =>
                {
                    hold.EndTiming = t;
                    Services.Gameplay.Chart.UpdateEvents(events);
                },
                constraint: t => t > hold.Timing);

            Services.Gameplay.Chart.RemoveEvents(events);
            if (success)
            {
                hold.EndTiming = endTiming;
                Services.History.AddCommand(new EventCommand(
                    I18n.S("Compose.Notify.History.CreateNote.Hold"),
                    add: events));
            }
        }

        private async UniTask CreateArc(bool isArc, SubAction confirm, SubAction cancel)
        {
            Arc arc = new Arc()
            {
                Timing = Services.Cursor.CursorTiming,
                EndTiming = Services.Cursor.CursorTiming + 1,
                LineType = Values.CreateArcTypeMode.Value,
                Color = Values.CreateArcColorMode.Value,
                Sfx = "none",
                IsTrace = !isArc,
                TimingGroup = Values.EditingTimingGroup.Value,
            };

            IEnumerable<ArcEvent> events = new ArcEvent[] { arc };
            Services.Gameplay.Chart.AddEvents(events);

            var (startPosSuccess, startPos) = await Services.Cursor.RequestVerticalSelection(
                confirm,
                cancel,
                showGridAtTiming: arc.Timing,
                update: pos =>
                {
                    arc.XStart = pos.x;
                    arc.YStart = pos.y;
                    arc.XEnd = pos.x;
                    arc.YEnd = pos.y;
                    Services.Gameplay.Chart.UpdateEvents(events);
                });

            if (!startPosSuccess)
            {
                Services.Gameplay.Chart.RemoveEvents(events);
                return;
            }
            else
            {
                arc.XStart = startPos.x;
                arc.YStart = startPos.y;
            }

            var (endTimingSuccess, endTiming) = await Services.Cursor.RequestTimingSelection(
                confirm,
                cancel,
                update: t =>
                {
                    arc.EndTiming = t;
                    Services.Gameplay.Chart.UpdateEvents(events);
                },
                constraint: t => t > arc.Timing);

            if (!endTimingSuccess)
            {
                Services.Gameplay.Chart.RemoveEvents(events);
                return;
            }
            else
            {
                arc.EndTiming = endTiming;
            }

            var (endPosSuccess, endPos) = await Services.Cursor.RequestVerticalSelection(
                confirm,
                cancel,
                showGridAtTiming: arc.EndTiming,
                update: position =>
                {
                    arc.XEnd = position.x;
                    arc.YEnd = position.y;
                    Services.Gameplay.Chart.UpdateEvents(events);
                });

            Services.Gameplay.Chart.RemoveEvents(events);
            if (endPosSuccess)
            {
                arc.XEnd = endPos.x;
                arc.YEnd = endPos.y;

                Services.History.AddCommand(new EventCommand(
                    I18n.S(isArc ? "Compose.Notify.History.CreateNote.Arc" : "Compose.Notify.History.CreateNote.Trace"),
                    add: events));
            }
        }

        private async UniTask CreateArcTap(SubAction confirm, SubAction cancel)
        {
            int timing = Services.Cursor.CursorTiming;
            List<Arc> elligibleArcs = new List<Arc>();
            foreach (var note in Services.Selection.SelectedNotes)
            {
                if (note is Arc arc && arc.Timing <= timing && timing <= arc.EndTiming)
                {
                    elligibleArcs.Add(arc);
                }
            }

            if (elligibleArcs.Count == 0)
            {
                foreach (var arc in Services.Gameplay.Chart.GetAll<Arc>())
                {
                    if (arc.IsTrace && arc.Timing <= timing && timing <= arc.EndTiming)
                    {
                        elligibleArcs.Add(arc);
                    }
                }
            }

            if (elligibleArcs.Count == 0)
            {
                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Exception.CreateSkyNoteNoValidArc"));
                return;
            }

            ArcTap arctap = new ArcTap()
            {
                Timing = timing,
                TimingGroup = elligibleArcs[0].TimingGroup,
                Arc = elligibleArcs[0],
            };

            IEnumerable<ArcEvent> events = new ArcEvent[] { arctap };
            if (elligibleArcs.Count > 1)
            {
                Services.Gameplay.Chart.AddEvents(events);
                var (success, pos) = await Services.Cursor.RequestVerticalSelection(
                    confirm,
                    cancel,
                    showGridAtTiming: timing,
                    update: p => arctap.Arc = GetClosestArc(timing, elligibleArcs, p));

                Services.Gameplay.Chart.RemoveEvents(events);
                if (success)
                {
                    arctap.Arc = GetClosestArc(timing, elligibleArcs, pos);
                }
                else
                {
                    return;
                }
            }

            Services.History.AddCommand(new EventCommand(
                I18n.S("Compose.Notify.History.CreateNote.ArcTap"),
                add: events));
        }

        private Arc GetClosestArc(int timing, List<Arc> arcs, Vector2 arcPos)
        {
            float closestDist = float.MaxValue;
            Arc closestArc = arcs[0];

            foreach (var arc in arcs)
            {
                Vector2 pos = new Vector2(arc.ArcXAt(timing), arc.ArcYAt(timing));
                float dist = (arcPos - pos).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestArc = arc;
                    closestDist = dist;
                }
            }

            return closestArc;
        }
    }
}