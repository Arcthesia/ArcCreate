using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Gameplay.Render;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class ArcTap : Note, INote, IArcTapJudgementReceiver
    {
        private bool judgementRequestSent = false;
        private bool isHit = false;
        private bool isSfx;
        private Texture texture;

        public HashSet<Tap> ConnectedTaps { get; } = new HashSet<Tap>();

        public Arc Arc { get; set; }

        public float WorldX => Arc.WorldXAt(Timing);

        public float WorldY => Arc.WorldYAt(Timing);

        public string Sfx => Arc.Sfx;

        public override ArcEvent Clone()
        {
            return new ArcTap()
            {
                Timing = Timing,
                Arc = Arc,
                TimingGroup = TimingGroup,
            };
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            ArcTap e = newValues as ArcTap;
            Arc = e.Arc;
        }

        public void ResetJudgeTo(int timing)
        {
            judgementRequestSent = timing > Timing;
            isHit = timing > Timing;
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

        public override Mesh GetColliderMesh()
        {
            return Services.Render.ArcTapMesh;
        }

        public override void GetColliderPosition(int timing, out Vector3 pos, out Vector3 scl)
        {
            double fp = TimingGroupInstance.GetFloorPosition(timing);
            float z = ZPos(fp);
            Vector3 basePos = new Vector3(WorldX, WorldY, 0);
            pos = (TimingGroupInstance.GroupProperties.FallDirection * z) + basePos;
            scl = TimingGroupInstance.GroupProperties.ScaleIndividual;
        }

        public void UpdateJudgement(int currentTiming, GroupProperties groupProperties)
        {
            if (!judgementRequestSent && currentTiming <= Timing)
            {
                RequestJudgement();
                judgementRequestSent = true;
            }
        }

        public void UpdateRender(int currentTiming, double currentFloorPosition, GroupProperties groupProperties)
        {
            if (isHit)
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
            Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, scl);
            Matrix4x4 shadowMatrix = matrix * Matrix4x4.Translate(new Vector3(0, -pos.y, 0));

            float alpha = ArcFormula.CalculateFadeOutAlpha(z);
            Color color = groupProperties.Color;
            color.a *= alpha;

            NoteRenderProperties tapProperties = new NoteRenderProperties
            {
                Color = color,
                Selected = IsSelected ? 1 : 0,
            };

            SpriteRenderProperties shadowProperties = new SpriteRenderProperties
            {
                Color = color,
            };

            Services.Render.DrawArcTap(isSfx, texture, matrix, tapProperties);
            Services.Render.DrawArcTapShadow(shadowMatrix, shadowProperties);
        }

        public int CompareTo(INote other)
        {
            return Timing.CompareTo(other.Timing);
        }

        public void ProcessArcTapJudgement(int offset)
        {
            JudgementResult result = offset.CalculateJudgeResult();
            Services.Particle.PlayTapParticle(new Vector3(WorldX, WorldY), result);
            Services.Particle.PlayTextParticle(new Vector3(WorldX, WorldY), result);
            Services.Score.ProcessJudgement(result);
            isHit = true;
        }

        private void RequestJudgement()
        {
            Services.Judgement.Request(
                new ArcTapJudgementRequest()
                {
                    ExpireAtTiming = Timing + Values.LostJudgeWindow,
                    AutoAtTiming = Timing,
                    X = WorldX,
                    Y = WorldY,
                    Receiver = this,
                });
        }
    }
}
