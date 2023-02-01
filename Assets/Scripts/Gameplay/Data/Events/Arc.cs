using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class Arc : LongNote, ILongNote<ArcBehaviour>, IArcJudgementReceiver
    {
        private ArcBehaviour instance;
        private bool highlight = false;
        private bool judgementRequestSent = false;
        private bool highlightRequestSent = false;
        private bool hasBeenHitOnce = false;
        private int flashCount = 0;
        private float arcGroupAlpha = 1;
        private int longParticleUntil = int.MinValue;
        private Arc firstArcOfGroup;

        // Avoid infinite recursion
        private bool recursivelyCalled = false;

        public float XStart { get; set; }

        public float YStart { get; set; }

        public float XEnd { get; set; }

        public float YEnd { get; set; }

        public int Color { get; set; }

        public bool IsTrace { get; set; }

        public string Sfx { get; set; }

        public ArcLineType LineType { get; set; }

        public List<ArcTap> ArcTaps { get; set; } = new List<ArcTap>();

        public HashSet<Arc> NextArcs { get; } = new HashSet<Arc>();

        public HashSet<Arc> PreviousArcs { get; } = new HashSet<Arc>();

        public bool IsAssignedInstance => instance != null;

        public float SegmentLength => ArcFormula.CalculateArcSegmentLength(EndTiming - Timing);

        public bool IsFirstArcOfGroup => PreviousArcs.Count == 0;

        public bool ShouldDrawHeightIndicator => !IsTrace && (YStart != YEnd || IsFirstArcOfGroup);

        public float ArcCapSize => IsTrace ? Values.TraceCapSize : Values.ArcCapSize;

        public bool Highlight
        {
            get => highlight;
            set
            {
                highlight = value;
                if (instance != null)
                {
                    instance.Highlight = value;
                }
            }
        }

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
                IsTrace = IsTrace,
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
            IsTrace = n.IsTrace;
            TimingGroup = n.TimingGroup;
        }

        public void AssignInstance(ArcBehaviour instance)
        {
            this.instance = instance;
            instance.SetData(this);
            ReloadSkin();
            RebuildSegments();
        }

        public ArcBehaviour RevokeInstance()
        {
            var result = instance;
            instance = null;
            return result;
        }

        public void ResetJudge()
        {
            ResetJudgeTimings();
            Highlight = false;
            longParticleUntil = int.MinValue;
            judgementRequestSent = false;
            highlightRequestSent = false;
            arcGroupAlpha = 1;
        }

        public void Rebuild()
        {
            if (IsFirstArcOfGroup)
            {
                SetGroupFirst(this);
            }

            RecalculateJudgeTimings();
            RebuildSegments();
        }

        public void ReloadSkin()
        {
            var (normal, highlight, arcCap, heightIndicatorColor) = Services.Skin.GetArcSkin(this);
            instance.SetSkin(normal, highlight, arcCap, heightIndicatorColor);
        }

        public void UpdateJudgement(int currentTiming, GroupProperties groupProperties)
        {
            if (!IsTrace && currentTiming >= Timing && Timing < EndTiming && !judgementRequestSent)
            {
                RequestJudgement(currentTiming);
                judgementRequestSent = true;
            }

            if (!IsTrace && currentTiming >= Timing && Timing < EndTiming && !highlightRequestSent)
            {
                RequestHighlight(currentTiming);
                highlightRequestSent = true;
            }
        }

        public void UpdateInstance(int currentTiming, double currentFloorPosition, GroupProperties groupProperties)
        {
            if (instance == null)
            {
                return;
            }

            float z = ZPos(currentFloorPosition);

            Vector3 pos = (groupProperties.FallDirection * z) + new Vector3(WorldXAt(Timing), WorldYAt(Timing), 0);
            Quaternion rot = groupProperties.RotationIndividual;
            Vector3 scl = groupProperties.ScaleIndividual;
            instance.SetTransform(pos, rot, scl);
            instance.UpdateSegmentsPosition(currentFloorPosition, groupProperties.FallDirection);

            float alpha = 1;
            if (!IsTrace)
            {
                if (Highlight)
                {
                    flashCount = (flashCount + 1) % Values.ArcFlashCycle;
                    if (flashCount == 0)
                    {
                        alpha = Values.FlashArcAlphaScalar;
                    }

                    if (currentTiming <= EndTiming)
                    {
                        Services.Camera.AddTiltToCamera(WorldXAt(currentTiming));
                    }
                }
                else
                {
                    if (currentTiming >= firstArcOfGroup.Timing)
                    {
                        alpha = Values.MissedArcAlphaScalar;
                    }
                }

                alpha *= Values.MaxArcAlpha;
            }
            else
            {
                alpha = EndTiming - Timing <= 1 ? Values.MaxArcAlpha / 2 : Values.MaxArcAlpha;
            }

            Color color = groupProperties.Color;
            color.a *= Mathf.Min(alpha, arcGroupAlpha);
            instance.SetColor(color);

            if (hasBeenHitOnce || IsTrace)
            {
                instance.ClipTo(currentTiming, currentFloorPosition, currentTiming, currentFloorPosition);
            }
            else
            {
                instance.ClipTo(currentTiming, currentFloorPosition, Timing, FloorPosition);
            }

            if (currentTiming <= longParticleUntil && currentTiming >= Timing && currentTiming <= EndTiming)
            {
                Services.Particle.PlayLongParticle(
                    this,
                    new Vector3(WorldXAt(currentTiming), WorldYAt(currentTiming), 0));
            }
        }

        public void ProcessArcJudgement(bool isExpired, bool isJudgement)
        {
            int currentTiming = Services.Audio.ChartTiming;

            if (!isJudgement)
            {
                RequestHighlight(currentTiming);
            }

            float x = WorldXAt(currentTiming);
            float y = WorldYAt(currentTiming);
            Vector3 currentPos = new Vector3(x, y);

            if (isExpired)
            {
                SetGroupHighlight(false, int.MinValue);

                if (isJudgement)
                {
                    Services.Score.ProcessJudgement(JudgementResult.LostLate);
                    Services.Particle.PlayTextParticle(currentPos, JudgementResult.LostLate);
                }
            }
            else if (currentTiming <= EndTiming)
            {
                SetGroupHighlight(true, currentTiming + Values.HoldParticlePersistDuration);
                hasBeenHitOnce = true;

                if (isJudgement)
                {
                    Services.Score.ProcessJudgement(JudgementResult.Max);
                    Services.Particle.PlayTextParticle(currentPos, JudgementResult.Max);
                }
            }
        }

        public int CompareTo(INote<ArcBehaviour> other)
        {
            LongNote note = other as LongNote;
            if (note.Timing == Timing)
            {
                return EndTiming.CompareTo(note.EndTiming);
            }

            return Timing.CompareTo(note.EndTiming);
        }

        public float WorldXAt(int timing)
        {
            if (EndTiming == Timing)
            {
                return ArcFormula.ArcXToWorld(XStart);
            }

            float p = Mathf.Clamp((float)(timing - Timing) / (EndTiming - Timing), 0, 1);
            return ArcFormula.ArcXToWorld(ArcFormula.X(XStart, XEnd, p, LineType));
        }

        public float WorldYAt(int timing)
        {
            if (EndTiming == Timing)
            {
                return ArcFormula.ArcYToWorld(YStart);
            }

            float p = Mathf.Clamp((float)(timing - Timing) / (EndTiming - Timing), 0, 1);
            return ArcFormula.ArcYToWorld(ArcFormula.Y(YStart, YEnd, p, LineType));
        }

        private void RebuildSegments()
        {
            if (instance == null)
            {
                return;
            }

            instance.RebuildSegments();
        }

        private void RequestJudgement(int currentTiming)
        {
            for (int t = FirstJudgeTime; t <= FinalJudgeTime; t += TimeIncrement)
            {
                if (t < currentTiming)
                {
                    continue;
                }

                Services.Judgement.Request(new ArcJudgementRequest()
                {
                    StartAtTiming = t - Values.FarJudgeWindow,
                    ExpireAtTiming = t + Values.HoldLostLateJudgeWindow,
                    AutoAtTiming = t,
                    Arc = this,
                    IsJudgement = true,
                    Receiver = this,
                });
            }
        }

        private void RequestHighlight(int timing)
        {
            Services.Judgement.Request(new ArcJudgementRequest()
            {
                StartAtTiming = timing,
                ExpireAtTiming = timing + Values.HoldHighlightPersistDuration,
                AutoAtTiming = timing,
                Arc = this,
                IsJudgement = false,
                Receiver = this,
            });
        }

        private void SetGroupHighlight(bool highlight, int longParticleUntil)
        {
            if (recursivelyCalled)
            {
                return;
            }

            recursivelyCalled = true;
            foreach (Arc arc in NextArcs)
            {
                arc.SetGroupHighlight(highlight, longParticleUntil);
            }

            Highlight = highlight;
            this.longParticleUntil = longParticleUntil;
            recursivelyCalled = false;
        }

        private void SetGroupFirst(Arc arc)
        {
            if (recursivelyCalled)
            {
                return;
            }

            recursivelyCalled = true;
            foreach (Arc next in NextArcs)
            {
                next.SetGroupFirst(arc);
            }

            firstArcOfGroup = arc;
            recursivelyCalled = false;
        }
    }
}
