using System;
using System.Collections.Generic;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    public class EditorBeatlineGenerator : IBeatlineGenerator
    {
        private readonly List<BeatlineColorSetting> beatlineColorSettings;
        private readonly Color defaultColor;
        private readonly Color barColor;

        public EditorBeatlineGenerator(List<BeatlineColorSetting> beatlineColorSettings, Color defaultColor, Color barColor)
        {
            this.beatlineColorSettings = beatlineColorSettings;
            beatlineColorSettings.Sort((a, b) => a.Density.CompareTo(b.Density));
            this.defaultColor = defaultColor;
            this.barColor = barColor;
        }

        public IEnumerable<Beatline> Generate(TimingGroup tg, int audioLength)
        {
            List<TimingEvent> timings = tg.Timings;
            for (int i = 0; i < timings.Count - 1; i++)
            {
                TimingEvent currentTiming = timings[i];
                int limit = timings[i + 1].Timing;

                if (currentTiming.Bpm > Settings.GridBpmLimit.Value)
                {
                    continue;
                }

                double distanceBetweenTwoLine =
                    currentTiming.Bpm * Values.BeatlineDensity.Value == 0 ?
                    double.MaxValue :
                    60000f / Mathf.Abs(currentTiming.Bpm) / Values.BeatlineDensity.Value;
                distanceBetweenTwoLine = Math.Max(distanceBetweenTwoLine, 1);

                int count = 0;
                double timing = currentTiming.Timing;
                while (timing < limit)
                {
                    Color beatlineColor = ResolveColor(count, Values.BeatlineDensity.Value);
                    int t = (int)Math.Round(timing);
                    yield return new Beatline(
                        t,
                        tg.GetFloorPosition(t),
                        Values.EditorBeatlineThickness,
                        beatlineColor);
                    count++;
                    timing = currentTiming.Timing + (distanceBetweenTwoLine * count);
                }
            }

            // Last timing event extend until end of audio
            {
                TimingEvent lastTiming = timings[timings.Count - 1];
                int limit = audioLength;

                if (lastTiming.Bpm > Settings.GridBpmLimit.Value)
                {
                    yield break;
                }

                double distanceBetweenTwoLine =
                    lastTiming.Bpm * Values.BeatlineDensity.Value == 0 ?
                    double.MaxValue :
                    60000f / Mathf.Abs(lastTiming.Bpm) / Values.BeatlineDensity.Value;
                distanceBetweenTwoLine = Math.Max(distanceBetweenTwoLine, 1);

                int count = 0;
                double timing = lastTiming.Timing;
                while (timing <= limit)
                {
                    Color beatlineColor = ResolveColor(count, Values.BeatlineDensity.Value);
                    int t = (int)Math.Round(timing);
                    yield return new Beatline(
                        t,
                        tg.GetFloorPosition(t),
                        Values.EditorBeatlineThickness,
                        beatlineColor);
                    count++;
                    timing = lastTiming.Timing + (distanceBetweenTwoLine * count);
                }
            }
        }

        private Color ResolveColor(int count, float beatlineDensity)
        {
            if (count % (beatlineDensity * beatlineDensity) == 0)
            {
                return barColor;
            }

            foreach (BeatlineColorSetting setting in beatlineColorSettings)
            {
                float density = setting.Density;
                Color color = setting.Color;

                if (count % (beatlineDensity / density) == 0)
                {
                    return color;
                }
            }

            return defaultColor;
        }
    }
}