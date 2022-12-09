using ArcCreate.Gameplay.Judgement;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class Hold : LongNote, ILongNote<HoldBehaviour>, ILaneTapJudgementReceiver, ILaneHoldJudgementReceiver
    {
        private HoldBehaviour instance;
        private int flashCount;
        private bool highlight = false;
        private bool locked = true;
        private bool tapJudgementRequestSent = false;
        private bool holdJudgementRequestSent = false;

        public int Lane { get; set; }

        public bool IsAssignedInstance => instance != null;

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

        public override ArcEvent Clone()
        {
            return new Hold()
            {
                Timing = Timing,
                EndTiming = EndTiming,
                TimingGroup = TimingGroup,
                Lane = Lane,
            };
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            Hold e = newValues as Hold;
            Lane = e.Lane;
        }

        public void AssignInstance(HoldBehaviour instance)
        {
            this.instance = instance;
            instance.SetData(this);
            ReloadSkin();
        }

        public HoldBehaviour RevokeInstance()
        {
            var result = instance;
            instance = null;
            return result;
        }

        public void ResetJudge()
        {
            ResetJudgeTimings();
            locked = true;
            Highlight = false;
            tapJudgementRequestSent = false;
            holdJudgementRequestSent = false;
            FloorPosition = TimingGroupInstance.GetFloorPosition(Timing);
        }

        public void Rebuild()
        {
            RecalculateJudgeTimings();
        }

        public void ReloadSkin()
        {
            var (normal, highlight) = Services.Skin.GetHoldSkin(this);
            instance.SetSprite(normal, highlight);
        }

        public void UpdateJudgement(int timing, GroupProperties groupProperties)
        {
            if (locked && !tapJudgementRequestSent)
            {
                RequestTapJudgement();
                tapJudgementRequestSent = true;
            }

            if (timing >= Timing && !holdJudgementRequestSent)
            {
                RequestHoldJudgement(timing);
                holdJudgementRequestSent = true;
            }
        }

        public void UpdateInstance(int timing, double floorPosition, GroupProperties groupProperties)
        {
            if (instance == null)
            {
                return;
            }

            float z = ZPos(floorPosition);
            float endZ = EndZPos(floorPosition);

            Vector3 pos = (groupProperties.FallDirection * z) + new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0);
            Quaternion rot = groupProperties.RotationIndividual;
            Vector3 scl = groupProperties.ScaleIndividual;
            scl.y *= z - endZ;
            instance.SetTransform(pos, rot, scl);
            instance.SetFallDirection(groupProperties.FallDirection);

            float alpha = 1;
            if (Highlight)
            {
                flashCount = (flashCount + 1) % Values.HoldFlashCycle;
                if (flashCount == 0)
                {
                    alpha = Values.FlashHoldAlphaScalar;
                }
            }
            else
            {
                if (timing >= Timing)
                {
                    alpha = Values.MissedHoldAlphaScalar;
                }
            }

            alpha *= Values.MaxHoldAlpha;

            Color color = groupProperties.Color;
            color.a *= alpha;
            instance.SetColor(color);

            if (!locked)
            {
                instance.SetFrom((float)((floorPosition - FloorPosition) / (EndFloorPosition - FloorPosition)));
            }
            else
            {
                instance.SetFrom(0);
            }

            if (Highlight && timing <= EndTiming)
            {
                Services.Particle.PlayLongParticle(this, new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0));
            }
        }

        public int CompareTo(INote<HoldBehaviour> other)
        {
            LongNote note = other as LongNote;
            if (note.Timing == Timing)
            {
                return EndTiming.CompareTo(note.EndTiming);
            }

            return Timing.CompareTo(note.EndTiming);
        }

        public void ProcessLaneTapJudgement(int offset)
        {
            int currentTiming = Services.Audio.Timing;
            if (currentTiming >= EndTiming)
            {
                return;
            }

            locked = false;
            OnHit(currentTiming);
            tapJudgementRequestSent = false;

            // Extend the note back
            if (currentTiming < Timing)
            {
                FloorPosition = TimingGroupInstance.GetFloorPosition(currentTiming);
            }
        }

        public void ProcessLaneHoldJudgement(int offset)
        {
            int currentTiming = Services.Audio.Timing;
            holdJudgementRequestSent = false;

            if (locked || offset >= Values.HoldLostLateJudgeWindow)
            {
                Highlight = false;
                int missCount = UpdateCurrentJudgePointTiming(currentTiming - Values.HoldLostLateJudgeWindow);
                if (missCount > 0)
                {
                    Services.Score.ProcessJudgement(JudgementResult.LostLate, missCount);
                    PlayParticle(JudgementResult.LostLate);
                }

                return;
            }

            OnHit(currentTiming);
        }

        private void RequestTapJudgement()
        {
            Services.Judgement.Request(new LaneTapJudgementRequest()
            {
                ExpireAtTiming = EndTiming,
                AutoAtTiming = Timing,
                Lane = Lane,
                Receiver = this,
            });
        }

        private void RequestHoldJudgement(int currentTiming)
        {
            Services.Judgement.Request(new LaneHoldJudgementRequest()
            {
                ExpireAtTiming = currentTiming + Values.HoldLostLateJudgeWindow,
                AutoAtTiming = currentTiming,
                Lane = Lane,
                Receiver = this,
            });
        }

        private void PlayParticle(JudgementResult result)
        {
            Services.Particle.PlayTextParticle(new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0), result);
        }

        private void OnHit(int currentTiming)
        {
            int hitCount = UpdateCurrentJudgePointTiming(currentTiming);
            if (hitCount > 0)
            {
                Services.Score.ProcessJudgement(JudgementResult.Max, hitCount);
                PlayParticle(JudgementResult.Max);
            }

            Services.InputFeedback.LaneFeedback(Lane);
            if (currentTiming <= EndTiming)
            {
                Services.Particle.PlayLongParticle(this, new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0));
            }

            Highlight = true;

            RequestHoldJudgement(currentTiming);
            holdJudgementRequestSent = true;
        }
    }
}