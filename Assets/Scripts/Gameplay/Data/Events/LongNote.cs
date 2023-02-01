using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public abstract class LongNote : Note
    {
        private int totalCombo;

        public int EndTiming { get; set; }

        public double EndFloorPosition { get; set; }

        public override int TotalCombo => totalCombo;

        public int TimeIncrement { get; private set; }

        public int FirstJudgeTime { get; private set; }

        public int FinalJudgeTime { get; private set; }

        /// <summary>
        /// Recalculate the judge timings value of this note.
        /// </summary>
        public void RecalculateJudgeTimings()
        {
            totalCombo = 0;
            float bpm = TimingGroupInstance.GetBpm(Timing);

            if (bpm == 0)
            {
                FirstJudgeTime = int.MaxValue;
                FinalJudgeTime = int.MinValue;
                TimeIncrement = int.MaxValue;
                return;
            }

            int duration = EndTiming - Timing;
            bpm = Mathf.Abs(bpm);
            int rawIncrement = (int)Mathf.Abs((bpm >= 255 ? 60_000f : 30_000f) / bpm / Values.TimingPointDensity);
            TimeIncrement = Mathf.Max(1, rawIncrement);

            totalCombo = Mathf.Max(1, (duration / TimeIncrement) - 1);

            FirstJudgeTime = (totalCombo == 1) ? (Timing + (duration / 2)) : (Timing + TimeIncrement);
            FinalJudgeTime = FirstJudgeTime + ((totalCombo - 1) * TimeIncrement);
        }

        public override int ComboAt(int timing)
        {
            int combo = (timing - FirstJudgeTime) / TimeIncrement;
            return Mathf.Clamp(combo, 0, totalCombo);
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            LongNote e = newValues as LongNote;
            EndTiming = e.EndTiming;
        }

        protected float EndZPos(double floorPosition)
            => ArcFormula.FloorPositionToZ(EndFloorPosition - floorPosition);

        protected void ResetJudgeTimings()
        {
            RecalculateJudgeTimings();
        }
    }
}
