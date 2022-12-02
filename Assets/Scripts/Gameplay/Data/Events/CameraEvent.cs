using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class CameraEvent : ArcEvent
    {
        public Vector3 Move { get; set; }

        public Vector3 Rotate { get; set; }

        public CameraType CameraType { get; set; }

        public int Duration { get; set; }

        public float Percent { get; set; }

        public override ArcEvent Clone()
        {
            return new CameraEvent()
            {
                Timing = Timing,
                Duration = Duration,
                CameraType = CameraType,
                Move = Move,
                Percent = Percent,
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

        public void Update(int timing)
        {
            if (timing > this.Timing + Duration)
            {
                Percent = 1;
                return;
            }
            else if (timing < this.Timing)
            {
                Percent = 0;
                return;
            }

            Percent = Mathf.Clamp(((1f * timing) - this.Timing) / Duration, 0, 1);
            switch (CameraType)
            {
                case CameraType.Qi:
                    Percent = ArcFormula.Qi(Percent);
                    break;
                case CameraType.Qo:
                    Percent = ArcFormula.Qo(Percent);
                    break;
                case CameraType.S:
                    Percent = ArcFormula.S(0, 1, Percent);
                    break;
            }
        }
    }
}
