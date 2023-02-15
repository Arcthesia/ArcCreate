using System.Collections.Generic;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Selection;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

namespace ArcCreate.Compose.Editing
{
    [EditorScope("NoteActions")]
    public class NoteActions
    {
        private HashSet<Note> defaultClipboard = null;
        private readonly Dictionary<string, HashSet<Note>> namedClipboards = new Dictionary<string, HashSet<Note>>();

        [EditorAction("Delete", true, "d", "<del>")]
        [SelectionService.RequireSelection]
        public void Delete()
        {
            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.History.DeleteNotes"),
                remove: Services.Selection.SelectedNotes));
        }

        [EditorAction("Copy", true, "<c-c>")]
        [SelectionService.RequireSelection]
        public void Copy()
        {
            defaultClipboard = new HashSet<Note>(Services.Selection.SelectedNotes);
            RequireClipboardAttribute.ClipboardAvailable = true;
            Services.Popups.Notify(
                Popups.Severity.Info,
                I18n.S("Compose.Notify.Clipboard.Copy", defaultClipboard.Count));
        }

        [EditorAction("Cut", true, "<c-x>")]
        [SelectionService.RequireSelection]
        public void Cut()
        {
            defaultClipboard = new HashSet<Note>(Services.Selection.SelectedNotes);
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
        [SubAction("Cancel", true, "<esc>")]
        [WhitelistScopes(typeof(Grid.GridService), typeof(Timeline.TimelineService))]
        public async UniTask Paste(EditorAction action)
        {
            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Cancel");
            await StartPasting(defaultClipboard, confirm, cancel);
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
            var notes = new HashSet<Note>(Services.Selection.SelectedNotes);
            if (!namedClipboards.ContainsKey(name))
            {
                namedClipboards.Add(name, notes);
            }
            else
            {
                namedClipboards[name] = notes;
            }

            Services.Popups.Notify(
                Popups.Severity.Info,
                I18n.S("Compose.Notify.Clipboard.NamedCopy", notes.Count, name));
        }

        [EditorAction("NamedCut", false, "<c-a-x>")]
        [SelectionService.RequireSelection]
        public async UniTask NamedCut()
        {
            string name = await GetKeyboardInput();
            var notes = new HashSet<Note>(Services.Selection.SelectedNotes);
            if (!namedClipboards.ContainsKey(name))
            {
                namedClipboards.Add(name, notes);
            }
            else
            {
                namedClipboards[name] = notes;
            }

            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.History.Cut"),
                remove: defaultClipboard));

            Services.Popups.Notify(
                Popups.Severity.Info,
                I18n.S("Compose.Notify.Clipboard.NamedCut", notes.Count, name));
        }

        [EditorAction("NamedPaste", false, "<c-a-v>")]
        [SubAction("Confirm", false, "<mouse1>")]
        [SubAction("Cancel", true, "<esc>")]
        [WhitelistScopes(typeof(Grid.GridService), typeof(Timeline.TimelineService))]
        public async UniTask NamedPaste(EditorAction action)
        {
            string name = await GetKeyboardInput();
            if (namedClipboards.TryGetValue(name, out var notes))
            {
                SubAction confirm = action.GetSubAction("Confirm");
                SubAction cancel = action.GetSubAction("Cancel");
                await StartPasting(notes, confirm, cancel);
            }
        }

        private async UniTask StartPasting(HashSet<Note> notes, SubAction confirm, SubAction cancel)
        {
            List<Note> newNotes = new List<Note>(notes.Count);
            foreach (var note in notes)
            {
                newNotes.Add(note.Clone() as Note);
            }

            EventCommand command = new EventCommand(
                name: I18n.S("Compose.Notify.History.Paste"),
                add: newNotes);

            int minTiming = int.MaxValue;
            Note anchorNote = newNotes[0];
            int arcCount = 0;
            for (int i = 0; i < newNotes.Count; i++)
            {
                Note note = newNotes[i];
                if (note.Timing < minTiming)
                {
                    minTiming = note.Timing;
                    anchorNote = note;
                }

                if (note is Arc)
                {
                    arcCount += 1;
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
                    ApplyTimingToPastingNotes(t, newNotes, anchorNote);
                    Services.Gameplay.Chart.UpdateEvents(newNotes);
                });

            if (success)
            {
                Services.Gameplay.Chart.EnableColliderGeneration = true;
                Services.Gameplay.Chart.EnableArcRebuildSegment = true;
                ApplyTimingToPastingNotes(timing, newNotes, anchorNote);
                Services.Gameplay.Chart.UpdateEvents(newNotes);
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

        private class RequireClipboardAttribute : ContextRequirementAttribute
        {
            public static bool ClipboardAvailable { get; set; }

            public override bool CheckRequirement() => ClipboardAvailable;
        }
    }
}