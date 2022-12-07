using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.RangeTree;
using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    /// <summary>
    /// Base class for arcs and holds note groups.
    /// </summary>
    /// <typeparam name="Note">The note type.</typeparam>
    /// <typeparam name="Behaviour">The MonoBehaviour component type corresponding to the note type.</typeparam>
    public abstract class LongNoteGroup<Note, Behaviour> : NoteGroup<Note, Behaviour>
        where Note : ILongNote<Behaviour>
        where Behaviour : MonoBehaviour
    {
        private readonly RangeTree<int, Note> timingTree = new RangeTree<int, Note>();
        private readonly RangeTree<double, Note> floorPositionTree = new RangeTree<double, Note>();
        private readonly List<Note> previousNotesInRange = new List<Note>();

        public override void Update(int timing, double floorPosition, GroupProperties groupProperties)
        {
            if (Notes.Count == 0)
            {
                return;
            }

            UpdateJudgement(timing, groupProperties);
            UpdateRender(timing, floorPosition, groupProperties);
        }

        public override int ComboAt(int timing)
        {
            var notes = timingTree[int.MinValue, timing];
            int combo = 0;

            while (notes.MoveNext())
            {
                var note = notes.Current;
                combo += note.ComboAt(timing);
            }

            return combo;
        }

        public override void RebuildList()
        {
            timingTree.Clear();
            floorPositionTree.Clear();

            for (int i = 0; i < Notes.Count; i++)
            {
                Note note = Notes[i];
                timingTree.Add(note.Timing, note.EndTiming, note);
                floorPositionTree.Add(note.FloorPosition, note.EndFloorPosition, note);
            }
        }

        private void UpdateJudgement(int timing, GroupProperties groupProperties)
        {
            if (groupProperties.NoInput)
            {
                return;
            }

            int judgeFrom = timing - Values.LostJudgeWindow;
            int judgeTo = timing + Values.HoldLostLateJudgeWindow;
            var notesInRange = timingTree[judgeFrom, judgeTo];

            while (notesInRange.MoveNext())
            {
                var note = notesInRange.Current;
                note.UpdateJudgement(timing, groupProperties);
            }
        }

        private void UpdateRender(int timing, double floorPosition, GroupProperties groupProperties)
        {
            double fpDistForward = System.Math.Abs(ArcFormula.ZToFloorPosition(Values.TrackLengthForward));
            double fpDistBackward = System.Math.Abs(ArcFormula.ZToFloorPosition(Values.TrackLengthBackward));
            double renderFrom =
                (groupProperties.NoInput && !groupProperties.NoClip) ?
                floorPosition :
                floorPosition - fpDistBackward;
            double renderTo = floorPosition + fpDistForward;

            var notesInRange = floorPositionTree[renderFrom, renderTo];

            // Disable old notes
            for (int i = 0; i < previousNotesInRange.Count; i++)
            {
                Note note = previousNotesInRange[i];
                if (note.FloorPosition < renderFrom || note.EndFloorPosition > renderTo)
                {
                    Pool.Return(note.RevokeInstance());
                }
            }

            previousNotesInRange.Clear();

            // Update notes
            while (notesInRange.MoveNext())
            {
                var note = notesInRange.Current;
                if (!note.IsAssignedInstance)
                {
                    note.AssignInstance(Pool.Get(ParentTransform));
                }

                note.UpdateInstance(timing, floorPosition, groupProperties);
                previousNotesInRange.Add(note);
            }
        }
    }
}