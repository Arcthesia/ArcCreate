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
    public abstract class LongNoteGroup<Note> : NoteGroup<Note>
        where Note : ILongNote
    {
        private readonly RangeTree<Note> timingTree = new RangeTree<Note>();
        private readonly RangeTree<Note> floorPositionTree = new RangeTree<Note>();
        private readonly List<Note> lastRenderingNotes = new List<Note>();

        protected RangeTree<Note> TimingTree => timingTree;

        protected RangeTree<Note> FloorPositionTree => floorPositionTree;

        protected List<Note> LastRenderingNotes => lastRenderingNotes;

        public override void UpdateJudgement(int timing, double floorPosition, GroupProperties groupProperties)
        {
            if (Notes.Count == 0 || groupProperties.NoInput)
            {
                return;
            }

            int judgeFrom = timing - Values.LostJudgeWindow;
            int judgeTo = timing + Values.HoldLostLateJudgeWindow;
            var notesInRange = timingTree[judgeFrom, judgeTo];

            int i = 0;
            while (notesInRange.MoveNext())
            {
                var note = notesInRange.Current;
                note.UpdateJudgement(timing, groupProperties);
                i++;
            }
        }

        public override void UpdateRender(int timing, double floorPosition, GroupProperties groupProperties)
        {
            lastRenderingNotes.Clear();
            if (Notes.Count == 0 || !groupProperties.Visible)
            {
                return;
            }

            double fpDistForward = System.Math.Abs(ArcFormula.ZToFloorPosition(Values.TrackLengthForward));
            double fpDistBackward = System.Math.Abs(ArcFormula.ZToFloorPosition(Values.TrackLengthBackward));
            double renderFrom =
                (groupProperties.NoInput && !groupProperties.NoClip) ?
                floorPosition :
                floorPosition - fpDistBackward;
            double renderTo = floorPosition + fpDistForward;

            var notesInRange = floorPositionTree[renderFrom, renderTo];

            // Update notes
            while (notesInRange.MoveNext())
            {
                var note = notesInRange.Current;
                lastRenderingNotes.Add(note);
                note.UpdateRender(timing, floorPosition, groupProperties);
            }
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
                timingTree.AddSilent(note.Timing, note.EndTiming, note);

                double fpStart = System.Math.Min(note.FloorPosition, note.EndFloorPosition);
                double fpEnd = System.Math.Max(note.FloorPosition, note.EndFloorPosition);
                floorPositionTree.AddSilent(fpStart, fpEnd, note);
            }

            timingTree.Rebuild();
            floorPositionTree.Rebuild();
        }

        public override void UpdateList()
        {
            for (int i = timingTree.Items.Count - 1; i >= 0; i--)
            {
                RangeValuePair<Note> pair = timingTree.Items[i];
                Note note = pair.Value;
                if (pair.From != note.Timing || pair.To != note.EndTiming)
                {
                    timingTree.RemoveAt(i);
                    timingTree.Add(note.Timing, note.EndTiming, note);
                }
            }

            for (int i = floorPositionTree.Items.Count - 1; i >= 0; i--)
            {
                RangeValuePair<Note> pair = floorPositionTree.Items[i];
                Note note = pair.Value;
                double fpStart = System.Math.Min(note.FloorPosition, note.EndFloorPosition);
                double fpEnd = System.Math.Max(note.FloorPosition, note.EndFloorPosition);
                if (pair.From != fpStart || pair.To != fpEnd)
                {
                    floorPositionTree.RemoveAt(i);
                    floorPositionTree.Add(fpStart, fpEnd, note);
                }
            }
        }

        public override IEnumerable<Note> FindByTiming(int from, int to)
        {
            var overlap = TimingTree[from, to];
            while (overlap.MoveNext())
            {
                Note note = overlap.Current;
                if (note.Timing >= from && note.Timing <= to)
                {
                    yield return note;
                }
            }
        }

        public IEnumerable<Note> FindByEndTiming(int from, int to)
        {
            var overlap = TimingTree[from, to];
            while (overlap.MoveNext())
            {
                Note note = overlap.Current;
                if (note.EndTiming >= from && note.EndTiming <= to)
                {
                    yield return note;
                }
            }
        }

        public override IEnumerable<Note> FindEventsWithinRange(int from, int to)
        {
            var overlap = TimingTree[from, to];
            while (overlap.MoveNext())
            {
                Note note = overlap.Current;
                if (note.Timing >= from && note.EndTiming <= to)
                {
                    yield return note;
                }
            }
        }

        public override IEnumerable<Note> GetRenderingNotes() => lastRenderingNotes;
    }
}