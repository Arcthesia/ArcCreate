using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class ArcTap : Note, INote<ArcTapBehaviour>, IArcTapJudgementReceiver
    {
        private ArcTapBehaviour instance;
        private bool judgementRequestSent = false;

        public HashSet<Tap> ConnectedTaps { get; } = new HashSet<Tap>();

        public Arc Arc { get; set; }

        public float WorldX => Arc.WorldXAt(Timing);

        public float WorldY => Arc.WorldYAt(Timing);

        public string Sfx => Arc.Sfx;

        public bool IsAssignedInstance => instance != null;

        public override ArcEvent Clone()
        {
            return new ArcTap()
            {
                Timing = Timing,
                Arc = Arc,
            };
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            ArcTap e = newValues as ArcTap;
            Arc = e.Arc;
        }

        public void AssignInstance(ArcTapBehaviour instance)
        {
            this.instance = instance;
            instance.SetData(this);
            ReloadSkin();
        }

        public ArcTapBehaviour RevokeInstance()
        {
            var result = instance;
            instance = null;
            return result;
        }

        public void ResetJudge()
        {
            judgementRequestSent = false;
        }

        public void Rebuild()
        {
        }

        public void ReloadSkin()
        {
            (Mesh mesh, Material mat) = Services.Skin.GetArcTapSkin(this);
            instance.SetSkin(mesh, mat);
        }

        public void UpdateJudgement(int timing, GroupProperties groupProperties)
        {
            if (!judgementRequestSent)
            {
                RequestJudgement();
                judgementRequestSent = true;
            }
        }

        public void UpdateInstance(int timing, double floorPosition, GroupProperties groupProperties)
        {
            if (instance == null)
            {
                return;
            }

            float z = ZPos(floorPosition);
            Vector3 pos = (groupProperties.FallDirection * z) + new Vector3(WorldX, WorldY, 0);
            Quaternion rot = groupProperties.RotationIndividual;
            Vector3 scl = groupProperties.ScaleIndividual;
            scl.y = ArcFormula.CalculateTapSizeScalar(z) * scl.y;
            instance.SetTransform(pos, rot, scl);

            float alpha = ArcFormula.CalculateFadeOutAlpha(z);
            Color color = groupProperties.Color;
            color.a *= alpha;
            instance.SetColor(color);
        }

        public int CompareTo(INote<ArcTapBehaviour> other)
        {
            return Timing.CompareTo(other.Timing);
        }

        public void ProcessArcTapJudgement(int offset)
        {
            JudgementResult result = offset.CalculateJudgeResult();
            Services.Particle.PlayTapParticle(new Vector3(WorldX, WorldY), result);
            Services.Particle.PlayTextParticle(new Vector3(WorldX, WorldY), result);
            Services.Score.ProcessJudgement(result);
            if (instance != null)
            {
                instance.gameObject.SetActive(false);
            }
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
