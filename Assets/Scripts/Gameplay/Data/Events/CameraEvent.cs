using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class CameraEvent : ArcEvent
    {
        public Vector3 Move { get; set; }

        public Vector3 Rotate { get; set; }

        public CameraType CameraType { get; set; }

        public int Duration { get; set; }

        public bool IsReset => CameraType == CameraType.Reset;

        public override ArcEvent Clone()
        {
            return new CameraEvent()
            {
                Timing = Timing,
                Duration = Duration,
                CameraType = CameraType,
                Move = Move,
                Rotate = Rotate,
                TimingGroup = TimingGroup,
            };
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            CameraEvent n = newValues as CameraEvent;
            Move = n.Move;
            Rotate = n.Rotate;
            CameraType = n.CameraType;
            Duration = n.Duration;
            TimingGroup = n.TimingGroup;
        }

        public int CompareTo(CameraEvent other)
        {
            if (Timing == other.Timing)
            {
                return Duration.CompareTo(other.Duration);
            }

            return Timing.CompareTo(other.Timing);
        }

        public float PercentAt(int timing)
        {
            if (timing > Timing + Duration)
            {
                return 1;
            }
            else if (timing < Timing)
            {
                return 0;
            }

            float p = Mathf.Clamp((float)(timing - Timing) / Duration, 0, 1);
            switch (CameraType)
            {
                case CameraType.Qi:
                    return ArcFormula.Qi(p);
                case CameraType.Qo:
                    return ArcFormula.Qo(p);
                default:
                    return p;
            }
        }
    }
}
