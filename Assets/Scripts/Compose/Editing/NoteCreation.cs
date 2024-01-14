using System.Collections.Generic;
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
        [SubAction("Confirm", false, "<mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [KeybindHint(Priority = KeybindPriorities.NoteCreation)]
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
            int timing1 = Services.Cursor.CursorTiming;
            Hold hold = new Hold()
            {
                Timing = timing1,
                EndTiming = timing1 + 1,
                Lane = Services.Cursor.CursorLane,
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
            var command = new EventCommand(
                I18n.S("Compose.Notify.History.CreateNote.ArcTap"),
                add: events);

            using (new NoteModifyTarget(new List<Note> { arctap }))
            {
                if (elligibleArcs.Count > 1)
                {
                    command.Execute();

                    var (success, pos) = await Services.Cursor.RequestVerticalSelection(
                        confirm,
                        cancel,
                        showGridAtTiming: timing,
                        update: p =>
                        {
                            arctap.Arc = GetClosestArc(timing, elligibleArcs, p);
                            Services.Gameplay.Chart.UpdateEvents(events);
                        });

                    if (success)
                    {
                        arctap.Arc = GetClosestArc(timing, elligibleArcs, pos);
                        Services.Gameplay.Chart.UpdateEvents(events);
                        Services.History.AddCommandWithoutExecuting(command);
                    }
                    else
                    {
                        command.Undo();
                    }

                    return;
                }
            }

            Services.History.AddCommand(command);
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