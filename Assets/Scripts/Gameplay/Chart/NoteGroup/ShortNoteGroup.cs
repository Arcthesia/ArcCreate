using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    /// <summary>
    /// Base class for taps and arc taps note groups.
    /// </summary>
    /// <typeparam name="Note">The note type.</typeparam>
    /// <typeparam name="Behaviour">The MonoBehaviour component type corresponding to the note type.</typeparam>
    public abstract class ShortNoteGroup<Note, Behaviour> : NoteGroup<Note, Behaviour>
        where Note : INote<Behaviour>
        where Behaviour : MonoBehaviour
    {
        private CachedBisect<Note, int> timingSearch;
        private CachedBisect<Note, double> floorPositionSearch;
        private readonly List<Note> previousNotesInRange = new List<Note>();

        public override int ComboAt(int timing)
        {
            return timingSearch.List.BisectRight(timing, note => note.Timing);
        }

        public override void Update(int timing, double floorPosition, GroupProperties groupProperties)
        {
            if (Notes.Count == 0)
            {
                return;
            }

            UpdateJudgement(timing, groupProperties);
            UpdateRender(timing, floorPosition, groupProperties);
        }

        public override void RebuildList()
        {
            timingSearch = new CachedBisect<Note, int>(Notes, note => note.Timing);
            floorPositionSearch = new CachedBisect<Note, double>(Notes, note => note.FloorPosition);
        }

        public override IEnumerable<Note> FindByTiming(int timing)
        {
            // Avoid modifying the cache of search tree.
            if (Notes.Count == 0)
            {
                yield break;
            }

            int i = timingSearch.List.BisectLeft(timing, n => n.Timing);

            while (i >= 0 && i < timingSearch.List.Count && timingSearch.List[i].Timing == timing)
            {
                yield return timingSearch.List[i];
                i++;
            }
        }

        public override IEnumerable<Note> FindEventsWithinRange(int from, int to)
        {
            if (Notes.Count == 0)
            {
                yield break;
            }

            // Avoid modifying the cache of search tree.
            int fromI = timingSearch.List.BisectLeft(from, n => n.Timing);
            int toI = timingSearch.List.BisectRight(to, n => n.Timing);

            for (int i = fromI; i <= toI; i++)
            {
                yield return timingSearch.List[i];
            }
        }

        private void UpdateJudgement(int timing, GroupProperties groupProperties)
        {
            if (groupProperties.NoInput)
            {
                return;
            }

            int judgeFrom = timing - Values.LostJudgeWindow;
            int judgeTo = timing + Values.LostJudgeWindow;
            int judgeIndex = timingSearch.Bisect(judgeFrom);
            while (judgeIndex < timingSearch.List.Count)
            {
                Note note = timingSearch.List[judgeIndex];
                if (note.Timing > judgeTo)
                {
                    break;
                }

                timingSearch.List[judgeIndex].UpdateJudgement(timing, groupProperties);
                judgeIndex++;
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

            int renderIndex = floorPositionSearch.Bisect(renderFrom);

            // Disable old notes
            for (int i = 0; i < previousNotesInRange.Count; i++)
            {
                Note note = previousNotesInRange[i];

                if (note.FloorPosition < renderFrom || note.FloorPosition > renderTo)
                {
                    Pool.Return(note.RevokeInstance());
                }
            }

            previousNotesInRange.Clear();

            // Update notes
            while (renderIndex < floorPositionSearch.List.Count)
            {
                Note note = floorPositionSearch.List[renderIndex];
                if (note.FloorPosition > renderTo)
                {
                    break;
                }

                if (!note.IsAssignedInstance)
                {
                    note.AssignInstance(Pool.Get(ParentTransform));
                }

                note.UpdateInstance(timing, floorPosition, groupProperties);
                renderIndex++;
                previousNotesInRange.Add(note);
            }
        }
    }
}