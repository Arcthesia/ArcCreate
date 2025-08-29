using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class ArcTap : Note, INote, IArcTapJudgementReceiver
    {
        private readonly Color DesignantColor = new(0.9411765f, 0.16078432f, 0.38039216f, 0.95f);
        private bool isHit;
        private bool isSfx;
        private bool judgementRequestSent;
        private bool sfxPlayed;
        private Texture texture;

        public HashSet<Tap> ConnectedTaps { get; } = new();

        public Arc Arc { get; set; }

        public float Width { get; set; } = 1;

        public float WorldX => Arc.WorldXAt(Timing);

        public float WorldY => Arc.WorldYAt(Timing);

        public string Sfx => Arc.Sfx;

        public bool Designant => Arc.Designant;

        public bool DesignantScore => Arc.DesignantScore;

        public void ProcessArcTapJudgement(int offset, GroupProperties props)
        {
            //Debug.Log(offset);
            var result = props.MapJudgementResult(offset.CalculateJudgeResult());
            var judgeOffset = props.CurrentJudgementOffset;
            Services.Particle.PlayTapParticle(new Vector3(WorldX, WorldY) + judgeOffset, result);
            Services.Particle.PlayTextParticle(new Vector3(WorldX, WorldY) + judgeOffset, result, offset);
            Services.Score.ProcessJudgement(result, offset);
            isHit = true;

            if (!result.IsMiss())
            {
                if (!result.HitOffsetFix())
                    Services.Hitsound.PlayArcTapHitsound(Timing, Sfx, true);
                else
                    Services.Hitsound.PlayArcTapHitsound(Timing - offset, Sfx, true);
            }
        }

        public void ResetJudgeTo(int timing)
        {
            judgementRequestSent = timing > Timing;
            isHit = timing > Timing;
            sfxPlayed = timing > Timing;
        }

        public void Rebuild()
        {
            RecalculateFloorPosition();
        }

        public void ReloadSkin()
        {
            texture = Services.Skin.GetArcTapSkin(this);
            isSfx = !string.IsNullOrEmpty(Sfx) && Sfx != "none";
        }

        public void UpdateJudgement(int currentTiming, GroupProperties groupProperties)
        {
            if (!judgementRequestSent && currentTiming <= Timing)
            {
                RequestJudgement(groupProperties);
                judgementRequestSent = true;
            }

            if (currentTiming >= Timing && !sfxPlayed)
            {
                Services.Hitsound.PlayArcTapHitsound(Timing, Sfx, false);
                sfxPlayed = true;
            }
        }

        public void UpdateRender(int currentTiming, double currentFloorPosition, GroupProperties groupProperties)
        {
            if (isHit && !groupProperties.NoClip) return;

            if (texture == null) ReloadSkin();

            var z = ZPos(currentFloorPosition);
            var pos = groupProperties.FallDirectionScenecontrolApplied * z + new Vector3(WorldX, WorldY, 0);
            var rot = groupProperties.RotationIndividual;
            var scl = groupProperties.ScaleIndividual;
            scl.x *= Width;
            var matrix = groupProperties.GroupMatrix * Matrix4x4.TRS(pos, rot, scl);

            var alpha = ArcFormula.CalculateFadeOutAlpha(z);
            var color = groupProperties.Color;
            color.a *= alpha;

            Services.Render.DrawArcTap(isSfx, texture, matrix, Designant ? DesignantColor : color, IsSelected);
            if (!groupProperties.NoShadow)
            {
                var shadowMatrix = matrix * Matrix4x4.Translate(new Vector3(0, -pos.y, 0));
                Services.Render.DrawArcTapShadow(shadowMatrix, color);
            }
        }

        public int CompareTo(INote other)
        {
            return Timing.CompareTo(other.Timing);
        }

        public override ArcEvent Clone()
        {
            return new ArcTap
            {
                Timing = Timing,
                Arc = Arc,
                Width = Width,
                TimingGroup = TimingGroup
            };
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            var e = newValues as ArcTap;
            Arc = e.Arc;
            Width = e.Width;
        }

        public override void GenerateColliderTriangles(int timing, List<Vector3> vertices, List<int> triangles)
        {
            var mesh = Services.Render.ArcTapMesh;
            vertices.Clear();
            triangles.Clear();
            mesh.GetVertices(vertices);
            mesh.GetTriangles(triangles, 0);

            var fp = TimingGroupInstance.GetFloorPosition(timing);
            var z = ZPos(fp);
            var basePos = new Vector3(WorldX, WorldY, 0);
            var pos = TimingGroupInstance.GroupProperties.FallDirectionScenecontrolApplied * z + basePos;
            var scl = TimingGroupInstance.GroupProperties.ScaleIndividual;
            scl.x *= Width;

            for (var i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                v = v.Multiply(scl);
                v += pos;
                vertices[i] = v;
            }
        }

        private void RequestJudgement(GroupProperties props)
        {
            Services.Judgement.Request(
                new ArcTapJudgementRequest
                {
                    ExpireAtTiming = Timing + Values.MissJudgeWindow,
                    AutoAtTiming = Timing,
                    X = WorldX,
                    Y = WorldY,
                    Width = Width,
                    Receiver = this,
                    Properties = props
                });
        }
    }
}