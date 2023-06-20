using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Navigation;
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
        [SerializeField] private MarkerRange rangeSelectPreview;
        [SerializeField] private SelectMeshBuilder selectMeshBuilder;

        private readonly HashSet<Note> selectedNotes = new HashSet<Note>();
        private float latestSelectedDistance = 0;
        private bool rangeSelected;
        private readonly RaycastHit[] hitResults = new RaycastHit[32];
        private readonly HashSet<Note> search = new HashSet<Note>();

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
        [RequireGameplayLoaded]
        public async UniTask SelectSingle()
        {
            if (EventSystem.current.currentSelectedGameObject != null
             || !Services.Cursor.IsCursorAboveViewport
             || inspectorMenu.IsCursorHovering
             || RangeSelected)
            {
                return;
            }

            selectMeshBuilder.RefreshCollider();
            await UniTask.NextFrame();
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
        [RequireGameplayLoaded]
        public async UniTask AddToSelection()
        {
            selectMeshBuilder.RefreshCollider();
            await UniTask.NextFrame();

            if (TryGetNoteUnderCursor(out Note note, SelectionMode.Deselected))
            {
                AddNoteToSelection(note);
            }

            UpdateInspector();
            OnSelectionChange?.Invoke(selectedNotes);
        }

        [EditorAction("Remove", false, "<a-h-mouse2>")]
        [RequireGameplayLoaded]
        public async UniTask RemoveFromSelection()
        {
            selectMeshBuilder.RefreshCollider();
            await UniTask.NextFrame();

            if (TryGetNoteUnderCursor(out Note note, SelectionMode.Selected))
            {
                RemoveNoteFromSelection(note);
            }

            UpdateInspector();
            OnSelectionChange?.Invoke(selectedNotes);
        }

        [EditorAction("Toggle", false, "<c-mouse1>")]
        [RequireGameplayLoaded]
        public async UniTask ToggleNoteSelection()
        {
            selectMeshBuilder.RefreshCollider();
            await UniTask.NextFrame();

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
                    while (a != null)
                    {
                        search.Add(a);
                        a = a.NextArc;
                    }

                    a = arc.PreviousArc;
                    while (a != null)
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

            int amount = Physics.RaycastNonAlloc(ray, hitResults, 99999, gameplayLayer);

            if (TryGetNoteWithMinDistance(latestSelectedDistance, amount, out note, selectionMode))
            {
                return true;
            }
            else
            {
                return TryGetNoteWithMinDistance(0, amount, out note, selectionMode);
            }
        }

        private bool TryGetNoteWithMinDistance(float distance, int amount, out Note note, SelectionMode selectionMode)
        {
            for (int i = 0; i < Mathf.Min(hitResults.Length, amount); i++)
            {
                RaycastHit hit = hitResults[i];

                if (hit.distance <= distance)
                {
                    continue;
                }

                if (hit.transform.TryGetComponent<NoteCollider>(out var collider))
                {
                    note = collider.Note;
                    if (!note.TimingGroupInstance.GroupProperties.Editable)
                    {
                        continue;
                    }

                    if (note is Arc arc)
                    {
                        int timing = Services.Gameplay.Audio.ChartTiming;
                        float arcStartZ = arc.ZPos(arc.TimingGroupInstance.GetFloorPosition(timing));
                        if (hit.point.z * arcStartZ > 0 && arc.Timing < timing)
                        {
                            continue;
                        }
                    }

                    switch (selectionMode)
                    {
                        case SelectionMode.Any:
                            latestSelectedDistance = hit.distance;
                            return true;
                        case SelectionMode.Selected:
                            if (note.IsSelected)
                            {
                                latestSelectedDistance = hit.distance;
                                return true;
                            }

                            break;
                        case SelectionMode.Deselected:
                            if (!note.IsSelected)
                            {
                                latestSelectedDistance = hit.distance;
                                return true;
                            }

                            break;
                    }
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
    }
}