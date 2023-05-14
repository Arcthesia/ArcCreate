using System;
using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    public class GameplayBeatlineGenerator : IBeatlineGenerator
    {
        private readonly Color beatlineColor;

        public GameplayBeatlineGenerator(Color beatlineColor)
        {
            this.beatlineColor = beatlineColor;
        }

        public IEnumerable<Beatline> Generate(TimingGroup tg, int audioLength)
        {
            List<TimingEvent> timings = tg.Timings;

            // Extend before timing = 0;
            {
                TimingEvent firstTiming = timings[0];

                float start = -3000 - Values.ChartAudioOffset - Settings.GlobalAudioOffset.Value;

                if (firstTiming.Bpm <= 10000)
                {
                    double distanceBetweenTwoLine =
                        firstTiming.Bpm * firstTiming.Divisor == 0 ?
                        double.MaxValue :
                        60000f / Mathf.Abs(firstTiming.Bpm) * firstTiming.Divisor;
                    distanceBetweenTwoLine = Math.Max(distanceBetweenTwoLine, 1);

                    if (distanceBetweenTwoLine > 0)
                    {
                        int count = 0;
                        double timing = 0;
                        while (timing >= start)
                        {
                            int t = (int)Math.Round(timing);
                            yield return new Beatline(
                                t,
                                tg.GetFloorPosition(t),
                                Values.BeatlineThickness,
                                beatlineColor);
                            count++;
                            timing = -count * distanceBetweenTwoLine;
                        }
                    }
                }
            }

            for (int i = 0; i < timings.Count - 1; i++)
            {
                TimingEvent currentTiming = timings[i];
                int limit = timings[i + 1].Timing;

                if (currentTiming.Bpm > 10000)
                {
                    continue;
                }

                double distanceBetweenTwoLine =
                    currentTiming.Bpm * currentTiming.Divisor == 0 ?
                    double.MaxValue :
                    60000f / Mathf.Abs(currentTiming.Bpm) * currentTiming.Divisor;
                distanceBetweenTwoLine = Math.Max(distanceBetweenTwoLine, 1);

                if (distanceBetweenTwoLine <= 0)
                {
                    continue;
                }

                int count = 0;
                double timing = currentTiming.Timing;
                while (timing < limit)
                {
                    int t = (int)Math.Round(timing);
                    yield return new Beatline(
                        t,
                        tg.GetFloorPosition(t),
                        Values.BeatlineThickness,
                        beatlineColor);
                    count++;
                    timing = currentTiming.Timing + (distanceBetweenTwoLine * count);
                }
            }

            // Last timing event extend until end of audio
            {
                TimingEvent lastTiming = timings[timings.Count - 1];
                int limit = audioLength;

                if (lastTiming.Bpm <= 10000)
                {
                    double distanceBetweenTwoLine =
                        lastTiming.Bpm * lastTiming.Divisor == 0 ?
                        double.MaxValue :
                        60000f / Mathf.Abs(lastTiming.Bpm) * lastTiming.Divisor;
                    distanceBetweenTwoLine = Math.Max(distanceBetweenTwoLine, 1);

                    if (distanceBetweenTwoLine > 0)
                    {
                        int count = 0;
                        double timing = lastTiming.Timing;
                        while (timing <= limit)
                        {
                            int t = (int)Math.Round(timing);
                            yield return new Beatline(
                                t,
                                tg.GetFloorPosition(t),
                                Values.BeatlineThickness,
                                beatlineColor);
                            count++;
                            timing = lastTiming.Timing + (distanceBetweenTwoLine * count);
                        }
                    }
                }
            }
        }
    }
}