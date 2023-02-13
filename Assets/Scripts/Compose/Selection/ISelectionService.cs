using System.Collections.Generic;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose.Selection
{
    public interface ISelectionService
    {
        HashSet<Note> SelectedNotes { get; }

        void SetSelection(IEnumerable<Note> notes);
    }
}