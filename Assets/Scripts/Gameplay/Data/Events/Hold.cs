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
        private bool holdHighlightRequestSent = false;
        private int longParticleUntil = int.MinValue;
        private bool isSelected;
        private int numHoldJudgementRequestsSent = 0;
        private bool spawnedParticleThisFrame = false;

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
            instance.SetSelected(isSelected);
            ReloadSkin();
        }

        public HoldBehaviour RevokeInstance()
        {
            var result = instance;
            instance = null;
            return result;
        }

        public void ResetJudgeTo(int timing)
        {
            RecalculateJudgeTimings();
            locked = true;
            Highlight = false;
            longParticleUntil = int.MinValue;
            tapJudgementRequestSent = false;
            numHoldJudgementRequestsSent = ComboAt(timing);
            holdHighlightRequestSent = false;
            FloorPosition = TimingGroupInstance.GetFloorPosition(Timing);
        }

        public override void RecalculateJudgeTimings()
        {
            TotalCombo = 0;
            double bpm = TimingGroupInstance.GetBpm(Timing);

            if (bpm == 0 || EndTiming == Timing)
            {
                FirstJudgeTime = double.MaxValue;
                TimeIncrement = double.MaxValue;
                return;
            }

            int duration = EndTiming - Timing;
            bpm = System.Math.Abs(bpm);
            TimeIncrement = (bpm >= 255 ? 60_000 : 30_000) / bpm / Values.TimingPointDensity;

            int count = (int)(duration / TimeIncrement);
            if (count <= 1)
            {
                TotalCombo = 1;
                FirstJudgeTime = Timing + (duration / 2);
            }
            else
            {
                TotalCombo = count - 1;
                FirstJudgeTime = Timing + TimeIncrement;
            }
        }

        public void Rebuild()
        {
            RecalculateFloorPosition();
            RecalculateJudgeTimings();
        }

        public void ReloadSkin()
        {
            var (normal, highlight) = Services.Skin.GetHoldSkin(this);
            instance.SetSprite(normal, highlight);
        }

        public void UpdateJudgement(int currentTiming, GroupProperties groupProperties)
        {
            if (currentTiming >= Timing - Values.LostJudgeWindow && locked && !tapJudgementRequestSent)
            {
                RequestTapJudgement();
                tapJudgementRequestSent = true;
            }

            if (currentTiming >= Timing)
            {
                RequestHoldJudgement();
            }

            if (currentTiming >= Timing && !holdHighlightRequestSent)
            {
                RequestHoldHighlight(currentTiming);
                holdHighlightRequestSent = true;
            }

            spawnedParticleThisFrame = false;
        }

        public void UpdateInstance(int currentTiming, double currentFloorPosition, GroupProperties groupProperties)
        {
            if (instance == null)
            {
                return;
            }

            float z = ZPos(currentFloorPosition);
            float endZ = EndZPos(currentFloorPosition);
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
                if (currentTiming >= Timing)
                {
                    alpha = Values.MissedHoldAlphaScalar;
                }
            }

            alpha *= Values.MaxHoldAlpha;

            Color color = groupProperties.Color;
            color.a *= alpha;
            instance.SetColor(color);

            if (!locked || groupProperties.NoInput)
            {
                instance.SetFrom((float)((currentFloorPosition - FloorPosition) / (EndFloorPosition - FloorPosition)));
            }
            else
            {
                instance.SetFrom(0);
            }

            if (currentTiming <= longParticleUntil && currentTiming <= EndTiming)
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
            int currentTiming = Services.Audio.ChartTiming;
            if (currentTiming >= EndTiming)
            {
                return;
            }

            locked = false;
            tapJudgementRequestSent = false;

            longParticleUntil = currentTiming + Values.HoldParticlePersistDuration;
            Highlight = true;
            Services.InputFeedback.LaneFeedback(Lane);
            Services.Particle.PlayLongParticle(this, new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0));

            // Extend the note back
            if (currentTiming < Timing)
            {
                FloorPosition = TimingGroupInstance.GetFloorPosition(currentTiming);
            }
        }

        public void ProcessLaneHoldJudgement(bool isExpired, bool isJudgement)
        {
            int currentTiming = Services.Audio.ChartTiming;
            if (!isJudgement)
            {
                holdHighlightRequestSent = false;
            }

            if (locked || isExpired)
            {
                longParticleUntil = int.MinValue;
                Highlight = false;
                if (isJudgement)
                {
                    Services.Score.ProcessJudgement(JudgementResult.LostLate);
                    if (!spawnedParticleThisFrame)
                    {
                        PlayParticle(JudgementResult.LostLate);
                        spawnedParticleThisFrame = true;
                    }
                }
            }
            else
            {
                longParticleUntil = currentTiming + Values.HoldParticlePersistDuration;
                Highlight = true;
                if (isJudgement)
                {
                    Services.Score.ProcessJudgement(JudgementResult.Max);
                    if (!spawnedParticleThisFrame)
                    {
                        PlayParticle(JudgementResult.Max);
                        spawnedParticleThisFrame = true;
                    }
                }
            }
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

        private void RequestHoldJudgement()
        {
            for (int t = numHoldJudgementRequestsSent; t < TotalCombo; t++)
            {
                int timing = (int)System.Math.Round(FirstJudgeTime + (t * TimeIncrement));

                Services.Judgement.Request(new LaneHoldJudgementRequest()
                {
                    StartAtTiming = timing - Values.FarJudgeWindow,
                    ExpireAtTiming = timing + Values.HoldLostLateJudgeWindow,
                    AutoAtTiming = timing,
                    Lane = Lane,
                    IsJudgement = true,
                    Receiver = this,
                });
            }

            numHoldJudgementRequestsSent = TotalCombo;
        }

        private void RequestHoldHighlight(int timing)
        {
            Services.Judgement.Request(new LaneHoldJudgementRequest()
            {
                StartAtTiming = timing,
                ExpireAtTiming = timing + Values.HoldHighlightPersistDuration,
                AutoAtTiming = timing,
                Lane = Lane,
                IsJudgement = false,
                Receiver = this,
            });
        }

        private void PlayParticle(JudgementResult result)
        {
            Services.Particle.PlayTextParticle(new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0), result);
        }
    }
}