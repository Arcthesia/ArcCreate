using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

        [EditorAction("Single", false, "<u-mouse1>")]
        [RequireGameplayLoaded]
        public async UniTask SelectSingle()
        {
            selectMeshBuilder.RefreshCollider();
            await UniTask.NextFrame();

            if (EventSystem.current.currentSelectedGameObject != null
             || !Services.Cursor.IsCursorAboveViewport
             || inspectorMenu.IsCursorHovering
             || RangeSelected)
            {
                return;
            }

            if (TryGetNoteUnderCursor(out Note note, SelectionMode.Any))
            {
                ClearSelection();
                AddNoteToSelection(note);
            }
            else
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

        public bool TrySelectNoteBlockNoteCreation()
        {
            if (EventSystem.current.currentSelectedGameObject != null
             || !Services.Cursor.IsCursorAboveViewport
             || RangeSelected)
            {
                return false;
            }

            if (TryGetNoteUnderCursor(out Note note, SelectionMode.Any))
            {
                ClearSelection();
                AddNoteToSelection(note);
                UpdateInspector();
                OnSelectionChange?.Invoke(selectedNotes);
                return true;
            }

            return false;
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
             || (Settings.InputMode.Value != (int)InputMode.Auto && Settings.InputMode.Value != (int)InputMode.AutoController))
            {
                note = null;
                return false;
            }

            Camera gameplayCamera = Services.Gameplay.Camera.GameplayCamera;
            Vector2 mousePosition = Mouse.current.position.ReadValue();
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
            RequireNoSelectionAttribute.Selection = selectedNotes;
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

        internal class RequireNoSelectionAttribute : ContextRequirementAttribute
        {
            public static HashSet<Note> Selection { get; set; }

            public override bool CheckRequirement() => Selection.Count == 0;
        }
    }
}