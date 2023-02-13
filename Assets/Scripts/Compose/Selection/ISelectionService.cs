using System.Collections.Generic;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose.Selection
{
    public interface ISelectionService
    {
        void SetSelection(IEnumerable<Note> notes);
    }
}