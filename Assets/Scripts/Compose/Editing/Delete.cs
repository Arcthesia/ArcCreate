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
        [KeybindHint(Priority = KeybindPriorities.Delete)]
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
                    list.AddRange(arctaps);
                }

                if (note is ArcTap arctap
                 && arctap.Arc != null
                 && arctap.Arc.Timing <= arctap.Arc.EndTiming - 1
                 && arctap.Arc.XStart == arctap.Arc.XEnd
                 && arctap.Arc.YStart == arctap.Arc.YEnd)
                {
                    list.Add(arctap.Arc);
                }
            }

            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.History.DeleteNotes"),
                remove: list));

            Services.Selection.RemoveFromSelection(list);
        }
    }
}