using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Selection;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

namespace ArcCreate.Compose.Editing
{
    [EditorScope("Clipboard")]
    public class Clipboard
    {
        private HashSet<Note> defaultClipboard = null;
        private readonly Dictionary<string, HashSet<Note>> namedClipboards = new Dictionary<string, HashSet<Note>>();

        [EditorAction("Copy", true, "<c-c>")]
        [SelectionService.RequireSelection]
        public void Copy()
        {
            defaultClipboard = MakeClipboard(Services.Selection.SelectedNotes);
            RequireClipboardAttribute.ClipboardAvailable = true;
            Services.Popups.Notify(
                Popups.Severity.Info,
                I18n.S("Compose.Notify.Clipboard.Copy", defaultClipboard.Count));
        }

        [EditorAction("Cut", true, "<c-x>")]
        [SelectionService.RequireSelection]
        public void Cut()
        {
            defaultClipboard = MakeClipboard(Services.Selection.SelectedNotes);
            RequireClipboardAttribute.ClipboardAvailable = true;

            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.History.Cut"),
                remove: defaultClipboard));

            Services.Popups.Notify(
                Popups.Severity.Info,
                I18n.S("Compose.Notify.Clipboard.Cut", defaultClipboard.Count));
        }

        [EditorAction("Paste", true, "<c-v>")]
        [RequireClipboard]
        [SubAction("Confirm", false, "<mouse1>")]
        [SubAction("Mirror", false, "m")]
        [SubAction("Cancel", true, "<esc>")]
        [WhitelistScopes(typeof(Grid.GridService), typeof(Timeline.TimelineService))]
        public async UniTask Paste(EditorAction action)
        {
            SubAction confirm = action.GetSubAction("Confirm");
            SubAction mirror = action.GetSubAction("Mirror");
            SubAction cancel = action.GetSubAction("Cancel");
            await StartPasting(defaultClipboard, confirm, mirror, cancel);
        }

        [EditorAction("Clear", true)]
        [RequireClipboard]
        public void ClearClipboard()
        {
            defaultClipboard = null;
            RequireClipboardAttribute.ClipboardAvailable = false;
        }

        [EditorAction("NamedCopy", false, "<c-a-c>")]
        [SelectionService.RequireSelection]
        public async UniTask NamedCopy()
        {
            string name = await GetKeyboardInput();
            var clipboard = MakeClipboard(Services.Selection.SelectedNotes);
            if (!namedClipboards.ContainsKey(name))
            {
                namedClipboards.Add(name, clipboard);
            }
            else
            {
                namedClipboards[name] = clipboard;
            }

            Services.Popups.Notify(
                Popups.Severity.Info,
                I18n.S("Compose.Notify.Clipboard.NamedCopy", clipboard.Count, name));
        }

        [EditorAction("NamedCut", false, "<c-a-x>")]
        [SelectionService.RequireSelection]
        public async UniTask NamedCut()
        {
            string name = await GetKeyboardInput();
            var clipboard = MakeClipboard(Services.Selection.SelectedNotes);
            if (!namedClipboards.ContainsKey(name))
            {
                namedClipboards.Add(name, clipboard);
            }
            else
            {
                namedClipboards[name] = clipboard;
            }

            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.History.Cut"),
                remove: defaultClipboard));

            Services.Popups.Notify(
                Popups.Severity.Info,
                I18n.S("Compose.Notify.Clipboard.NamedCut", clipboard.Count, name));
        }

        [EditorAction("NamedPaste", false, "<c-a-v>")]
        [SubAction("Confirm", false, "<mouse1>")]
        [SubAction("Mirror", false, "m")]
        [SubAction("Cancel", true, "<esc>")]
        [WhitelistScopes(typeof(Grid.GridService), typeof(Timeline.TimelineService))]
        public async UniTask NamedPaste(EditorAction action)
        {
            string name = await GetKeyboardInput();
            if (namedClipboards.TryGetValue(name, out var notes))
            {
                SubAction confirm = action.GetSubAction("Confirm");
                SubAction mirror = action.GetSubAction("Mirror");
                SubAction cancel = action.GetSubAction("Cancel");
                await StartPasting(notes, confirm, mirror, cancel);
            }
        }

        private async UniTask StartPasting(HashSet<Note> notes, SubAction confirm, SubAction mirror, SubAction cancel)
        {
            List<Note> newNotes = new List<Note>(MakePaste(notes));
            EventCommand command = new EventCommand(
                name: I18n.S("Compose.Notify.History.Paste"),
                add: newNotes);

            int minTiming = int.MaxValue;
            Note anchorNote = newNotes[0];
            for (int i = 0; i < newNotes.Count; i++)
            {
                Note note = newNotes[i];
                if (note.Timing < minTiming)
                {
                    minTiming = note.Timing;
                    anchorNote = note;
                }
            }

            command.Execute();
            Services.Gameplay.Chart.EnableColliderGeneration = false;
            Services.Gameplay.Chart.EnableArcRebuildSegment = false;

            var (success, timing) = await Services.Cursor.RequestTimingSelection(
                confirm,
                cancel,
                update: t =>
                {
                    bool mirrored = mirror.WasExecuted;
                    ApplyTimingToPastingNotes(t, newNotes, anchorNote);
                    if (mirrored)
                    {
                        Services.Gameplay.Chart.EnableArcRebuildSegment = true;
                        Mirror.MirrorHorizontal(newNotes, true);
                    }

                    Services.Gameplay.Chart.UpdateEvents(newNotes);

                    if (mirrored)
                    {
                        Services.Gameplay.Chart.EnableArcRebuildSegment = false;
                    }
                });

            if (success)
            {
                Services.Gameplay.Chart.EnableColliderGeneration = true;
                Services.Gameplay.Chart.EnableArcRebuildSegment = true;
                ApplyTimingToPastingNotes(timing, newNotes, anchorNote);
                Services.Gameplay.Chart.UpdateEvents(newNotes);

                List<Arc> extraArc = new List<Arc>();
                foreach (var note in newNotes)
                {
                    if (note is ArcTap at && (at.Timing < at.Arc.Timing || at.Timing > at.Arc.EndTiming))
                    {
                        Arc newArc = at.Arc.Clone() as Arc;
                        at.Arc = newArc;
                        newArc.Timing = newArc.Timing - minTiming + timing;
                        newArc.EndTiming = newArc.EndTiming - minTiming + timing;
                        extraArc.Add(newArc);
                    }
                }

                if (extraArc.Count > 0)
                {
                    command.Undo();
                    newNotes.AddRange(extraArc);
                    command = new EventCommand(
                        name: I18n.S("Compose.Notify.History.Paste"),
                        add: newNotes);

                    command.Execute();
                }

                Services.History.AddCommandWithoutExecuting(command);
            }
            else
            {
                Services.Gameplay.Chart.EnableColliderGeneration = true;
                Services.Gameplay.Chart.EnableArcRebuildSegment = true;
                command.Undo();
            }
        }

        private async UniTask<string> GetKeyboardInput()
        {
            string res = string.Empty;
            bool typed = false;
            void OnType(char c)
            {
                res += c;
                typed = true;
            }

            Keyboard.current.onTextInput += OnType;
            while (!typed)
            {
                await UniTask.NextFrame();
            }

            Keyboard.current.onTextInput -= OnType;
            return res;
        }

        private void ApplyTimingToPastingNotes(int timing, List<Note> notes, Note anchorNote)
        {
            int anchor = anchorNote.Timing;
            for (int i = 0; i < notes.Count; i++)
            {
                Note note = notes[i];
                note.Timing = note.Timing - anchor + timing;

                if (note is LongNote l)
                {
                    l.EndTiming = l.EndTiming - anchor + timing;
                }
            }
        }

        private HashSet<Note> MakeClipboard(HashSet<Note> notes)
        {
            HashSet<Note> clipboard = new HashSet<Note>();
            foreach (var note in notes)
            {
                if (note is ArcTap arcTap)
                {
                    if (!notes.Contains(arcTap.Arc))
                    {
                        clipboard.Add(arcTap.Clone() as Note);
                    }
                }
                else if (note is Arc arc)
                {
                    Arc clone = arc.Clone() as Arc;
                    var arctaps = Services.Gameplay.Chart.GetAll<ArcTap>().Where(at => at.Arc == arc);
                    foreach (var arctap in arctaps)
                    {
                        ArcTap cloneArctap = arctap.Clone() as ArcTap;
                        cloneArctap.Arc = clone;
                        clipboard.Add(cloneArctap);
                    }

                    clipboard.Add(clone);
                }
                else
                {
                    clipboard.Add(note.Clone() as Note);
                }
            }

            return clipboard;
        }

        private HashSet<Note> MakePaste(HashSet<Note> notes)
        {
            HashSet<Note> paste = new HashSet<Note>();
            foreach (var note in notes)
            {
                if (note is Arc arc)
                {
                    Arc clone = arc.Clone() as Arc;
                    var arctaps = notes.Where(n => n is ArcTap at && at.Arc == arc);
                    foreach (var arctap in arctaps)
                    {
                        ArcTap cloneArctap = arctap.Clone() as ArcTap;
                        cloneArctap.Arc = clone;
                        paste.Add(cloneArctap);
                    }

                    paste.Add(clone);
                }
                else if (note is ArcTap arcTap)
                {
                    if (!notes.Contains(arcTap.Arc))
                    {
                        paste.Add(arcTap.Clone() as Note);
                    }
                }
                else
                {
                    paste.Add(note.Clone() as Note);
                }
            }

            return paste;
        }

        private class RequireClipboardAttribute : ContextRequirementAttribute
        {
            public static bool ClipboardAvailable { get; set; }

            public override bool CheckRequirement() => ClipboardAvailable;
        }
    }
}