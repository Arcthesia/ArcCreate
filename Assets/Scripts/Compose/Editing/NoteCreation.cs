using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Navigation;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.Compose.Editing
{
    [EditorScope("NoteCreation")]
    public class NoteCreation : MonoBehaviour
    {
        [SerializeField] private Transform previewTap;
        [SerializeField] private Transform previewHold;
        [SerializeField] private Transform previewArcTap;
        [SerializeField] private Transform previewArc;
        [SerializeField] private Transform previewTrace;

        private readonly HashSet<Arc> selectedArcs = new HashSet<Arc>();

        private bool AllowCreatingNoteBackwards => Settings.AllowCreatingNotesBackward.Value;

        [EditorAction("Start", false, "<mouse1>")]
        [SubAction("ToggleFreeSky", false, "<s>")]
        [SubAction("Confirm", false, "<mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [KeybindHint(Priority = KeybindPriorities.NoteCreation)]
        [KeybindHint("ToggleFreeSky", Priority = KeybindPriorities.SubConfirm + 1)]
        [KeybindHint("Confirm", Priority = KeybindPriorities.SubConfirm)]
        [KeybindHint("Cancel", Priority = KeybindPriorities.SubCancel)]
        [RequireGameplayLoaded]
        [NotePallete.RequirePallete]
        [WhitelistScopes(typeof(Timeline.TimelineService), typeof(Grid.GridService), typeof(Cursor.CursorService), typeof(NotePallete), typeof(Mirror))]
        public async UniTask StartCreatingNote(EditorAction action)
        {
            int selectionSize = Services.Selection.SelectedNotes.Count;
            await UniTask.NextFrame();
            await UniTask.NextFrame();
            bool selectionChanged = Services.Selection.SelectedNotes.Count != selectionSize;

            if (!Services.Cursor.IsHittingLane
             || selectionChanged
             || (Values.CreateNoteMode.Value != CreateNoteMode.ArcTap && selectionSize != 0))
            {
                return;
            }

            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Cancel");
            SubAction toggleFreeSky = action.GetSubAction("ToggleFreeSky");

            CreateNoteMode mode = Values.CreateNoteMode.Value;
            switch (mode)
            {
                case CreateNoteMode.Idle:
                    break;
                case CreateNoteMode.Tap:
                    if (Settings.SnapFloorNoteWithGrid.Value)
                    {
                        toggleFreeSky.ForceDisabled = true;
                        await CreateDecimalTap(confirm, cancel);
                    }
                    else
                    {
                        CreateTap();
                    }
                    break;
                case CreateNoteMode.Hold:
                    toggleFreeSky.ForceDisabled = true;
                    if (Settings.SnapFloorNoteWithGrid.Value)
                    {
                        await CreateDecimalHold(confirm, cancel);
                    }
                    else
                    {
                        await CreateHold(confirm, cancel);
                    }
                    break;
                case CreateNoteMode.Arc:
                    toggleFreeSky.ForceDisabled = true;
                    await CreateArc(isArc: true, confirm, cancel);
                    break;
                case CreateNoteMode.Trace:
                    toggleFreeSky.ForceDisabled = true;
                    await CreateArc(isArc: false, confirm, cancel);
                    break;
                case CreateNoteMode.ArcTap:
                    await CreateArcTap(toggleFreeSky, confirm, cancel);
                    break;
            }

            Values.CreateNoteMode.Value = mode;
        }

        private void CreateTap()
        {
            int timing = Services.Cursor.CursorTiming;
            int lane = Services.Cursor.CursorLane;
            if (Settings.BlockOverlapNoteCreation.Value
            && HasOverlap(timing, lane))
            {
                Services.Popups.Notify(Popups.Severity.Warning, I18n.S("Compose.Notify.Creation.Overlap"));
                return;
            }

            Tap tap = new Tap()
            {
                Timing = timing,
                Lane = lane,
                TimingGroup = Values.EditingTimingGroup.Value,
            };

            Services.History.AddCommand(new EventCommand(
                I18n.S("Compose.Notify.History.CreateNote.Tap"),
                add: new ArcEvent[] { tap }));
        }

        private async UniTask CreateHold(SubAction confirm, SubAction cancel)
        {
            int timing1 = Services.Cursor.CursorTiming;
            int lane = Services.Cursor.CursorLane;
            if (Settings.BlockOverlapNoteCreation.Value
             && HasOverlap(timing1, lane))
            {
                Services.Popups.Notify(Popups.Severity.Warning, I18n.S("Compose.Notify.Creation.Overlap"));
                return;
            }

            Hold hold = new Hold()
            {
                Timing = timing1,
                EndTiming = timing1 + 1,
                Lane = lane,
                TimingGroup = Values.EditingTimingGroup.Value,
            };

            IEnumerable<ArcEvent> events = new ArcEvent[] { hold };
            var command = new EventCommand(
                I18n.S("Compose.Notify.History.CreateNote.Hold"),
                add: events);
            command.Execute();

            using (new NoteModifyTarget(new List<Note> { hold }))
            {
                previewHold.gameObject.SetActive(false);
                var (success, timing2) = await Services.Cursor.RequestTimingSelection(
                    confirm,
                    cancel,
                    update: t =>
                    {
                        hold.Timing = Mathf.Min(timing1, t);
                        hold.EndTiming = Mathf.Max(timing1, t);
                        Services.Gameplay.Chart.UpdateEvents(events);
                    },
                    constraint: t => t != timing1 && (AllowCreatingNoteBackwards || t > timing1));
                previewHold.gameObject.SetActive(false);
                Services.Cursor.EnableLaneCursor = true;

                if (success)
                {
                    hold.Timing = Mathf.Min(timing1, timing2);
                    hold.EndTiming = Mathf.Max(timing1, timing2);
                    Services.History.AddCommandWithoutExecuting(command);
                }
                else
                {
                    command.Undo();
                }
            }
        }

        private async UniTask CreateDecimalTap(SubAction confirm, SubAction cancel)
        {
            int timing = Services.Cursor.CursorTiming;
            Tap tap = new Tap()
            {
                Timing = timing,
                Lane = Gameplay.Values.InvalidLane,
                TimingGroup = Values.EditingTimingGroup.Value,
            };

            IEnumerable<ArcEvent> events = new ArcEvent[] { tap };
            var command = new EventCommand(
                I18n.S("Compose.Notify.History.CreateNote.Tap"),
                add: events);
            command.Execute();

            using (new NoteModifyTarget(new List<Note> { tap }))
            {
                previewTap.gameObject.SetActive(false);
                var (posSuccess, pos) = await Services.Cursor.RequestVerticalSelection(
                    confirm,
                    cancel,
                    showGridAtTiming: timing,
                    update: p =>
                    {
                        tap.Lane = ArcFormula.ArcXToLane(p.x);
                        Services.Gameplay.Chart.UpdateEvents(events);
                    });

                if (!posSuccess)
                {
                    command.Undo();
                }
                else
                {
                    tap.Lane = ArcFormula.ArcXToLane(pos.x);
                    Services.History.AddCommandWithoutExecuting(command);
                }

            }
        }

        private async UniTask CreateDecimalHold(SubAction confirm, SubAction cancel)
        {
            int timing1 = Services.Cursor.CursorTiming;
            int lane = Services.Cursor.CursorLane;
            if (Settings.BlockOverlapNoteCreation.Value
             && HasOverlap(timing1, lane))
            {
                Services.Popups.Notify(Popups.Severity.Warning, I18n.S("Compose.Notify.Creation.Overlap"));
                return;
            }

            Hold hold = new Hold()
            {
                Timing = timing1,
                EndTiming = timing1 + 1,
                Lane = Gameplay.Values.InvalidLane,
                TimingGroup = Values.EditingTimingGroup.Value,
            };

            IEnumerable<ArcEvent> events = new ArcEvent[] { hold };
            var command = new EventCommand(
                I18n.S("Compose.Notify.History.CreateNote.Hold"),
                add: events);
            command.Execute();

            using (new NoteModifyTarget(new List<Note> { hold }))
            {
                previewHold.gameObject.SetActive(false);
                var (posSuccess, pos) = await Services.Cursor.RequestVerticalSelection(
                    confirm,
                    cancel,
                    showGridAtTiming: timing1,
                    update: p =>
                    {
                        hold.Lane = ArcFormula.ArcXToLane(p.x);
                        Services.Gameplay.Chart.UpdateEvents(events);
                    });
                if (posSuccess)
                {
                    hold.Lane = ArcFormula.ArcXToLane(pos.x);
                }
                else
                {
                    
                    command.Undo();
                    return;
                }
                var (success, timing2) = await Services.Cursor.RequestTimingSelection(
                    confirm,
                    cancel,
                    update: t =>
                    {
                        hold.Timing = Mathf.Min(timing1, t);
                        hold.EndTiming = Mathf.Max(timing1, t);
                        Services.Gameplay.Chart.UpdateEvents(events);
                    },
                    constraint: t => t != timing1 && (AllowCreatingNoteBackwards || t > timing1));
                previewHold.gameObject.SetActive(false);
                Services.Cursor.EnableLaneCursor = true;

                if (success)
                {
                    hold.Timing = Mathf.Min(timing1, timing2);
                    hold.EndTiming = Mathf.Max(timing1, timing2);
                    Services.History.AddCommandWithoutExecuting(command);
                }
                else
                {
                    command.Undo();
                }
            }
        }

        private async UniTask CreateArc(bool isArc, SubAction confirm, SubAction cancel)
        {
            int timing1 = Services.Cursor.CursorTiming;
            Arc arc = new Arc()
            {
                Timing = timing1,
                EndTiming = timing1 + 1,
                LineType = Values.CreateArcTypeMode.Value,
                Color = Values.CreateArcColorMode.Value,
                Sfx = "none",
                IsTrace = !isArc,
                TimingGroup = Values.EditingTimingGroup.Value,
            };

            IEnumerable<ArcEvent> events = new ArcEvent[] { arc };
            var command = new EventCommand(
                I18n.S("Compose.Notify.History.CreateNote.Hold"),
                add: events);
            command.Execute();

            using (new NoteModifyTarget(new List<Note> { arc }))
            {
                (isArc ? previewArc : previewTrace).gameObject.SetActive(false);
                var (pos1Success, pos1) = await Services.Cursor.RequestVerticalSelection(
                    confirm,
                    cancel,
                    showGridAtTiming: arc.Timing,
                    update: p =>
                    {
                        arc.XStart = p.x;
                        arc.YStart = p.y;
                        arc.XEnd = p.x;
                        arc.YEnd = p.y;
                        Services.Gameplay.Chart.UpdateEvents(events);
                    });

                if (!pos1Success)
                {
                    command.Undo();
                    return;
                }
                else
                {
                    arc.XStart = pos1.x;
                    arc.YStart = pos1.y;
                }

                var (timing2Success, timing2) = await Services.Cursor.RequestTimingSelection(
                    confirm,
                    cancel,
                    update: t =>
                    {
                        arc.Timing = Mathf.Min(timing1, t);
                        arc.EndTiming = Mathf.Max(timing1, t);
                        Services.Gameplay.Chart.UpdateEvents(events);
                    },
                    constraint: t => AllowCreatingNoteBackwards || t >= timing1);

                if (!timing2Success)
                {
                    command.Undo();
                    return;
                }
                else
                {
                    arc.Timing = Mathf.Min(timing1, timing2);
                    arc.EndTiming = Mathf.Max(timing1, timing2);
                }

                var (pos2Success, pos2) = await Services.Cursor.RequestVerticalSelection(
                    confirm,
                    cancel,
                    showGridAtTiming: timing2,
                    update: p =>
                    {
                        if (timing1 <= timing2)
                        {
                            arc.XEnd = p.x;
                            arc.YEnd = p.y;
                        }
                        else
                        {
                            arc.XStart = p.x;
                            arc.YStart = p.y;
                        }

                        Services.Gameplay.Chart.UpdateEvents(events);
                    });
                (isArc ? previewArc : previewTrace).gameObject.SetActive(false);
                Services.Cursor.EnableLaneCursor = true;

                if (pos2Success)
                {
                    if (timing1 <= timing2)
                    {
                        arc.XEnd = pos2.x;
                        arc.YEnd = pos2.y;
                    }
                    else
                    {
                        arc.XStart = pos2.x;
                        arc.YStart = pos2.y;
                    }

                    Services.History.AddCommandWithoutExecuting(command);
                }
                else
                {
                    command.Undo();
                }
            }
        }

        private async UniTask CreateArcTap(SubAction toggleFreeSky, SubAction confirm, SubAction cancel)
        {
            bool blockOverlap = Settings.BlockOverlapNoteCreation.Value;
            int timing = Services.Cursor.CursorTiming;
            List<Arc> elligibleArcs = new List<Arc>();
            HashSet<Arc> blockedArcs = Services.Gameplay.Chart.FindEventsWithinRange<ArcTap>(timing - 1, timing + 1)
                .Select(a => a.Arc).ToHashSet();
            foreach (var note in Services.Selection.SelectedNotes)
            {
                if (note is Arc arc && arc.Timing <= timing && timing <= arc.EndTiming
                 && !blockedArcs.Contains(arc))
                {
                    elligibleArcs.Add(arc);
                }
            }

            bool arcsSelected = elligibleArcs.Count > 0;
            if (elligibleArcs.Count == 0)
            {
                foreach (var arc in Services.Gameplay.Chart.GetAll<Arc>())
                {
                    if (arc.IsTrace && arc.Timing <= timing && timing <= arc.EndTiming
                     && !blockedArcs.Contains(arc))
                    {
                        elligibleArcs.Add(arc);
                    }
                }
            }

            if (elligibleArcs.Count == 0)
            {
                toggleFreeSky.ForceDisabled = true;
                Arc arc = new Arc()
                {
                    Timing = timing,
                    EndTiming = timing + 1,
                    LineType = Values.CreateArcTypeMode.Value,
                    Color = Values.CreateArcColorMode.Value,
                    Sfx = "none",
                    IsTrace = true,
                    TimingGroup = Values.EditingTimingGroup.Value,
                };

                ArcTap arctap = new ArcTap()
                {
                    Timing = timing,
                    TimingGroup = Values.EditingTimingGroup.Value,
                    Arc = arc,
                };

                IEnumerable<ArcEvent> events = new ArcEvent[] { arctap, arc };
                var command = new EventCommand(
                    I18n.S("Compose.Notify.History.CreateNote.ArcTap"),
                    add: events);
                command.Execute();

                bool success;
                Vector2 pos;
                
                do
                {
                    (success, pos) = await Services.Cursor.RequestVerticalSelection(
                        confirm,
                        cancel,
                        showGridAtTiming: timing,
                        update: p =>
                        {
                            arc.XStart = p.x;
                            arc.XEnd = p.x;
                            arc.YStart = p.y;
                            arc.YEnd = p.y;
                            Services.Gameplay.Chart.UpdateEvents(events);
                        });
                    
                    if (!success)
                    {
                        command.Undo();
                        return;
                    }

                    if (!blockOverlap)
                    {
                        break;
                    }

                    if (HasOverlap(timing, arctap))
                    {
                        Services.Popups.Notify(
                            Popups.Severity.Warning, I18n.S("Compose.Notify.Creation.Overlap"));
                    }
                    else
                    {
                        break;
                    }
                }
                while (true);

                arc.XStart = pos.x;
                arc.XEnd = pos.x;
                arc.YStart = pos.y;
                arc.YEnd = pos.y;
                Services.Gameplay.Chart.UpdateEvents(events);
                Services.History.AddCommandWithoutExecuting(command);
            }
            else
            {
                toggleFreeSky.ForceDisabled = arcsSelected;

                Arc arc = new Arc()
                {
                    Timing = timing,
                    EndTiming = timing + 1,
                    LineType = Values.CreateArcTypeMode.Value,
                    Color = Values.CreateArcColorMode.Value,
                    Sfx = "none",
                    IsTrace = true,
                    TimingGroup = Values.EditingTimingGroup.Value,
                };

                ArcTap arctap = new ArcTap()
                {
                    Timing = timing,
                    TimingGroup = elligibleArcs[0].TimingGroup,
                    Arc = elligibleArcs[0],
                };

                IEnumerable<ArcEvent> events = new ArcEvent[] { arctap };
                var command = new EventCommand(
                    I18n.S("Compose.Notify.History.CreateNote.ArcTap"),
                    add: events);

                bool freeSky = false;
                using (new NoteModifyTarget(new List<Note> { arctap }))
                {
                    if (elligibleArcs.Count == 1 && arcsSelected)
                    {
                        Services.History.AddCommand(command);
                        return;
                    }

                    command.Execute();
                    bool success;
                    Vector2 pos;
                    do
                    {
                        (success, pos) = await Services.Cursor.RequestVerticalSelection(
                            confirm,
                            cancel,
                            showGridAtTiming: timing,
                            update: p =>
                            {
                                if (!arcsSelected && toggleFreeSky.WasExecuted)
                                {
                                    freeSky = !freeSky;
                                }

                                if (freeSky)
                                {
                                    arc.XStart = p.x;
                                    arc.XEnd = p.x;
                                    arc.YStart = p.y;
                                    arc.YEnd = p.y;
                                    arctap.Arc = arc;
                                }
                                else
                                {
                                    arctap.Arc = GetClosestArc(timing, elligibleArcs, p);
                                }

                                Services.Gameplay.Chart.UpdateEvents(events);
                            });

                        if (!success)
                        {
                            command.Undo();
                            return;
                        }

                        if (!blockOverlap)
                        {
                            break;
                        }

                        if (HasOverlap(timing, arctap))
                        {
                            Services.Popups.Notify(
                                Popups.Severity.Warning, I18n.S("Compose.Notify.Creation.Overlap"));
                        }
                        else
                        {
                            break;
                        }
                    }
                    while (true);

                    if (freeSky)
                    {
                        arc.XStart = pos.x;
                        arc.XEnd = pos.x;
                        arc.YStart = pos.y;
                        arc.YEnd = pos.y;
                        arctap.Arc = arc;

                        command.Undo();
                        events = new ArcEvent[] { arc, arctap };
                        command = new EventCommand(
                            I18n.S("Compose.Notify.History.CreateNote.ArcTap"),
                            add: events);
                        Services.History.AddCommand(command);
                    }
                    else
                    {
                        arctap.Arc = GetClosestArc(timing, elligibleArcs, pos);
                        Services.Gameplay.Chart.UpdateEvents(events);
                        Services.History.AddCommandWithoutExecuting(command);
                    }
                }
            }
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

        private void Awake()
        {
            Values.CreateNoteMode.OnValueChange += SwitchPreviewMode;
            Services.Selection.OnSelectionChange += OnSelectionChange;
        }

        private void OnDestroy()
        {
            Values.CreateNoteMode.OnValueChange -= SwitchPreviewMode;
            Services.Selection.OnSelectionChange -= OnSelectionChange;
        }

        private void SwitchPreviewMode(CreateNoteMode mode)
        {
            previewTap.gameObject.SetActive(mode == CreateNoteMode.Tap);
            previewHold.gameObject.SetActive(mode == CreateNoteMode.Hold);
            previewArc.gameObject.SetActive(mode == CreateNoteMode.Arc);
            previewArcTap.gameObject.SetActive(mode == CreateNoteMode.ArcTap);
            previewTrace.gameObject.SetActive(mode == CreateNoteMode.Trace);
        }

        private void OnSelectionChange(HashSet<Note> selection)
        {
            selectedArcs.Clear();
            foreach (var note in selection)
            {
                if (note is Arc a)
                {
                    selectedArcs.Add(a);
                }
            }
        }

        private bool HasOverlap(int timing, ArcTap creatingInstance)
        {
            Vector2 p = new Vector2(creatingInstance.WorldX, creatingInstance.WorldY);
            foreach (var note in Services.Gameplay.Chart.FindEventsWithinRange<Tap>(timing - 1, timing + 1, false))
            {
                if (note.TimingGroupInstance.GroupProperties.NoInput)
                {
                    continue;
                }

                Debug.Log(p + " " + new Vector2(ArcFormula.LaneToWorldX(note.Lane), 0));
                if (AreOverlapping(p, new Vector2(ArcFormula.LaneToWorldX(note.Lane), 0)))
                {
                    return true;
                }
            }

            foreach (var note in Services.Gameplay.Chart.FindEventsWithinRange<Hold>(timing - 1, timing + 1, false))
            {
                if (note.TimingGroupInstance.GroupProperties.NoInput)
                {
                    continue;
                }

                if (note.Timing >= timing - 1 && note.Timing <= timing + 1
                 && AreOverlapping(p, new Vector2(ArcFormula.LaneToWorldX(note.Lane), 0)))
                {
                    return true;
                }
            }

            foreach (var note in Services.Gameplay.Chart.FindEventsWithinRange<ArcTap>(timing - 1, timing + 1, false))
            {
                if (note.TimingGroupInstance.GroupProperties.NoInput || note == creatingInstance)
                {
                    continue;
                }

                if (AreOverlapping(new Vector2(note.WorldX, note.WorldY), p))
                {
                    return true;
                }
            }

            return false;

        }

        private bool HasOverlap(int timing, int lane)
        {
            foreach (var note in Services.Gameplay.Chart.FindEventsWithinRange<Tap>(timing - 1, timing + 1, false))
            {
                if (note.TimingGroupInstance.GroupProperties.NoInput)
                {
                    continue;
                }

                if (note.Lane == lane)
                {
                    return true;
                }
            }

            foreach (var note in Services.Gameplay.Chart.FindEventsWithinRange<Hold>(timing - 1, timing + 1, false))
            {
                if (note.TimingGroupInstance.GroupProperties.NoInput)
                {
                    continue;
                }

                if (note.Lane == lane && note.Timing >= timing - 1 && note.Timing <= timing + 1)
                {
                    return true;
                }
            }

            foreach (var note in Services.Gameplay.Chart.FindEventsWithinRange<ArcTap>(timing - 1, timing + 1, false))
            {
                if (note.TimingGroupInstance.GroupProperties.NoInput)
                {
                    continue;
                }

                if (AreOverlapping(new Vector2(note.WorldX, note.WorldY), new Vector2(ArcFormula.LaneToWorldX(lane), 0)))
                {
                    return true;
                }
            }

            return false;
        }

        private bool AreOverlapping(Vector2 v1, Vector2 v2)
        {
            return (v1 - v2).sqrMagnitude <= Values.TapOverlapWarningThreshold;
        }

        private void Update()
        {
            if (Values.CreateNoteMode.Value == CreateNoteMode.Idle)
            {
                return;
            }

            int cursorTiming = Services.Cursor.CursorTiming;
            int cursorLane = Services.Cursor.CursorLane;
            int tg = Values.EditingTimingGroup.Value;
            Vector3 cursorPosition = Services.Cursor.CursorWorldPosition;
            float z = cursorPosition.z;

            GroupProperties groupProperties = Services.Gameplay.Chart.GetTimingGroup(tg).GroupProperties;
            Vector3 pos = (groupProperties.FallDirection * z) + new Vector3(ArcFormula.LaneToWorldX(cursorLane), 0, 0);
            Quaternion rot = groupProperties.RotationIndividual;
            Vector3 scl = groupProperties.ScaleIndividual;

            switch (Values.CreateNoteMode.Value)
            {
                case CreateNoteMode.Tap:
                    scl.y = ArcFormula.CalculateTapSizeScalar(z) * scl.y;
                    previewTap.localPosition = pos;
                    previewTap.localRotation = rot;
                    previewTap.localScale = scl;
                    break;
                case CreateNoteMode.Hold:
                    scl.z *= 10;
                    previewHold.localPosition = pos;
                    previewHold.localRotation = rot;
                    previewHold.localScale = scl;
                    break;
                case CreateNoteMode.Arc:
                    pos = (groupProperties.FallDirection * z) + new Vector3(cursorPosition.x, 1, 0);
                    previewArc.localPosition = pos;
                    break;
                case CreateNoteMode.Trace:
                    pos = (groupProperties.FallDirection * z) + new Vector3(cursorPosition.x, 1, 0);
                    previewTrace.localPosition = pos;
                    break;
                case CreateNoteMode.ArcTap:
                    Arc parentArc = null;
                    bool found = false;
                    foreach (var arc in selectedArcs)
                    {
                        if (arc.Timing <= cursorTiming && cursorTiming <= arc.EndTiming)
                        {
                            if (parentArc != null)
                            {
                                parentArc = null;
                                break;
                            }
                            else
                            {
                                parentArc = arc;
                                found = true;
                            }
                        }
                    }

                    if (!found && parentArc == null)
                    {
                        foreach (var arc in Services.Gameplay.Chart.GetAll<Arc>())
                        {
                            if (arc.IsTrace && arc.Timing <= cursorTiming && cursorTiming <= arc.EndTiming)
                            {
                                if (parentArc != null)
                                {
                                    parentArc = null;
                                    break;
                                }
                                else
                                {
                                    parentArc = arc;
                                }
                            }
                        }
                    }

                    if (parentArc != null)
                    {
                        previewArcTap.gameObject.SetActive(true);
                        float worldX = parentArc.WorldXAt(cursorTiming);
                        float worldY = parentArc.WorldYAt(cursorTiming);
                        pos = (groupProperties.FallDirection * z) + new Vector3(worldX, worldY, 0);
                        previewArcTap.localPosition = pos;
                        previewArcTap.localRotation = rot;
                        previewArcTap.localScale = scl;
                    }
                    else
                    {
                        previewArcTap.gameObject.SetActive(false);
                    }

                    break;
            }
        }
    }
}