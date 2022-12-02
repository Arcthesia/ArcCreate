using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
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
        private int lastRenderRangeLower = int.MaxValue;
        private int lastRenderRangeUpper = int.MaxValue;

        public override int ResetJudgeToTiming(int timing)
        {
            return base.ResetJudgeToTiming(timing);
        }

        public override void Update(int timing, double floorPosition, GroupProperties groupProperties)
        {
            UpdateJudgement(timing, groupProperties);
            UpdateRender(timing, floorPosition, groupProperties);
        }

        public override void RebuildList()
        {
            timingSearch = new CachedBisect<Note, int>(Notes, note => note.Timing);
            floorPositionSearch = new CachedBisect<Note, double>(Notes, note => note.FloorPosition);
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
            double fpDist = System.Math.Abs(ArcFormula.ZToFloorPosition(Values.TrackLength));
            double renderFrom =
                (groupProperties.NoInput && !groupProperties.NoClip) ?
                floorPosition :
                floorPosition - fpDist;
            double renderTo = floorPosition + fpDist;

            int renderIndex = floorPositionSearch.Bisect(renderFrom);

            // Disable old notes
            for (int i = lastRenderRangeLower; i < lastRenderRangeUpper; i++)
            {
                Note note = floorPositionSearch.List[i];

                if (note.FloorPosition < renderFrom || note.FloorPosition > renderTo)
                {
                    Pool.Return(note.RevokeInstance());
                }
            }

            lastRenderRangeLower = renderIndex;

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
                    note.AssignInstance(Pool.Get());
                }

                note.UpdateInstance(timing, floorPosition, groupProperties);
                renderIndex++;
            }

            lastRenderRangeUpper = renderIndex;
        }
    }
}