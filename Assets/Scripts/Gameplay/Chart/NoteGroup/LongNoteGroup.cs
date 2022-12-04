using System.Collections.Generic;
using System.Linq;
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
        private List<RangeValuePair<double, Note>> previousNotesInRange = new List<RangeValuePair<double, Note>>();

        public override void Update(int timing, double floorPosition, GroupProperties groupProperties)
        {
            UpdateJudgement(timing, groupProperties);
            UpdateRender(timing, floorPosition, groupProperties);
        }

        public override int ComboAt(int timing)
        {
            var notes = timingTree[int.MinValue, timing];
            int combo = 0;
            foreach (var note in notes)
            {
                combo += note.Value.ComboAt(timing);
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
            int judgeFrom = timing - Values.LostJudgeWindow;
            int judgeTo = timing + Values.LostJudgeWindow;
            var notesInRange = timingTree[judgeFrom, judgeTo];

            foreach (var pair in notesInRange)
            {
                pair.Value.UpdateJudgement(timing, groupProperties);
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

            List<RangeValuePair<double, Note>> notesInRange = floorPositionTree[renderFrom, renderTo].ToList();

            // Disable old notes
            for (int i = 0; i < previousNotesInRange.Count; i++)
            {
                RangeValuePair<double, Note> pair = previousNotesInRange[i];
                if (pair.From < renderFrom || pair.To > renderTo)
                {
                    Pool.Return(pair.Value.RevokeInstance());
                }
            }

            // Update notes
            for (int i = 0; i < notesInRange.Count; i++)
            {
                RangeValuePair<double, Note> pair = notesInRange[i];
                if (!pair.Value.IsAssignedInstance)
                {
                    pair.Value.AssignInstance(Pool.Get());
                }

                pair.Value.UpdateInstance(timing, floorPosition, groupProperties);
            }

            previousNotesInRange = notesInRange;
        }
    }
}