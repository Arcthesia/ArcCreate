using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class Tap : Note, INote<TapBehaviour>, ILaneTapJudgementReceiver
    {
        private TapBehaviour instance;
        private bool judgementRequestSent = false;
        private bool isSelected;

        public HashSet<ArcTap> ConnectedArcTaps { get; } = new HashSet<ArcTap>();

        public int Lane { get; set; }

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

        public void AssignInstance(TapBehaviour instance)
        {
            this.instance = instance;
            instance.SetData(this);
            instance.SetConnectionLines(
                ConnectedArcTaps,
                new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0));
            instance.SetSelected(isSelected);
            ReloadSkin();
        }

        public TapBehaviour RevokeInstance()
        {
            var result = instance;
            instance = null;
            return result;
        }

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
            if (instance != null)
            {
                instance.gameObject.SetActive(true);
            }
        }

        public void Rebuild()
        {
            if (instance != null)
            {
                instance.SetConnectionLines(
                    ConnectedArcTaps,
                    new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0));
            }
        }

        public void ReloadSkin()
        {
            if (instance != null)
            {
                instance.SetSprite(Services.Skin.GetTapSkin(this));
            }
        }

        public int CompareTo(INote<TapBehaviour> other)
        {
            return Timing.CompareTo(other.Timing);
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
            Vector3 pos = (groupProperties.FallDirection * z) + new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0);
            Quaternion rot = groupProperties.RotationIndividual;
            Vector3 scl = groupProperties.ScaleIndividual;
            scl.y = ArcFormula.CalculateTapSizeScalar(z) * scl.y;
            instance.SetTransform(pos, rot, scl);

            float alpha = ArcFormula.CalculateFadeOutAlpha(z);
            Color color = groupProperties.Color;
            color.a *= alpha;
            instance.SetColor(color);
        }

        public void ProcessLaneTapJudgement(int offset)
        {
            JudgementResult result = offset.CalculateJudgeResult();
            Services.Particle.PlayTapParticle(new Vector3(ArcFormula.LaneToWorldX(Lane), 0), result);
            Services.Particle.PlayTextParticle(new Vector3(ArcFormula.LaneToWorldX(Lane), 0), result);
            Services.Score.ProcessJudgement(result);
            if (instance != null)
            {
                instance.gameObject.SetActive(false);
            }

            if (!result.IsLost())
            {
                Services.InputFeedback.LaneFeedback(Lane);
            }
        }

        private void RequestJudgement()
        {
            Services.Judgement.Request(
                new LaneTapJudgementRequest()
                {
                    ExpireAtTiming = Timing + Values.LostJudgeWindow,
                    AutoAtTiming = Timing,
                    Lane = Lane,
                    Receiver = this,
                });
        }
    }
}
