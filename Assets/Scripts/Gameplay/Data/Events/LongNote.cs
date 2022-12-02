using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public abstract class LongNote : Note
    {
        private int timeIncrement;
        private int firstJudgeTime;
        private int finalJudgeTime;
        private int currentJudgeTime;
        private int totalCombo;

        public int EndTiming { get; set; }

        public double EndFloorPosition { get; set; }

        public override int TotalCombo => totalCombo;

        /// <summary>
        /// Recalculate the judge timings value of this note.
        /// </summary>
        public void RecalculateJudgeTimings()
        {
            totalCombo = 0;
            float bpm = TimingGroupInstance.GetBpm(Timing);

            if (bpm == 0)
            {
                firstJudgeTime = int.MaxValue;
                finalJudgeTime = int.MinValue;
                currentJudgeTime = int.MaxValue;
                timeIncrement = int.MaxValue;
                return;
            }

            int duration = EndTiming - Timing;
            bpm = Mathf.Abs(bpm);
            int rawIncrement = (int)Mathf.Abs((bpm >= 255 ? 60_000f : 30_000f) / bpm);
            timeIncrement = Mathf.Max(1, rawIncrement);

            totalCombo = Mathf.Max(1, (duration / timeIncrement) - 1);

            firstJudgeTime = (totalCombo == 1) ? (Timing + (duration / 2)) : (Timing + timeIncrement);
            finalJudgeTime = firstJudgeTime + ((totalCombo - 1) * timeIncrement);
            currentJudgeTime = firstJudgeTime;
        }

        public override int ComboAt(int timing)
        {
            int combo = (timing - firstJudgeTime) / timeIncrement;
            return Mathf.Clamp(combo, 0, totalCombo);
        }

        public override int CompareTo(Note other)
        {
            LongNote note = other as LongNote;
            if (note.Timing == Timing)
            {
                return EndTiming.CompareTo(note.EndTiming);
            }

            return Timing.CompareTo(note.EndTiming);
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            LongNote e = newValues as LongNote;
            EndTiming = e.EndTiming;
        }

        protected float EndZPos(double floorPosition)
            => ArcFormula.FloorPositionToZ(EndFloorPosition - floorPosition);

        /// <summary>
        /// Update the current judge state to the new timing value.
        /// </summary>
        /// <param name="timing">The timing value.</param>
        /// <returns>Total judge points count that has elapsed.</returns>
        protected int UpdateCurrentJudgePointTiming(int timing)
        {
            if (currentJudgeTime > finalJudgeTime)
            {
                return 0;
            }

            timing = Mathf.Min(timing, finalJudgeTime);

            int count = (timing - currentJudgeTime) / timeIncrement;
            if (count >= 0)
            {
                currentJudgeTime += timeIncrement * (count + 1);
                return count + 1;
            }

            return 0;
        }

        protected void ResetJudgeTimings()
        {
            currentJudgeTime = firstJudgeTime;
        }
    }
}
