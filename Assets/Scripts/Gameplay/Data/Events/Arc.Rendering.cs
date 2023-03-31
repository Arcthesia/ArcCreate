using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Gameplay.Render;
using ArcCreate.Gameplay.Utility;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    /// <summary>
    /// Partial class for rendering.
    /// </summary>
    public partial class Arc : LongNote, ILongNote, IArcJudgementReceiver
    {
        private bool highlight = false;
        private bool hasBeenHitOnce = false;
        private int flashCount = 0;
        private float arcGroupAlpha = 1;
        private int longParticleUntil = int.MinValue;
        private Arc firstArcOfBranch;
        private Mesh colliderMesh;
        private Texture arcCap;
        private Color heightIndicatorColor;
        private readonly List<ArcSegmentData> segments = new List<ArcSegmentData>();

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

        public Arc NextArc { get; set; }

        public Arc PreviousArc { get; set; }

        public float SegmentLength => ArcFormula.CalculateArcSegmentLength(EndTiming - Timing);

        public bool ShouldDrawHeightIndicator => !IsTrace && (YStart != YEnd || IsFirstArcOfGroup);

        public float ArcCapSize => IsTrace ? Values.TraceCapSize : Values.ArcCapSize;

        public bool IsFirstArcOfGroup => PreviousArc == null;

        public bool IsFirstArcOfBranch => PreviousArc == null || PreviousArc.NextArc != this;

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
                Sfx = Sfx,
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
            Sfx = n.Sfx;
        }

        public override int ComboAt(int timing)
        {
            return IsTrace ? 0 : base.ComboAt(timing);
        }

        public void Rebuild()
        {
            if (IsFirstArcOfBranch)
            {
                SetBranchFirst(this);
            }

            RecalculateFloorPosition();
            RecalculateJudgeTimings();
            ReloadSkin();
            RebuildSegments();
            RebuildCollider();
        }

        public void ReloadSkin()
        {
            (arcCap, heightIndicatorColor) = Services.Skin.GetArcSkin(this);
        }

        public override Mesh GetColliderMesh()
        {
            if (colliderMesh == null)
            {
                colliderMesh = ArcMeshGenerator.GenerateColliderMesh(this);
            }

            return colliderMesh;
        }

        public override void GetColliderPosition(int timing, out Vector3 pos, out Vector3 scl)
        {
            double fp = TimingGroupInstance.GetFloorPosition(timing);
            float z = ZPos(fp);
            Vector3 basePos = new Vector3(ArcFormula.ArcXToWorld(XStart), ArcFormula.ArcYToWorld(YStart), 0);
            pos = (TimingGroupInstance.GroupProperties.FallDirection * z) + basePos;
            scl = TimingGroupInstance.GroupProperties.ScaleIndividual;
        }

        public void UpdateRender(int currentTiming, double currentFloorPosition, GroupProperties groupProperties)
        {
            float z = ZPos(currentFloorPosition);

            Vector3 basePos = new Vector3(ArcFormula.ArcXToWorld(XStart), ArcFormula.ArcYToWorld(YStart), 0);
            Vector3 pos = (groupProperties.FallDirection * z) + basePos;
            Quaternion rot = groupProperties.RotationIndividual;
            Vector3 scl = groupProperties.ScaleIndividual;
            Matrix4x4 matrix = groupProperties.GroupMatrix * Matrix4x4.TRS(pos, rot, scl);

            float alpha = 1;
            float redArcValue = Services.Skin.GetRedArcValue(Color);
            if (!IsTrace)
            {
                if (highlight)
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
                    if (currentTiming >= (firstArcOfBranch?.Timing ?? Timing))
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

            int clipToTiming;
            double clipToFloorPosition;
            if (hasBeenHitOnce || IsTrace || groupProperties.NoInput || Timing == EndTiming)
            {
                clipToTiming = currentTiming;
                clipToFloorPosition = currentFloorPosition;
            }
            else
            {
                clipToTiming = Timing;
                clipToFloorPosition = FloorPosition;
            }

            var (shouldDrawArcCap, arcCapMatrix, arcCapColor) = UpdateSegmentsAndArcCap(
                currentTiming,
                currentFloorPosition,
                clipToTiming,
                clipToFloorPosition,
                groupProperties.FallDirection,
                z,
                groupProperties.NoClip);

            for (int i = 0; i < segments.Count; i++)
            {
                ArcSegmentData segment = segments[i];
                float from = segment.From;
                float zPos = segment.CalculateZPos(currentFloorPosition);
                float endZPos = segment.CalculateEndZPos(currentFloorPosition);
                if (from > 1
                 || (zPos < -Values.TrackLengthForward && endZPos < -Values.TrackLengthForward)
                 || (zPos > Values.TrackLengthBackward && endZPos > Values.TrackLengthBackward)
                 || (Timing != EndTiming && zPos == endZPos))
                {
                    continue;
                }

                ArcRenderProperties properties = new ArcRenderProperties
                {
                    Color = color,
                    RedValue = redArcValue,
                    Selected = IsSelected ? 1 : 0,
                };

                SpriteRenderProperties shadowProperties = new SpriteRenderProperties
                {
                    Color = color,
                };

                var (bodyMatrix, shadowMatrix) = segment.GetMatrices(currentFloorPosition, groupProperties.FallDirection, z);
                if (IsTrace)
                {
                    Services.Render.DrawTraceSegment(matrix * bodyMatrix, properties);
                    Services.Render.DrawTraceShadow(matrix * shadowMatrix, shadowProperties);
                }
                else
                {
                    Services.Render.DrawArcSegment(Color, highlight, matrix * bodyMatrix, properties);
                    Services.Render.DrawArcShadow(matrix * shadowMatrix, shadowProperties);
                }
            }

            if ((clipToTiming <= Timing || groupProperties.NoClip) && ShouldDrawHeightIndicator)
            {
                SpriteRenderProperties heightIndicatorProperties = new SpriteRenderProperties
                {
                    Color = heightIndicatorColor * groupProperties.Color,
                };

                Matrix4x4 heightIndicatorMatrix = Matrix4x4.Scale(new Vector3(1, pos.y - (Values.TraceMeshOffset / 2), 1));

                Services.Render.DrawHeightIndicator(matrix * heightIndicatorMatrix, heightIndicatorProperties);
            }

            if ((clipToTiming <= Timing || groupProperties.NoClip) && IsFirstArcOfGroup)
            {
                ArcRenderProperties properties = new ArcRenderProperties
                {
                    Color = color,
                    RedValue = redArcValue,
                    Selected = IsSelected ? 1 : 0,
                };

                if (IsTrace)
                {
                    Services.Render.DrawTraceHead(matrix, properties);
                }
                else
                {
                    Services.Render.DrawArcHead(Color, highlight, matrix, properties);
                }
            }

            if (shouldDrawArcCap)
            {
                SpriteRenderProperties arccapProperties = new SpriteRenderProperties
                {
                    Color = arcCapColor * groupProperties.Color,
                };

                Services.Render.DrawArcCap(arcCap, matrix * arcCapMatrix, arccapProperties);
            }

            if (currentTiming <= longParticleUntil && currentTiming >= Timing && currentTiming <= EndTiming)
            {
                Services.Particle.PlayLongParticle(
                    firstArcOfBranch ?? this,
                    new Vector3(WorldXAt(currentTiming), WorldYAt(currentTiming), 0));
            }
        }

        public int CompareTo(INote other)
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
            if (colliderMesh != null)
            {
                Object.Destroy(colliderMesh);
                colliderMesh = null;
            }
        }

        private void RebuildSegments()
        {
            if (Values.EnableArcRebuildSegment || segments.Count == 0)
            {
                int lastEndTiming = Timing;
                double lastEndFloorPosition = TimingGroupInstance.GetFloorPosition(Timing);
                Vector2 basePosition = new Vector2(ArcFormula.ArcXToWorld(XStart), ArcFormula.ArcYToWorld(YStart));
                Vector2 lastPosition = basePosition;

                int i = 0;
                while (true)
                {
                    int timing = lastEndTiming;
                    int endTiming = timing + Mathf.RoundToInt(SegmentLength);
                    int cappedEndTiming = Mathf.Min(endTiming, EndTiming);

                    ArcSegmentData segment = i < segments.Count ? segments[i] : default;
                    if (i >= segments.Count)
                    {
                        segments.Add(segment);
                    }

                    segment.Timing = timing;
                    segment.EndTiming = cappedEndTiming;
                    segment.FloorPosition = lastEndFloorPosition;
                    segment.StartPosition = lastPosition - basePosition;

                    lastEndFloorPosition = TimingGroupInstance.GetFloorPosition(cappedEndTiming);
                    segment.EndFloorPosition = lastEndFloorPosition;
                    lastPosition = cappedEndTiming == EndTiming ?
                        new Vector2(ArcFormula.ArcXToWorld(XEnd), ArcFormula.ArcYToWorld(YEnd)) :
                        new Vector2(WorldXAt(cappedEndTiming), WorldYAt(cappedEndTiming));
                    segment.EndPosition = lastPosition - basePosition;
                    segment.From = 0;

                    segments[i] = segment;

                    i += 1;

                    lastEndTiming = endTiming;
                    if (endTiming >= EndTiming)
                    {
                        break;
                    }
                }

                for (int j = segments.Count - 1; j >= i; j--)
                {
                    segments.RemoveAt(j);
                }
            }
            else if (segments.Count > 0)
            {
                // realign segment timing
                int firstTiming = segments[0].Timing;
                double firstFloorPos = segments[0].FloorPosition;
                for (int i = 0; i < segments.Count; i++)
                {
                    ArcSegmentData segment = segments[i];
                    segment.Timing = segment.Timing - firstTiming + Timing;
                    segment.EndTiming = segment.EndTiming - firstTiming + Timing;
                    segment.FloorPosition = segment.FloorPosition - firstFloorPos + FloorPosition;
                    segment.EndFloorPosition = segment.EndFloorPosition - firstFloorPos + FloorPosition;
                    segments[i] = segment;
                }
            }
        }

        private (bool shouldDrawArcCap, Matrix4x4 arcCapMatrix, Vector4 arcCapColor) UpdateSegmentsAndArcCap(
            int currentTiming,
            double currentFloorPosition,
            int clipToTiming,
            double clipToFloorPosition,
            Vector3 fallDirection,
            float z,
            bool noclip)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                ArcSegmentData segment = segments[i];
                if (clipToTiming >= segment.EndTiming && currentTiming >= segment.EndTiming && !noclip)
                {
                    segment.From = 1;
                }
                else if (clipToTiming <= segment.Timing || noclip)
                {
                    segment.From = 0;
                }
                else
                {
                    float p = (float)((clipToFloorPosition - segment.FloorPosition) / (segment.EndFloorPosition - segment.FloorPosition));
                    segment.From = p;
                }

                segments[i] = segment;

                if (currentTiming >= segment.Timing && currentTiming < segment.EndTiming)
                {
                    float p = (float)((currentFloorPosition - segment.FloorPosition) / (segment.EndFloorPosition - segment.FloorPosition));
                    if (segment.EndFloorPosition == segment.FloorPosition)
                    {
                        p = 0;
                    }

                    Vector3 capPos = segment.StartPosition + (p * (segment.EndPosition - segment.StartPosition)) - (fallDirection * z);
                    Vector3 scale = new Vector3(ArcCapSize, ArcCapSize, 1);
                    Vector4 color = new Vector4(1, 1, 1, IsTrace ? Values.TraceCapAlpha : Values.ArcCapAlpha);
                    return (true, Matrix4x4.TRS(capPos, Quaternion.identity, scale), color);
                }
            }

            if (currentTiming <= Timing)
            {
                if (IsFirstArcOfGroup && !IsTrace)
                {
                    float approach = 1 - (Mathf.Abs(z) / Values.TrackLengthForward);
                    Vector3 capPos = -(fallDirection * z);
                    Vector4 color = new Color(1, 1, 1, IsTrace ? 0 : approach);
                    Vector3 scale = Vector3.one;
                    float size = Values.ArcCapSize + (Values.ArcCapSizeAdditionMax * (1 - approach));
                    scale = new Vector3(size, size, 1);
                    return (true, Matrix4x4.TRS(capPos, Quaternion.identity, scale), color);
                }
                else
                {
                    return (false, default, default);
                }
            }

            return (false, default, default);
        }

        private void RebuildCollider()
        {
            CleanColliderMesh();
            if (Values.EnableColliderGeneration)
            {
                colliderMesh = ArcMeshGenerator.GenerateColliderMesh(this);
            }
        }

        private void SetGroupHighlight(bool highlight, int longParticleUntil)
        {
            if (recursivelyCalled)
            {
                return;
            }

            recursivelyCalled = true;
            NextArc?.SetGroupHighlight(highlight, longParticleUntil);

            this.highlight = highlight;
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
            NextArc?.SetBranchFirst(arc);

            firstArcOfBranch = arc;
            recursivelyCalled = false;
        }
    }
}
