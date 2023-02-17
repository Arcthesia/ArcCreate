using System;
using System.Collections.Generic;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose.Selection
{
    public interface ISelectionService
    {
        /// <summary>
        /// Invoked whenever there's a change to the selection.
        /// </summary>
        event Action<HashSet<Note>> OnSelectionChange;

        /// <summary>
        /// Gets the set of selected notes.
        /// </summary>
        HashSet<Note> SelectedNotes { get; }

        /// <summary>
        /// Clear the old selection and set the new note selection.
        /// </summary>
        /// <param name="notes">The note to select.</param>
        void SetSelection(IEnumerable<Note> notes);

        /// <summary>
        /// don't question it.
        /// </summary>
        /// <returns>Whether or not to block note creation.</returns>
        bool TrySelectNoteBlockNoteCreation();
    }
}