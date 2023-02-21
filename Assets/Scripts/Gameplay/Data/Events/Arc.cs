using System.Collections.Generic;
using System.Linq;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Gameplay.Utility;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class Arc : LongNote, ILongNote<ArcBehaviour>, IArcJudgementReceiver
    {
        private ArcBehaviour instance;
        private bool highlight = false;
        private int numJudgementRequestsSent = 0;
        private bool highlightRequestSent = false;
        private bool hasBeenHitOnce = false;
        private int flashCount = 0;
        private float arcGroupAlpha = 1;
        private int longParticleUntil = int.MinValue;
        private Arc firstArcOfBranch;
        private Mesh colliderMesh;
        private bool isSelected;
        private bool spawnedParticleThisFrame = false;

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

        public HashSet<Arc> NextArcs { get; } = new HashSet<Arc>();

        public HashSet<Arc> PreviousArcs { get; } = new HashSet<Arc>();

        public bool IsAssignedInstance => instance != null;

        public float SegmentLength => ArcFormula.CalculateArcSegmentLength(EndTiming - Timing);

        public bool ShouldDrawHeightIndicator => !IsTrace && (YStart != YEnd || IsFirstArcOfGroup);

        public float ArcCapSize => IsTrace ? Values.TraceCapSize : Values.ArcCapSize;

        public bool IsFirstArcOfGroup => PreviousArcs.Count == 0;

        public bool IsFirstArcOfBranch
        {
            get
            {
                if (PreviousArcs.Count == 0)
                {
                    return true;
                }

                Arc prev = PreviousArcs.First();
                if (prev.NextArcs.Count == 1)
                {
                    return false;
                }

                Arc nextFirst = prev.NextArcs.First();
                return nextFirst != this;
            }
        }

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

        public override bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                if (instance != null)
                {
                    instance.SetSelected(value);
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
            instance.SetCollider(colliderMesh);

            ReloadSkin();
            RebuildSegments();

            instance.SetSelected(isSelected);
        }

        public ArcBehaviour RevokeInstance()
        {
            var result = instance;
            instance = null;
            return result;
        }

        public override int ComboAt(int timing)
        {
            return IsTrace ? 0 : base.ComboAt(timing);
        }

        public void ResetJudgeTo(int timing)
        {
            RecalculateJudgeTimings();
            Highlight = Highlight && timing >= Timing && timing <= EndTiming;
            longParticleUntil = int.MinValue;
            numJudgementRequestsSent = ComboAt(timing);
            highlightRequestSent = false;
            arcGroupAlpha = 1;
            hasBeenHitOnce = hasBeenHitOnce && timing >= Timing && timing <= EndTiming;
        }

        public void Rebuild()
        {
            if (IsFirstArcOfBranch)
            {
                SetBranchFirst(this);
            }

            RecalculateFloorPosition();
            RecalculateJudgeTimings();
            RebuildCollider();

            if (instance != null)
            {
                AssignInstance(instance);
                instance.SetData(this);
                instance.SetCollider(colliderMesh);
                instance.SetSelected(isSelected);
                ReloadSkin();

                if (Values.EnableArcRebuildSegment)
                {
                    instance.RebuildSegments();
                }
            }
        }

        public override void RecalculateJudgeTimings()
        {
            TotalCombo = 0;
            FirstJudgeTime = double.MaxValue;
            TimeIncrement = double.MaxValue;

            if (IsTrace || EndTiming == Timing)
            {
                return;
            }

            double bpm = TimingGroupInstance.GetBpm(Timing);

            if (bpm == 0)
            {
                return;
            }

            int duration = EndTiming - Timing;
            bpm = System.Math.Abs(bpm);
            TimeIncrement = (bpm >= 255 ? 60_000 : 30_000) / bpm / Values.TimingPointDensity;

            int count = (int)(duration / TimeIncrement);
            int whatTheFuckDoesThisMean = (IsFirstArcOfGroup ? 0 : 1) ^ 1;
            if (count <= whatTheFuckDoesThisMean)
            {
                TotalCombo = 1;
                FirstJudgeTime = Timing + (duration / 2);
            }
            else
            {
                TotalCombo = count - whatTheFuckDoesThisMean;
                FirstJudgeTime = Timing + (whatTheFuckDoesThisMean * TimeIncrement);
            }
        }

        public void ReloadSkin()
        {
            var (normal, highlight, shadow, arcCap, heightIndicatorSprite, heightIndicatorColor) = Services.Skin.GetArcSkin(this);
            instance.SetSkin(normal, highlight, shadow, arcCap, heightIndicatorSprite, heightIndicatorColor);
        }

        public void UpdateJudgement(int currentTiming, GroupProperties groupProperties)
        {
            if (!IsTrace && currentTiming >= Timing && Timing < EndTiming)
            {
                RequestJudgement();
            }

            if (!IsTrace && currentTiming >= Timing && Timing < EndTiming && !highlightRequestSent)
            {
                RequestHighlight(currentTiming);
                highlightRequestSent = true;
            }

            spawnedParticleThisFrame = false;
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
                    if (currentTiming >= firstArcOfBranch.Timing)
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

            if (hasBeenHitOnce || IsTrace || groupProperties.NoInput)
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
                    firstArcOfBranch,
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
                    if (!spawnedParticleThisFrame)
                    {
                        Services.Particle.PlayTextParticle(currentPos, JudgementResult.LostLate);
                        spawnedParticleThisFrame = true;
                    }

                    Services.Score.ProcessJudgement(JudgementResult.LostLate);
                }
            }
            else if (currentTiming <= EndTiming + Values.HoldLostLateJudgeWindow)
            {
                SetGroupHighlight(true, currentTiming + Values.HoldParticlePersistDuration);
                hasBeenHitOnce = true;

                if (isJudgement)
                {
                    if (!spawnedParticleThisFrame)
                    {
                        Services.Particle.PlayTextParticle(currentPos, JudgementResult.Max);
                        spawnedParticleThisFrame = true;
                    }

                    Services.Score.ProcessJudgement(JudgementResult.Max);
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
                return ArcFormula.ArcXToWorld(timing <= Timing ? XStart : XEnd);
            }

            float p = Mathf.Clamp((float)(timing - Timing) / (EndTiming - Timing), 0, 1);
            return ArcFormula.ArcXToWorld(ArcFormula.X(XStart, XEnd, p, LineType));
        }

        public float WorldYAt(int timing)
        {
            if (EndTiming == Timing)
            {
                return ArcFormula.ArcYToWorld(timing <= Timing ? YStart : YEnd);
            }

            float p = Mathf.Clamp((float)(timing - Timing) / (EndTiming - Timing), 0, 1);
            return ArcFormula.ArcYToWorld(ArcFormula.Y(YStart, YEnd, p, LineType));
        }

        public float ArcXAt(int timing)
        {
            if (EndTiming == Timing)
            {
                return timing <= Timing ? XStart : XEnd;
            }

            float p = Mathf.Clamp((float)(timing - Timing) / (EndTiming - Timing), 0, 1);
            return ArcFormula.X(XStart, XEnd, p, LineType);
        }

        public float ArcYAt(int timing)
        {
            if (EndTiming == Timing)
            {
                return timing <= Timing ? YStart : YEnd;
            }

            float p = Mathf.Clamp((float)(timing - Timing) / (EndTiming - Timing), 0, 1);
            return ArcFormula.Y(YStart, YEnd, p, LineType);
        }

        public void CleanColliderMesh()
        {
            UnityEngine.Object.Destroy(colliderMesh);
            colliderMesh = null;
        }

        private void RebuildSegments()
        {
            if (instance == null)
            {
                return;
            }

            instance.RebuildSegments();
        }

        private void RebuildCollider()
        {
            if (colliderMesh != null)
            {
                UnityEngine.Object.Destroy(colliderMesh);
                colliderMesh = null;
            }

            if (Values.EnableColliderGeneration)
            {
                colliderMesh = ArcMeshGenerator.GenerateColliderMesh(this);
                if (instance != null)
                {
                    instance.SetCollider(colliderMesh);
                }
            }
        }

        private void RequestJudgement()
        {
            for (int t = numJudgementRequestsSent; t < TotalCombo; t++)
            {
                int timing = (int)System.Math.Round(FirstJudgeTime + (t * TimeIncrement));
                Services.Judgement.Request(new ArcJudgementRequest()
                {
                    StartAtTiming = timing - Values.FarJudgeWindow,
                    ExpireAtTiming = timing + Values.HoldLostLateJudgeWindow,
                    AutoAtTiming = timing,
                    Arc = this,
                    IsJudgement = true,
                    Receiver = this,
                });
            }

            numJudgementRequestsSent = TotalCombo;
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

        private void SetBranchFirst(Arc arc)
        {
            if (recursivelyCalled)
            {
                return;
            }

            recursivelyCalled = true;
            foreach (Arc next in NextArcs)
            {
                next.SetBranchFirst(arc);
            }

            firstArcOfBranch = arc;
            recursivelyCalled = false;
        }
    }
}
