using System.Collections.Generic;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Navigation;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose.Editing
{
    [EditorScope("Mirror")]
    public class Mirror
    {
        [EditorAction("Horizontal", true, "m")]
        [KeybindHint(Priority = KeybindPriorities.Mirror)]
        [NoteModifyTarget.RequireTarget]
        public void MirrorHorizontal()
        {
            MirrorHorizontal(true);
        }

        [EditorAction("HorizontalNoColorSwitch", true, "<c-m>")]
        [KeybindHint(Exclude = true)]
        [NoteModifyTarget.RequireTarget]
        public void MirrorHorizontalNoColorSwitch()
        {
            MirrorHorizontal(false);
        }

        [EditorAction("Vertical", true, "<a-m>")]
        [KeybindHint(Exclude = true)]
        [NoteModifyTarget.RequireTarget]
        public void MirrorVertical()
        {
            IEnumerable<Note> notes = GetTargetNotes();
            List<(ArcEvent instance, ArcEvent newValue)> events = new List<(ArcEvent, ArcEvent)>();
            foreach (ArcEvent ev in notes)
            {
                if (ev is Arc a)
                {
                    Arc newArc = a.Clone() as Arc;
                    newArc.YStart = 1 - newArc.YStart;
                    newArc.YEnd = 1 - newArc.YEnd;
                    events.Add((a, newArc));
                }
            }

            NoteModifyTarget.MarkCurrentAsModified();
            NoteModifyTarget.ToggleCurrentVerticalModifiedState();
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
            IEnumerable<Note> notes = GetTargetNotes();
            List<(ArcEvent instance, ArcEvent newValue)> events = new List<(ArcEvent, ArcEvent)>();
            foreach (ArcEvent ev in notes)
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

            NoteModifyTarget.MarkCurrentAsModified();
            NoteModifyTarget.ToggleCurrentHorizontalModifiedState();
            if (events.Count > 0)
            {
                Services.History.AddCommand(new EventCommand(
                    name: I18n.S("Compose.Notify.History.Mirror.Horizontal"),
                    update: events));
            }
            else
            {
                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.Mirror.Actions.CannotMirror"));
            }
        }

        private IEnumerable<Note> GetTargetNotes() => NoteModifyTarget.Current ?? Services.Selection.SelectedNotes;
    }
}