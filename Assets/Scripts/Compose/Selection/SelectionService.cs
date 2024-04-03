using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Popups;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Selection
{
    public enum SelectionMode
    {
        Any,
        Selected,
        Deselected,
    }

    [EditorScope("Selection")]
    public class SelectionService : MonoBehaviour, ISelectionService
    {
        [SerializeField] private LayerMask gameplayLayer;
        [SerializeField] private GameObject inspectorWindow;
        [SerializeField] private InspectorMenu inspectorMenu;
        [SerializeField] private TimingGroupPicker timingGroupPicker;
        [SerializeField] private MarkerRange rangeSelectPreview;

        private readonly HashSet<Note> selectedNotes = new HashSet<Note>();
        private bool rangeSelected;
        private readonly NoteRaycastHit[] hitResults = new NoteRaycastHit[32];
        private readonly HashSet<Note> search = new HashSet<Note>();
        private RaycastHitComparer hitComparer = new RaycastHitComparer();

        public event Action<HashSet<Note>> OnSelectionChange;

        public HashSet<Note> SelectedNotes => selectedNotes;

        private bool RangeSelected
        {
            get
            {
                bool res = rangeSelected;
                rangeSelected = false;
                return res;
            }
        }

        [EditorAction("Single", false, "<mouse1>")]
        [KeybindHint(Priority = KeybindPriorities.Selection + 4)]
        [RequireGameplayLoaded]
        public void SelectSingle()
        {
            if (EventSystem.current.currentSelectedGameObject != null
             || !Services.Cursor.IsCursorAboveViewport
             || inspectorMenu.IsCursorHovering
             || timingGroupPicker.IsCursorHovering
             || RangeSelected)
            {
                return;
            }

            if (TryGetNoteUnderCursor(out Note note, SelectionMode.Any))
            {
                ClearSelection();
                AddNoteToSelection(note);
            }
            else if (Values.CreateNoteMode.Value != CreateNoteMode.ArcTap || !Services.Cursor.IsHittingLane)
            {
                ClearSelection();
            }

            UpdateInspector();
            OnSelectionChange?.Invoke(selectedNotes);
        }

        [EditorAction("Add", false, "<s-h-mouse2>")]
        [KeybindHint(Priority = KeybindPriorities.Selection + 2)]
        [RequireGameplayLoaded]
        public void AddToSelection()
        {
            if (TryGetNoteUnderCursor(out Note note, SelectionMode.Deselected))
            {
                AddNoteToSelection(note);
            }

            UpdateInspector();
            OnSelectionChange?.Invoke(selectedNotes);
        }

        [EditorAction("Remove", false, "<a-h-mouse2>")]
        [KeybindHint(Priority = KeybindPriorities.Selection + 1)]
        [RequireGameplayLoaded]
        public void RemoveFromSelection()
        {
            if (TryGetNoteUnderCursor(out Note note, SelectionMode.Selected))
            {
                RemoveNoteFromSelection(note);
            }

            UpdateInspector();
            OnSelectionChange?.Invoke(selectedNotes);
        }

        [EditorAction("Toggle", false, "<c-mouse1>")]
        [KeybindHint(Priority = KeybindPriorities.Selection + 3)]
        [RequireGameplayLoaded]
        public void ToggleNoteSelection()
        {
            if (TryGetNoteUnderCursor(out Note note, SelectionMode.Any))
            {
                if (selectedNotes.Contains(note))
                {
                    RemoveNoteFromSelection(note);
                }
                else
                {
                    AddNoteToSelection(note);
                }
            }

            UpdateInspector();
            OnSelectionChange?.Invoke(selectedNotes);
        }

        [EditorAction("Clear", true)]
        [RequireGameplayLoaded]
        [RequireSelection]
        public void ClearSelection()
        {
            foreach (var note in selectedNotes)
            {
                note.IsSelected = false;
            }

            selectedNotes.Clear();
            rangeSelectPreview.gameObject.SetActive(false);
            UpdateInspector();
            OnSelectionChange?.Invoke(selectedNotes);
        }

        [EditorAction("ArcChain", false, "c")]
        [KeybindHint(Exclude = true)]
        [RequireGameplayLoaded]
        [RequireSelection]
        public void SelectArcChain()
        {
            search.Clear();
            foreach (Note note in selectedNotes)
            {
                if (note is Arc arc && !search.Contains(arc))
                {
                    search.Add(arc);
                    Arc a = arc.NextArc;
                    while (a != null && !search.Contains(a))
                    {
                        search.Add(a);
                        a = a.NextArc;
                    }

                    a = arc.PreviousArc;
                    while (a != null && !search.Contains(a))
                    {
                        search.Add(a);
                        a = a.PreviousArc;
                    }
                }
            }

            foreach (Note note in search)
            {
                AddNoteToSelection(note);
            }
        }

        [EditorAction("ToggleArcTapAndArc", false, "v")]
        [KeybindHint(Exclude = true)]
        [RequireSelection]
        public void SwitchSelectArcAndArcTap()
        {
            search.Clear();
            bool arcExistsInSelection = false;
            foreach (Note note in selectedNotes)
            {
                if (note is Arc arc)
                {
                    arcExistsInSelection = true;
                }
            }

            if (arcExistsInSelection)
            {
                foreach (Note note in selectedNotes)
                {
                    if (note is Arc arc)
                    {
                        var ats = Services.Gameplay.Chart.GetAll<ArcTap>().Where(at => at.Arc == arc);
                        foreach (ArcTap at in ats)
                        {
                            search.Add(at);
                        }
                    }
                }
            }
            else
            {
                foreach (Note note in selectedNotes)
                {
                    if (note is ArcTap arctap)
                    {
                        search.Add(arctap.Arc);
                    }
                }
            }

            SetSelection(search);
        }

        public void RemoveFromSelection(IEnumerable<ArcEvent> events)
        {
            bool changed = false;
            foreach (var ev in events)
            {
                if (ev is Note n && selectedNotes.Contains(n))
                {
                    selectedNotes.Remove(n);
                    n.IsSelected = false;
                    changed = true;
                }
            }

            if (changed)
            {
                UpdateInspector();
                OnSelectionChange?.Invoke(selectedNotes);
            }
        }

        [EditorAction("RangeSelect", true, "<c-r>")]
        [KeybindHint(Priority = KeybindPriorities.Selection)]
        [RequireGameplayLoaded]
        [SubAction("Confirm", false, "<mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [WhitelistScopes(typeof(Grid.GridService), typeof(Timeline.TimelineService), typeof(History.HistoryService), typeof(Cursor.CursorService))]
        public async UniTask RangeSelect(EditorAction action)
        {
            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Confirm");
            var (timing1Success, timing1) = await Services.Cursor.RequestTimingSelection(confirm, cancel);
            if (!timing1Success)
            {
                return;
            }

            var (timing2Success, timing2) = await Services.Cursor.RequestTimingSelection(confirm, cancel);
            if (!timing2Success)
            {
                return;
            }

            int from = Mathf.Min(timing1, timing2);
            int to = Mathf.Max(timing1, timing2);

            rangeSelectPreview.gameObject.SetActive(true);
            rangeSelectPreview.SetTiming(from, to);

            SelectNotesBetweenRange(from, to);
            rangeSelected = true;
            rangeSelectPreview.gameObject.SetActive(true);
        }

        public void AddNoteToSelection(Note note)
        {
            if (!note.TimingGroupInstance.GroupProperties.Editable)
            {
                return;
            }

            selectedNotes.Add(note);
            note.IsSelected = true;
        }

        public void AddNotesToSelection(IEnumerable<Note> notes)
        {
            foreach (var note in notes)
            {
                AddNoteToSelection(note);
            }
        }

        public void RemoveNoteFromSelection(Note note)
        {
            selectedNotes.Remove(note);
            note.IsSelected = false;
            if (selectedNotes.Count == 0)
            {
                rangeSelectPreview.gameObject.SetActive(false);
            }
        }

        public void RemoveNoteFromSelection(IEnumerable<Note> notes)
        {
            foreach (var note in notes)
            {
                RemoveNoteFromSelection(note);
            }
        }

        public void SetSelection(IEnumerable<Note> notes)
        {
            foreach (var note in selectedNotes)
            {
                var group = note.TimingGroupInstance;
                if (!group.IsVisible
                 || !group.GroupProperties.Editable)
                {
                    return;
                }

                note.IsSelected = false;
            }

            selectedNotes.Clear();
            AddNotesToSelection(notes);
            UpdateInspector();
            OnSelectionChange?.Invoke(selectedNotes);
        }

        private bool TryGetNoteUnderCursor(out Note note, SelectionMode selectionMode)
        {
            if (EventSystem.current.currentSelectedGameObject != null
             || (Settings.InputMode.Value != (int)InputMode.Auto
                && Settings.InputMode.Value != (int)InputMode.AutoController
                && Settings.InputMode.Value != (int)InputMode.Idle))
            {
                note = null;
                return false;
            }

            Camera gameplayCamera = Services.Gameplay.Camera.GameplayCamera;
            Vector2 mousePosition = Input.mousePosition;
            Ray ray = gameplayCamera.ScreenPointToRay(mousePosition);

            int amount = NoteRaycaster.Raycast(ray, hitResults, 99999);
            Array.Sort(hitResults, 0, amount, hitComparer);

            int length = Mathf.Min(hitResults.Length, amount);
            int initialOffset = 0;
            bool skipOne = false;

            if (selectionMode == SelectionMode.Any)
            {
                for (int i = 0; i < length; i++)
                {
                    NoteRaycastHit hit = hitResults[i];
                    if (SelectedNotes.Contains(hit.Note))
                    {
                        initialOffset = i + 1;
                        skipOne = true;
                    }
                }
            }

            int loopNum = skipOne ? length - 1 : length;
            for (int i = 0; i < loopNum; i++)
            {
                NoteRaycastHit hit = hitResults[(i + initialOffset) % length];

                if (hit.HitPoint.z >= Gameplay.Values.TrackLengthBackward
                 || hit.HitPoint.z <= -Gameplay.Values.TrackLengthForward)
                {
                    continue;
                }

                note = hit.Note;
                if (!note.TimingGroupInstance.GroupProperties.Editable)
                {
                    continue;
                }

                int timing = Services.Gameplay.Audio.ChartTiming;

                if (note is LongNote l)
                {
                    if (l.EndTiming < timing)
                    {
                        continue;
                    }

                    float startZ = l.ZPos(l.TimingGroupInstance.GetFloorPosition(timing));
                    if (hit.HitPoint.z * startZ > 0 && l.Timing < timing)
                    {
                        continue;
                    }
                }
                else if (note.Timing < timing)
                {
                    continue;
                }

                switch (selectionMode)
                {
                    case SelectionMode.Any:
                        return true;
                    case SelectionMode.Selected:
                        if (note.IsSelected)
                        {
                            return true;
                        }

                        break;
                    case SelectionMode.Deselected:
                        if (!note.IsSelected)
                        {
                            return true;
                        }

                        break;
                }
            }

            note = null;
            return false;
        }

        private void SelectNotesBetweenRange(int from, int to)
        {
            SetSelection(GetNotesBetweenRange(from, to));
            rangeSelected = true;
        }

        private IEnumerable<Note> GetNotesBetweenRange(int from, int to)
        {
            foreach (var tap in Services.Gameplay.Chart.GetAll<Tap>().Where(t => from <= t.Timing && t.Timing <= to))
            {
                yield return tap;
            }

            foreach (var hold in Services.Gameplay.Chart.GetAll<Hold>().Where(t => from <= t.Timing && t.EndTiming <= to))
            {
                yield return hold;
            }

            foreach (var arc in Services.Gameplay.Chart.GetAll<Arc>().Where(t => from <= t.Timing && t.EndTiming <= to))
            {
                yield return arc;
            }

            foreach (var arctap in Services.Gameplay.Chart.GetAll<ArcTap>().Where(t => from <= t.Timing && t.Timing <= to))
            {
                yield return arctap;
            }
        }

        private void UpdateInspector()
        {
            inspectorWindow.SetActive(selectedNotes.Count > 0);
            inspectorMenu.ApplySelection(selectedNotes);
        }

        private void Awake()
        {
            RequireSelectionAttribute.Selection = selectedNotes;
            Physics.queriesHitBackfaces = true;
            rangeSelectPreview.OnEndEdit += SelectNotesBetweenRange;
        }

        private void OnDestroy()
        {
            rangeSelectPreview.OnEndEdit -= SelectNotesBetweenRange;
        }

        public class RequireSelectionAttribute : ContextRequirementAttribute
        {
            public static HashSet<Note> Selection { get; set; }

            public override bool CheckRequirement() => Selection.Count > 0;
        }

        private class RaycastHitComparer : IComparer<NoteRaycastHit>
        {
            public int Compare(NoteRaycastHit x, NoteRaycastHit y)
            {
                return x.HitDistance.CompareTo(y.HitDistance);
            }
        }
    }
}