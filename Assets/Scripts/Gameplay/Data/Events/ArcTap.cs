using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class ArcTap : Note, INote<ArcTapBehaviour>, IArcTapJudgementReceiver
    {
        private ArcTapBehaviour instance;
        private bool judgementRequestSent = false;
        private bool isSelected;

        public HashSet<Tap> ConnectedTaps { get; } = new HashSet<Tap>();

        public Arc Arc { get; set; }

        public float WorldX => Arc.WorldXAt(Timing);

        public float WorldY => Arc.WorldYAt(Timing);

        public string Sfx => Arc.Sfx;

        public bool IsAssignedInstance => instance != null;

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

        public void AssignInstance(ArcTapBehaviour instance)
        {
            this.instance = instance;
            instance.SetData(this);
            instance.SetSelected(isSelected);
            ReloadSkin();
        }

        public ArcTapBehaviour RevokeInstance()
        {
            var result = instance;
            instance = null;
            return result;
        }

        public void ResetJudgeTo(int timing)
        {
            judgementRequestSent = timing > Timing;
            if (instance != null)
            {
                instance.gameObject.SetActive(true);
            }
        }

        public void Rebuild()
        {
            RecalculateFloorPosition();
        }

        public void ReloadSkin()
        {
            (Mesh mesh, Material mat, Sprite shadow) = Services.Skin.GetArcTapSkin(this);
            instance.SetSkin(mesh, mat, shadow);
        }

        public void UpdateJudgement(int currentTiming, GroupProperties groupProperties)
        {
            if (!judgementRequestSent && currentTiming <= Timing)
            {
                RequestJudgement();
                judgementRequestSent = true;
            }
        }

        public void UpdateInstance(int currentTiming, double currentFloorPosition, GroupProperties groupProperties)
        {
            if (instance == null)
            {
                return;
            }

            float z = ZPos(currentFloorPosition);
            Vector3 pos = (groupProperties.FallDirection * z) + new Vector3(WorldX, WorldY, 0);
            Quaternion rot = groupProperties.RotationIndividual;
            Vector3 scl = groupProperties.ScaleIndividual;
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
