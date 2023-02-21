using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Selection;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose.Editing
{
    [EditorScope("Delete")]
    public class Delete
    {
        [EditorAction("Execute", true, "d", "<del>")]
        [SelectionService.RequireSelection]
        public void Execute()
        {
            List<ArcEvent> list = new List<ArcEvent>();
            foreach (var note in Services.Selection.SelectedNotes)
            {
                list.Add(note);
                if (note is Arc arc)
                {
                    var arctaps = Services.Gameplay.Chart.GetAll<ArcTap>()
                        .Where(at => at.Arc == arc);
                    foreach (var arctap in arctaps)
                    {
                        list.Add(arctap);
                    }
                }
            }

            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.History.DeleteNotes"),
                remove: list));
        }
    }
}