using ArcCreate.Gameplay.Judgement;
using ArcCreate.Gameplay.Render;
using ArcCreate.Utility;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class Hold : LongNote, ILongNote, ILaneTapJudgementReceiver, ILaneHoldJudgementReceiver
    {
        private int flashCount;
        private bool highlight = false;
        private bool locked = true;
        private bool tapJudgementRequestSent = false;
        private bool holdHighlightRequestSent = false;
        private int longParticleUntil = int.MinValue;
        private int numHoldJudgementRequestsSent = 0;
        private bool spawnedParticleThisFrame = false;
        private Texture normalTexture;
        private Texture highlightTexture;

        public int Lane { get; set; }

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

        public void ResetJudgeTo(int timing)
        {
            RecalculateJudgeTimings();
            locked = true;
            highlight = false;
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
            (normalTexture, highlightTexture) = Services.Skin.GetHoldSkin(this);
        }

        public override Mesh GetColliderMesh()
        {
            return Services.Render.HoldMesh;
        }

        public override void GetColliderPosition(int timing, out Vector3 pos, out Vector3 scl)
        {
            double fp = TimingGroupInstance.GetFloorPosition(timing);
            float z = ZPos(fp);
            float endZ = EndZPos(fp);
            Vector3 basePos = new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0);
            pos = (TimingGroupInstance.GroupProperties.FallDirection * z) + basePos;
            scl = TimingGroupInstance.GroupProperties.ScaleIndividual;
            scl.z *= z - endZ;
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

        public void UpdateRender(int currentTiming, double currentFloorPosition, GroupProperties groupProperties)
        {
            if (normalTexture == null || highlightTexture == null)
            {
                ReloadSkin();
            }

            float z = ZPos(currentFloorPosition);
            float endZ = EndZPos(currentFloorPosition);
            Vector3 pos = (groupProperties.FallDirection * z) + new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0);
            Quaternion rot = groupProperties.RotationIndividual;
            Vector3 scl = groupProperties.ScaleIndividual;
            Matrix4x4 matrix = groupProperties.GroupMatrix
                             * Matrix4x4.TRS(pos, rot, scl)
                             * MatrixUtility.Shear(groupProperties.FallDirection * (z - endZ));

            Texture texture = highlight ? highlightTexture : normalTexture;
            float alpha = 1;
            if (highlight)
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

            float from = 0;
            if ((!locked || groupProperties.NoInput) && !groupProperties.NoClip)
            {
                from = (float)((currentFloorPosition - FloorPosition) / (EndFloorPosition - FloorPosition));
            }

            LongNoteRenderProperties properties = new LongNoteRenderProperties
            {
                From = from,
                Color = color,
                Selected = IsSelected ? 1 : 0,
            };

            Services.Render.DrawHold(texture, matrix, properties);

            if (currentTiming <= longParticleUntil && currentTiming <= EndTiming)
            {
                Services.Particle.PlayLongParticle(this, new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0));
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

        public void ProcessLaneTapJudgement(int offset)
        {
            int currentTiming = Services.Audio.ChartTiming;
            if (currentTiming >= EndTiming + Values.FarJudgeWindow)
            {
                return;
            }

            locked = false;
            tapJudgementRequestSent = false;

            longParticleUntil = currentTiming + Values.HoldParticlePersistDuration;
            highlight = true;
            Services.InputFeedback.LaneFeedback(Lane);
            Services.Particle.PlayLongParticle(this, new Vector3(ArcFormula.LaneToWorldX(Lane), 0, 0));
            Services.Hitsound.PlayTapHitsound();

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
                highlight = false;
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
                highlight = true;
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
                ExpireAtTiming = EndTiming + Values.FarJudgeWindow,
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