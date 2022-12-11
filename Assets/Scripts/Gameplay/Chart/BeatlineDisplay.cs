using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    public class BeatlineDisplay
    {
        private CachedBisect<Beatline, double> floorPositionSearch = new CachedBisect<Beatline, double>(new List<Beatline>(), x => x.FloorPosition);
        private Pool<Transform> beatlinePool;
        private readonly List<Beatline> previousBeatlinesInRange = new List<Beatline>(32);

        public void LoadFromTimingList()
        {
            beatlinePool?.ReturnAll();
            List<Beatline> beatlineFloorPosition = new List<Beatline>();

            TimingGroup tg = Services.Chart.GetTimingGroup(0);
            List<TimingEvent> timings = tg.Timings;

            // Extend before timing = 0;
            {
                TimingEvent firstTiming = timings[0];

                float start = -3000 - Values.ChartAudioOffset - Settings.GlobalAudioOffset.Value;

                float distanceBetweenTwoLine =
                    firstTiming.Bpm == 0 ?
                    float.MaxValue :
                    60000f / Mathf.Abs(firstTiming.Bpm) * firstTiming.Divisor;

                if (distanceBetweenTwoLine > 0)
                {
                    for (float timing = 0; timing >= start; timing -= distanceBetweenTwoLine)
                    {
                        beatlineFloorPosition.Add(new Beatline(tg.GetFloorPosition(Mathf.RoundToInt(timing))));
                    }
                }
            }

            for (int i = 0; i < timings.Count - 1; i++)
            {
                TimingEvent currentTiming = timings[i];
                int limit = timings[i + 1].Timing;

                float distanceBetweenTwoLine =
                    currentTiming.Bpm == 0 ?
                    float.MaxValue :
                    60000f / Mathf.Abs(currentTiming.Bpm) * currentTiming.Divisor;

                if (distanceBetweenTwoLine == 0)
                {
                    continue;
                }

                for (float timing = currentTiming.Timing; timing < limit; timing += distanceBetweenTwoLine)
                {
                    beatlineFloorPosition.Add(new Beatline(tg.GetFloorPosition(Mathf.RoundToInt(timing))));
                }
            }

            // Last timing event extend until end of audio
            {
                TimingEvent lastTiming = timings[timings.Count - 1];
                int limit = Services.Audio.AudioLength;

                float distanceBetweenTwoLine =
                    lastTiming.Bpm == 0 ?
                    float.MaxValue :
                    60000f / Mathf.Abs(lastTiming.Bpm) * lastTiming.Divisor;

                if (distanceBetweenTwoLine > 0)
                {
                    for (float timing = lastTiming.Timing; timing < limit; timing += distanceBetweenTwoLine)
                    {
                        beatlineFloorPosition.Add(new Beatline(tg.GetFloorPosition(Mathf.RoundToInt(timing))));
                    }
                }
            }

            // Build list
            floorPositionSearch = new CachedBisect<Beatline, double>(beatlineFloorPosition, x => x.FloorPosition);
            beatlinePool = Pools.Get<Transform>(Values.BeatlinePoolName);
        }

        public void UpdateBeatlines(double floorPosition)
        {
            double fpDistForward = System.Math.Abs(ArcFormula.ZToFloorPosition(Values.TrackLengthForward));
            double fpDistBackward = System.Math.Abs(ArcFormula.ZToFloorPosition(Values.TrackLengthBackward));
            double renderFrom = floorPosition - fpDistBackward;
            double renderTo = floorPosition + fpDistForward;

            int renderIndex = floorPositionSearch.Bisect(renderFrom);

            // Disable old notes
            for (int i = 0; i < previousBeatlinesInRange.Count; i++)
            {
                Beatline beatline = previousBeatlinesInRange[i];
                if (beatline.FloorPosition < renderFrom || beatline.FloorPosition > renderTo)
                {
                    beatlinePool.Return(beatline.RevokeInstance());
                }
            }

            previousBeatlinesInRange.Clear();

            // Update notes
            while (renderIndex < floorPositionSearch.List.Count)
            {
                Beatline beatline = floorPositionSearch.List[renderIndex];
                if (beatline.FloorPosition > renderTo)
                {
                    break;
                }

                if (!beatline.IsAssignedInstance)
                {
                    beatline.AssignInstance(beatlinePool.Get());
                }

                beatline.UpdateInstance(floorPosition);
                renderIndex++;
                previousBeatlinesInRange.Add(beatline);
            }
        }
    }
}