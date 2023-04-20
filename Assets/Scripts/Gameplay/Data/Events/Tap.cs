using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Gameplay.Render;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class Tap : Note, INote, ILaneTapJudgementReceiver
    {
        private bool judgementRequestSent = false;
        private bool isHit = false;
        private Texture texture;
        private Color connectionLineColor;

        public HashSet<ArcTap> ConnectedArcTaps { get; } = new HashSet<ArcTap>();

        public int Lane { get; set; }

        public override ArcEvent Clone()
        {
            return new Tap()
            {
                Timing = Timing,
                TimingGroup = TimingGroup,
                Lane = Lane,
            };
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            Tap e = newValues as Tap;
            Lane = e.Lane;
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
            (texture, connectionLineColor) = Services.Skin.GetTapSkin(this);
        }

        public int CompareTo(INote other)
        {
            return Timing.CompareTo(other.Timing);
        }

        public override Mesh GetColliderMesh()
        {
            return Services.Render.TapMesh;
        }

        public override void GetColliderPosition(int timing, out Vector3 pos, out Vector3 scl)
        {
            float z = ZPos(TimingGroupInstance.GetFloorPosition(timing));
            Vector3 basePos = new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0);
            pos = (TimingGroupInstance.GroupProperties.FallDirection * z) + basePos;
            scl = TimingGroupInstance.GroupProperties.ScaleIndividual;
            scl.z *= ArcFormula.CalculateTapSizeScalar(z);
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
            if (isHit && !groupProperties.NoClip)
            {
                return;
            }

            if (texture == null)
            {
                ReloadSkin();
            }

            float z = ZPos(currentFloorPosition);
            Vector3 basePos = new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0);
            Vector3 pos = (groupProperties.FallDirection * z) + basePos;
            Quaternion rot = groupProperties.RotationIndividual;
            Vector3 scl = groupProperties.ScaleIndividual;
            scl.z *= ArcFormula.CalculateTapSizeScalar(z);
            Matrix4x4 matrix = groupProperties.GroupMatrix * Matrix4x4.TRS(pos, rot, scl);

            float alpha = ArcFormula.CalculateFadeOutAlpha(z);
            Color color = groupProperties.Color;
            Color connectionColor = connectionLineColor;
            color.a *= alpha;
            connectionColor.a *= alpha;

            Services.Render.DrawTap(texture, matrix, color, IsSelected);

            foreach (var arctap in ConnectedArcTaps)
            {
                Vector3 arctapPos = new Vector3(arctap.WorldX, arctap.WorldY, 0);
                Vector3 direction = arctapPos - basePos;

                Matrix4x4 lineMatrix = matrix * Matrix4x4.TRS(
                    pos: Vector3.zero,
                    q: Quaternion.LookRotation(direction, Vector3.up),
                    s: new Vector3(1, 1, direction.magnitude));
                Services.Render.DrawConnectionLine(lineMatrix, connectionColor);
            }
        }

        public void ProcessLaneTapJudgement(int offset)
        {
            JudgementResult result = offset.CalculateJudgeResult();
            Services.Particle.PlayTapParticle(new Vector3(ArcFormula.LaneToWorldX(Lane), 0), result);
            Services.Particle.PlayTextParticle(new Vector3(ArcFormula.LaneToWorldX(Lane), 0), result);
            Services.Score.ProcessJudgement(result);
            isHit = true;

            if (!result.IsMiss())
            {
                Services.InputFeedback.LaneFeedback(Lane);
                Services.Hitsound.PlayTapHitsound();
            }
        }

        private void RequestJudgement()
        {
            Services.Judgement.Request(
                new LaneTapJudgementRequest()
                {
                    ExpireAtTiming = Timing + Values.MissJudgeWindow,
                    AutoAtTiming = Timing,
                    Lane = Lane,
                    Receiver = this,
                });
        }
    }
}
