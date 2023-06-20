using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Navigation;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose.Editing
{
    public class NoteModifyTarget : IDisposable
    {
        private static readonly List<NoteModifyTarget> TargetStack = new List<NoteModifyTarget>();
        private readonly IEnumerable<Note> notes;
        private bool modified;

        public NoteModifyTarget(IEnumerable<Note> notes)
        {
            this.notes = notes;
            TargetStack.Add(this);
        }

        public static IEnumerable<Note> Current => TargetStack.LastOrDefault()?.notes;

        public bool WasModified
        {
            get
            {
                bool output = modified;
                modified = false;
                return output;
            }
        }

        public static void MarkCurrentAsModified()
        {
            TargetStack.LastOrDefault()?.MarkModified();
        }

        public void Dispose()
        {
            TargetStack.Remove(this);
        }

        private void MarkModified()
        {
            modified = true;
        }

        public class RequireTargetAttribute : ContextRequirementAttribute
        {
            public static HashSet<Note> Selection { get; set; }

            public override bool CheckRequirement()
                => (Current != null && Current.Any())
                || Services.Selection.SelectedNotes.Count > 0;
        }
    }
}