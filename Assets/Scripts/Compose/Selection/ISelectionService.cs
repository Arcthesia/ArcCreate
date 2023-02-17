using System;
using System.Collections.Generic;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose.Selection
{
    public interface ISelectionService
    {
        event Action<HashSet<Note>> OnSelectionChange;

        HashSet<Note> SelectedNotes { get; }

        void SetSelection(IEnumerable<Note> notes);
    }
}