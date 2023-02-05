using System.Collections.Generic;
using ArcCreate.Compose.Navigation;
using ArcCreate.Gameplay.Data;
using UnityEngine;
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
        private readonly HashSet<Note> selectedNotes = new HashSet<Note>();
        private float latestSelectedDistance = 0;

        private readonly RaycastHit[] hitResults = new RaycastHit[32];

        [EditorAction("Single", false, "<mouse1>")]
        [RequireGameplayLoaded]
        public void SelectSingle()
        {
            if (TryGetNoteUnderCursor(out Note note, SelectionMode.Deselected))
            {
                ClearSelection();
                AddNoteToSelection(note);
            }
            else
            {
                ClearSelection();
            }
        }

        [EditorAction("Add", false, "<s-h-mouse2>")]
        [RequireGameplayLoaded]
        public void AddToSelection()
        {
            if (TryGetNoteUnderCursor(out Note note, SelectionMode.Deselected))
            {
                AddNoteToSelection(note);
            }
        }

        [EditorAction("Remove", false, "<a-h-mouse2>")]
        [RequireGameplayLoaded]
        public void RemoveFromSelection()
        {
            if (TryGetNoteUnderCursor(out Note note, SelectionMode.Selected))
            {
                RemoveNoteFromSelection(note);
            }
        }

        [EditorAction("Toggle", false, "<c-mouse1>")]
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
        }

        public void AddNoteToSelection(Note note)
        {
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
        }

        public void RemoveNoteFromSelection(IEnumerable<Note> notes)
        {
            foreach (var note in notes)
            {
                RemoveNoteFromSelection(note);
            }
        }

        private bool TryGetNoteUnderCursor(out Note note, SelectionMode selectionMode)
        {
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

                if (hit.transform.TryGetComponent<NoteBehaviour>(out var behaviour))
                {
                    note = behaviour.Note;
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

        private void Awake()
        {
            RequireSelectionAttribute.Selection = selectedNotes;
        }

        private class RequireSelectionAttribute : ContextRequirementAttribute
        {
            public static HashSet<Note> Selection { get; set; }

            public override bool CheckRequirement() => Selection.Count > 0;
        }
    }
}