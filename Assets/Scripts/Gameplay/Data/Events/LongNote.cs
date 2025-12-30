using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public abstract class LongNote : Note
    {
        public int EndTiming { get; set; }

        public double EndFloorPosition { get; set; }

        public double TimeIncrement { get; protected set; }

        public double FirstJudgeTime { get; protected set; }

        /// <summary>
        /// Recalculate the judge timings value of this note.
        /// </summary>
        public abstract void RecalculateJudgeTimings();

        public override int ComboAt(int timing)
        {
            if (timing < FirstJudgeTime)
            {
                return 0;
            }

            int combo = (int)((timing - FirstJudgeTime) / TimeIncrement) + 1;
            return Mathf.Clamp(combo, 0, TotalCombo);
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            LongNote e = newValues as LongNote;
            EndTiming = e.EndTiming;
        }

        public override void RecalculateFloorPosition()
        {
            base.RecalculateFloorPosition();
            EndFloorPosition = TimingGroupInstance.GetFloorPosition(EndTiming);
        }

        public float EndZPos(double floorPosition)
            => ArcFormula.FloorPositionToZ(EndFloorPosition - floorPosition, TimingGroup);
    }
}
