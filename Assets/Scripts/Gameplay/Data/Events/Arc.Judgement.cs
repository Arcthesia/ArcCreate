using System.Collections.Generic;
using System.Linq;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Gameplay.Utility;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    /// <summary>
    /// Partial class for judgement.
    /// </summary>
    public partial class Arc : LongNote, ILongNote, IArcJudgementReceiver
    {
        private int numJudgementRequestsSent = 0;
        private bool highlightRequestSent = false;
        private bool spawnedParticleThisFrame = false;

        public void ResetJudgeTo(int timing)
        {
            RecalculateJudgeTimings();
            highlight = highlight && timing >= Timing && timing <= EndTiming;
            longParticleUntil = int.MinValue;
            numJudgementRequestsSent = ComboAt(timing);
            highlightRequestSent = false;
            arcGroupAlpha = 1;
            hasBeenHitOnce = hasBeenHitOnce && timing >= Timing && timing <= EndTiming;
            for (int i = 0; i < segments.Count; i++)
            {
                ArcSegmentData segment = segments[i];
                segment.From = 0;
                segments[i] = segment;
            }
        }

        public override void RecalculateJudgeTimings()
        {
            TotalCombo = 0;
            FirstJudgeTime = double.MaxValue;
            TimeIncrement = double.MaxValue;

            if (IsTrace || EndTiming == Timing)
            {
                return;
            }

            double bpm = TimingGroupInstance.GetBpm(Timing);

            if (bpm == 0)
            {
                return;
            }

            int duration = EndTiming - Timing;
            bpm = System.Math.Abs(bpm);
            TimeIncrement = (bpm >= 255 ? 60_000 : 30_000) / bpm / Values.TimingPointDensity;

            int count = (int)(duration / TimeIncrement);
            int whatTheFuckDoesThisMean = (IsFirstArcOfGroup ? 0 : 1) ^ 1;
            if (count <= whatTheFuckDoesThisMean)
            {
                TotalCombo = 1;
                FirstJudgeTime = Timing + (duration / 2);
            }
            else
            {
                TotalCombo = count - whatTheFuckDoesThisMean;
                FirstJudgeTime = Timing + (whatTheFuckDoesThisMean * TimeIncrement);
            }
        }

        public void UpdateJudgement(int currentTiming, GroupProperties groupProperties)
        {
            if (!IsTrace && currentTiming >= Timing && Timing < EndTiming)
            {
                RequestJudgement();
            }

            if (!IsTrace && currentTiming >= Timing && Timing < EndTiming && !highlightRequestSent)
            {
                RequestHighlight(currentTiming);
                highlightRequestSent = true;
            }

            spawnedParticleThisFrame = false;
        }

        public void ProcessArcJudgement(bool isExpired, bool isJudgement)
        {
            int currentTiming = Services.Audio.ChartTiming;

            if (!isJudgement && currentTiming <= EndTiming)
            {
                RequestHighlight(currentTiming);
            }

            float x = WorldXAt(currentTiming);
            float y = WorldYAt(currentTiming);
            Vector3 currentPos = new Vector3(x, y);

            if (isExpired)
            {
                SetGroupHighlight(false, int.MinValue);

                if (isJudgement)
                {
                    if (!spawnedParticleThisFrame)
                    {
                        Services.Particle.PlayTextParticle(currentPos, JudgementResult.MissLate);
                        spawnedParticleThisFrame = true;
                    }

                    Services.Score.ProcessJudgement(JudgementResult.MissLate);
                }
            }
            else if (currentTiming <= EndTiming + Values.HoldMissLateJudgeWindow)
            {
                SetGroupHighlight(true, currentTiming + Values.HoldParticlePersistDuration);
                if (!hasBeenHitOnce)
                {
                    Services.Hitsound.PlayArcHitsound();
                }

                hasBeenHitOnce = true;

                if (isJudgement)
                {
                    if (!spawnedParticleThisFrame)
                    {
                        Services.Particle.PlayTextParticle(currentPos, JudgementResult.Max);
                        spawnedParticleThisFrame = true;
                    }

                    Services.Score.ProcessJudgement(JudgementResult.Max);
                }
            }
        }

        private void RequestJudgement()
        {
            for (int t = numJudgementRequestsSent; t < TotalCombo; t++)
            {
                int timing = (int)System.Math.Round(FirstJudgeTime + (t * TimeIncrement));
                Services.Judgement.Request(new ArcJudgementRequest()
                {
                    StartAtTiming = timing - Values.GoodJudgeWindow,
                    ExpireAtTiming = timing + Values.HoldMissLateJudgeWindow,
                    AutoAtTiming = timing,
                    Arc = this,
                    IsJudgement = true,
                    Receiver = this,
                });
            }

            numJudgementRequestsSent = TotalCombo;
        }

        private void RequestHighlight(int timing)
        {
            Services.Judgement.Request(new ArcJudgementRequest()
            {
                StartAtTiming = timing,
                ExpireAtTiming = timing + Values.HoldHighlightPersistDuration,
                AutoAtTiming = timing,
                Arc = this,
                IsJudgement = false,
                Receiver = this,
            });
        }
    }
}
