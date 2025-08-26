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
        private static bool isControllerMode = false;

        private bool highlight = false;
        private bool hasBeenHitOnce = false;
        private int flashCount = 0;
        private float arcGroupAlpha = 1;
        private int longParticleUntil = int.MinValue;
        private Arc firstArcOfBranch;
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

        public float SegmentLength
            => ArcFormula.CalculateArcSegmentLength(EndTiming - Timing, TimingGroupInstance.GroupProperties.ArcResolution);

        public bool ShouldDrawHeightIndicator => !IsTrace && (YStart != YEnd || IsFirstArcOfGroup);

        public float ArcCapSize => IsTrace ? Values.TraceCapSize : Values.ArcCapSize;

        public bool IsFirstArcOfGroup => PreviousArc == null;

        public bool IsFirstArcOfBranch => PreviousArc == null || PreviousArc.NextArc != this;

        public float CurrentDepth { get; private set; }

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
        }

        public void ReloadSkin()
        {
            InputMode inputMode = (InputMode)Settings.InputMode.Value;
            isControllerMode = inputMode == InputMode.AutoController || inputMode == InputMode.Controller;
            (arcCap, heightIndicatorColor) = Services.Skin.GetArcSkin(this);
        }

        public override void GenerateColliderTriangles(int timing, List<Vector3> vertices, List<int> triangles)
        {
            double fp = TimingGroupInstance.GetFloorPosition(timing);
            float z = ZPos(fp);
            Vector3 basePos = new Vector3(ArcFormula.ArcXToWorld(XStart), ArcFormula.ArcYToWorld(YStart), 0);
            Vector3 pos = (TimingGroupInstance.GroupProperties.FallDirection * z) + basePos;
            Vector3 scl = TimingGroupInstance.GroupProperties.ScaleIndividual;
            ArcMeshGenerator.GenerateColliderTriangles(this, vertices, triangles, pos, scl);
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

                    if (currentTiming >= Timing && currentTiming <= EndTiming)
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

                Vector3 midpoint = pos + ((segment.StartPosition + segment.EndPosition) / 2);
                midpoint.z = (zPos + endZPos) / 2;
                float depth = Services.Camera.CalculateDepthSquared(midpoint);

                var (bodyMatrix, shadowMatrix, cornerOffset) =
                        (EndTiming > Timing || IsTrace) ?
                        segment.GetMatrices(currentFloorPosition, groupProperties.FallDirection, z, pos.y) :
                        segment.GetMatricesSlam(
                            currentFloorPosition,
                            groupProperties.FallDirection,
                            z,
                            pos,
                            TimingGroupInstance,
                            NextArc,
                            Values.ArcMeshOffset);

                if (IsTrace)
                {
                    Services.Render.DrawTraceSegment(matrix * bodyMatrix, color, IsSelected, depth);
                    if (!groupProperties.NoShadow)
                    {
                        Services.Render.DrawTraceShadow(matrix * shadowMatrix, color);
                    }
                }
                else
                {
                    Services.Render.DrawArcSegment(Color, highlight, matrix * bodyMatrix, color, IsSelected, redArcValue, basePos.y + segment.EndPosition.y, depth);
                    if (!groupProperties.NoShadow)
                    {
                        Services.Render.DrawArcShadow(matrix * shadowMatrix, color, cornerOffset);
                    }
                }
            }

            if (!groupProperties.NoHeightIndicator && (clipToTiming <= Timing || groupProperties.NoClip) && ShouldDrawHeightIndicator)
            {
                Matrix4x4 heightIndicatorMatrix = Matrix4x4.Scale(new Vector3(1, pos.y - (Values.TraceMeshOffset / 2), 1));
                Services.Render.DrawHeightIndicator(matrix * heightIndicatorMatrix, heightIndicatorColor * groupProperties.Color);
            }

            if (!groupProperties.NoHead && (clipToTiming <= Timing || groupProperties.NoClip) && IsFirstArcOfGroup)
            {
                if (IsTrace)
                {
                    Services.Render.DrawTraceHead(matrix, color, IsSelected);
                }
                else
                {
                    Services.Render.DrawArcHead(Color, highlight, matrix, color, IsSelected, redArcValue, basePos.y + segments[0].EndPosition.y);
                }
            }

            if (!groupProperties.NoArcCap && shouldDrawArcCap)
            {
                Services.Render.DrawArcCap(arcCap, matrix * arcCapMatrix, arcCapColor * groupProperties.Color, isControllerMode);
            }

            if (currentTiming <= longParticleUntil && currentTiming >= Timing && currentTiming <= EndTiming)
            {
                Services.Particle.PlayArcParticle(
                    Color,
                    firstArcOfBranch ?? this,
                    WorldSegmentedPosAt(currentTiming) + groupProperties.CurrentJudgementOffset);
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

        public bool TryGetFirstSegement(out ArcSegmentData segment)
        {
            if (segments.Count >= 1)
            {
                segment = segments[0];
                return true;
            }

            segment = default;
            return false;
        }

        public float WorldSegmentedXAt(int timing)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                if (seg.Timing <= timing && timing <= seg.EndTiming)
                {
                    if (seg.Timing == seg.EndTiming)
                    {
                        return seg.StartPosition.x + ArcFormula.ArcXToWorld(XStart);
                    }

                    float dx = (seg.EndPosition - seg.StartPosition).x;
                    float dt = (float)(timing - seg.Timing) / (seg.EndTiming - seg.Timing);
                    return seg.StartPosition.x + (dt * dx) + ArcFormula.ArcXToWorld(XStart);
                }
            }

            return WorldXAt(timing);
        }

        public Vector3 WorldSegmentedPosAt(int timing)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                if (seg.Timing <= timing && timing <= seg.EndTiming)
                {
                    var xStart = ArcFormula.ArcXToWorld(XStart);
                    var yStart = ArcFormula.ArcYToWorld(YStart);
                    var xSegStart = seg.StartPosition.x;
                    var ySegStart = seg.StartPosition.y;
                    if (seg.Timing == seg.EndTiming)
                    {
                        var sx = xSegStart + xStart;
                        var sy = ySegStart + yStart;
                        return new Vector3(sx, sy);
                    }

                    Vector3 dv = (seg.EndPosition - seg.StartPosition);
                    float dx = dv.x;
                    float dy = dv.y;
                    float dt = (float)(timing - seg.Timing) / (seg.EndTiming - seg.Timing);
                    var x = xSegStart + (dt * dx) + xStart;
                    var y = ySegStart + (dt * dy) + yStart;
                    return new Vector3(x, y);
                }
            }

            return new Vector3(WorldXAt(timing), WorldYAt(timing));
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
                    segment.TimingGroup = TimingGroup;
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
                    Quaternion capRot = Quaternion.identity;
                    if (isControllerMode)
                    {
                        Vector3 displacement = segment.EndPosition - segment.StartPosition;
                        if (IsTrace || (Mathf.Approximately(displacement.x, 0) && Mathf.Approximately(displacement.y, 0)))
                        {
                            return (false, default, default);
                        }

                        displacement.z = 0;
                        capRot = Quaternion.FromToRotation(Vector3.up, displacement);
                    }

                    Vector3 capScale = new Vector3(ArcCapSize, ArcCapSize, 1);

                    Vector4 color = new Vector4(1, 1, 1, IsTrace ? Values.TraceCapAlpha : Values.ArcCapAlpha);
                    return (true, Matrix4x4.TRS(capPos, capRot, capScale), color);
                }
            }

            if (currentTiming <= Timing)
            {
                if (IsFirstArcOfGroup && !IsTrace && !isControllerMode)
                {
                    float approach = Mathf.Clamp(1 - (Mathf.Abs(z) / Values.TrackLengthForward), 0, 1);
                    float size = Values.ArcCapSize + (Values.ArcCapSizeAdditionMax * (1 - approach));

                    Vector3 capPos = -(fallDirection * z);
                    Vector3 scale = new Vector3(size, size, 1);
                    Vector4 color = new Color(1, 1, 1, IsTrace ? 0 : approach);

                    return (true, Matrix4x4.TRS(capPos, Quaternion.identity, scale), color);
                }
                else
                {
                    return (false, default, default);
                }
            }

            return (false, default, default);
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
