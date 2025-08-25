using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Gameplay.Scenecontrol;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class ArcTap : Note, INote, IArcTapJudgementReceiver
    {
        private bool judgementRequestSent = false;
        private bool isHit = false;
        private bool isSfx;
        private bool sfxPlayed = false;
        private Texture texture;

        public HashSet<Tap> ConnectedTaps { get; } = new HashSet<Tap>();

        public Arc Arc { get; set; }

        public float Width { get; set; } = 1;

        public float WorldX => Arc.WorldXAt(Timing);

        public float WorldY => Arc.WorldYAt(Timing);

        public string Sfx => Arc.Sfx;

        public override ArcEvent Clone()
        {
            return new ArcTap()
            {
                Timing = Timing,
                Arc = Arc,
                Width = Width,
                TimingGroup = TimingGroup,
            };
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            ArcTap e = newValues as ArcTap;
            Arc = e.Arc;
            Width = e.Width;
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

        public override void GenerateColliderTriangles(int timing, List<Vector3> vertices, List<int> triangles)
        {
            Mesh mesh = Services.Render.ArcTapMesh;
            vertices.Clear();
            triangles.Clear();
            mesh.GetVertices(vertices);
            mesh.GetTriangles(triangles, 0);

            double fp = TimingGroupInstance.GetFloorPosition(timing);
            float z = ZPos(fp);
            Vector3 basePos = new Vector3(WorldX, WorldY, 0);
            Vector3 pos = (TimingGroupInstance.GroupProperties.FallDirection * z) + basePos;
            Vector3 scl = TimingGroupInstance.GroupProperties.ScaleIndividual;
            scl.x *= Width;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 v = vertices[i];
                v = v.Multiply(scl);
                v += pos;
                vertices[i] = v;
            }
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
                Services.Hitsound.PlayArcTapHitsound(Timing, Sfx, isFromJudgement: false);
                sfxPlayed = true;
            }
        }

        public void UpdateRender(int currentTiming, double currentFloorPosition, GroupProperties groupProperties)
        {
            if (isHit && !groupProperties.NoClip)
            {
                return;
            }

            if (texture == null)
            {
                ReloadSkin();
            }

            float z = ZPos(currentFloorPosition);
            Vector3 pos = (groupProperties.FallDirection * z) + new Vector3(WorldX, WorldY, 0);
            Quaternion rot = groupProperties.RotationIndividual;
            Vector3 scl = groupProperties.ScaleIndividual;
            scl.x *= Width;
            Matrix4x4 matrix = groupProperties.GroupMatrix * Matrix4x4.TRS(pos, rot, scl);

            float alpha = ArcFormula.CalculateFadeOutAlpha(z);
            Color color = groupProperties.Color;
            color.a *= alpha;

            Services.Render.DrawArcTap(isSfx, texture, matrix, color, IsSelected);
            if (!groupProperties.NoShadow)
            {
                Matrix4x4 shadowMatrix = matrix * Matrix4x4.Translate(new Vector3(0, -pos.y, 0));
                Services.Render.DrawArcTapShadow(shadowMatrix, color);
            }
        }

        public int CompareTo(INote other)
        {
            return Timing.CompareTo(other.Timing);
        }

        public void ProcessArcTapJudgement(int offset, GroupProperties props)
        {
            JudgementResult result = props.MapJudgementResult(offset.CalculateJudgeResult());
            Vector3 judgeOffset = props.CurrentJudgementOffset;
            Services.Particle.PlayTapParticle(new Vector3(WorldX, WorldY) + judgeOffset, result, Sfx != "none" && Sfx != "");
            Services.Particle.PlayTextParticle(new Vector3(WorldX, WorldY) + judgeOffset, result, offset);
            Services.Score.ProcessJudgement(result, offset);
            isHit = true;

            if (!result.IsMiss())
            {
                Services.Hitsound.PlayArcTapHitsound(Timing, Sfx, isFromJudgement: true);
            }
        }

        private void RequestJudgement(GroupProperties props)
        {
            Services.Judgement.Request(
                new ArcTapJudgementRequest()
                {
                    ExpireAtTiming = Timing + Values.MissJudgeWindow,
                    AutoAtTiming = Timing,
                    X = WorldX,
                    Y = WorldY,
                    Width = Width,
                    Receiver = this,
                    Properties = props,
                });
        }
    }
}
