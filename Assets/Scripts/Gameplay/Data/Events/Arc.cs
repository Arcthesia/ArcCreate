using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class Arc : LongNote, ILongNote<ArcBehaviour>
    {
        public float XStart { get; set; }

        public float YStart { get; set; }

        public float XEnd { get; set; }

        public float YEnd { get; set; }

        public int Color { get; set; }

        public bool IsVoid { get; set; }

        public string Sfx { get; set; }

        public ArcLineType LineType { get; set; }

        public List<ArcTap> ArcTaps { get; set; } = new List<ArcTap>();

        public bool IsAssignedInstance => throw new System.NotImplementedException();

        public override ArcEvent Clone()
        {
            Arc arc = new Arc()
            {
                Timing = Timing,
                EndTiming = EndTiming,
                XStart = XStart,
                XEnd = XEnd,
                LineType = LineType,
                YStart = YStart,
                YEnd = YEnd,
                Color = Color,
                IsVoid = IsVoid,
                TimingGroup = TimingGroup,
            };
            foreach (var arctap in ArcTaps)
            {
                arc.ArcTaps.Add(arctap.Clone() as ArcTap);
            }

            return arc;
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            Arc n = newValues as Arc;
            XStart = n.XStart;
            XEnd = n.XEnd;
            LineType = n.LineType;
            YStart = n.YStart;
            YEnd = n.YEnd;
            Color = n.Color;
            IsVoid = n.IsVoid;
            TimingGroup = n.TimingGroup;
        }

        public float WorldXAt(int timing)
        {
            if (EndTiming == Timing)
            {
                return ArcFormula.ArcXToWorld(XStart);
            }

            float p = Mathf.Clamp((timing - Timing) / (EndTiming - Timing), 0, 1);
            return ArcFormula.ArcXToWorld(ArcFormula.X(XStart, XEnd, p, LineType));
        }

        public float WorldYAt(int timing)
        {
            if (EndTiming == Timing)
            {
                return ArcFormula.ArcYToWorld(XStart);
            }

            float p = Mathf.Clamp((timing - Timing) / (EndTiming - Timing), 0, 1);
            return ArcFormula.ArcYToWorld(ArcFormula.Y(YStart, YEnd, p, LineType));
        }

        public void AssignInstance(ArcBehaviour instance)
        {
            throw new System.NotImplementedException();
        }

        public ArcBehaviour RevokeInstance()
        {
            throw new System.NotImplementedException();
        }

        public void ResetJudge()
        {
            throw new System.NotImplementedException();
        }

        public void Rebuild()
        {
            throw new System.NotImplementedException();
        }

        public void ReloadSkin()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateJudgement(int timing, GroupProperties groupProperties)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateInstance(int timing, double floorPosition, GroupProperties groupProperties)
        {
            throw new System.NotImplementedException();
        }

        public int CompareTo(INote<ArcBehaviour> other)
        {
            throw new System.NotImplementedException();
        }
    }
}
