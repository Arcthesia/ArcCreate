using System.Collections.Generic;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Selection;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;

namespace ArcCreate.Compose.Editing
{
    [EditorScope("Mirror")]
    public class Mirror
    {
        [EditorAction("Horizontal", true, "m")]
        [SelectionService.RequireSelection]
        public void MirrorHorizontal()
        {
            MirrorHorizontal(true);
        }

        [EditorAction("HorizontalNoColorSwitch", true, "<c-m>")]
        [SelectionService.RequireSelection]
        public void MirrorHorizontalNoColorSwitch()
        {
            MirrorHorizontal(false);
        }

        [EditorAction("Vertical", true, "<a-m>")]
        [SelectionService.RequireSelection]
        public void MirrorVertical()
        {
            HashSet<Note> selected = Services.Selection.SelectedNotes;
            List<(ArcEvent instance, ArcEvent newValue)> events = new List<(ArcEvent, ArcEvent)>();
            foreach (ArcEvent ev in selected)
            {
                if (ev is Arc a)
                {
                    Arc newArc = a.Clone() as Arc;
                    newArc.YStart = 1 - newArc.YStart;
                    newArc.YEnd = 1 - newArc.YEnd;
                    events.Add((a, newArc));
                }
            }

            if (events.Count > 0)
            {
                Services.History.AddCommand(new EventCommand(
                    name: I18n.S("Compose.Notify.History.Mirror.Vertical"),
                    update: events));
            }
            else
            {
                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.Mirror.CannotMirror"));
            }
        }

        private void MirrorHorizontal(bool switchColor)
        {
            HashSet<Note> selected = Services.Selection.SelectedNotes;
            List<(ArcEvent instance, ArcEvent newValue)> events = new List<(ArcEvent, ArcEvent)>();
            foreach (ArcEvent ev in selected)
            {
                switch (ev)
                {
                    case Tap t:
                        Tap newTap = t.Clone() as Tap;
                        newTap.Lane = 5 - newTap.Lane;
                        events.Add((t, newTap));
                        break;
                    case Hold h:
                        Hold newHold = h.Clone() as Hold;
                        newHold.Lane = 5 - newHold.Lane;
                        events.Add((h, newHold));
                        break;
                    case Arc a:
                        Arc newArc = a.Clone() as Arc;
                        newArc.XStart = 1 - newArc.XStart;
                        newArc.XEnd = 1 - newArc.XEnd;
                        if (switchColor)
                        {
                            newArc.Color = (newArc.Color == 0 || newArc.Color == 1) ? 1 - newArc.Color : newArc.Color;
                        }

                        events.Add((a, newArc));
                        break;
                }
            }

            if (events.Count > 0)
            {
                Services.History.AddCommand(new EventCommand(
                    name: I18n.S("Compose.Notify.History.Mirror.Horizontal"),
                    update: events));
            }
            else
            {
                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.Mirror.CannotMirror"));
            }
        }
    }
}