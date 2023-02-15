using System;
using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    /// <summary>
    /// Contains methods related to timing events.
    /// </summary>
    public partial class TimingGroup
    {
        private List<TimingEvent> timings = new List<TimingEvent>();

        public List<TimingEvent> Timings => timings;

        public TimingEvent GetEventAt(int timing)
        {
            int index = timings.BisectRight(timing, ev => ev.Timing) - 1;
            index = Mathf.Max(index, 0);
            return timings[index];
        }

        public float GetBpm(int timing)
        {
            return GetEventAt(timing).Bpm;
        }

        public float GetCurrentBpm()
        {
            return GetBpm(Services.Audio.ChartTiming);
        }

        public double GetFloorPosition(int timing)
        {
            TimingEvent note = GetEventAt(timing);
            double baseFloorPosition = note.FloorPosition;
            return baseFloorPosition + ((double)note.Bpm * (timing - note.Timing));
        }

        public float GetDivisor(int timing)
        {
            return GetEventAt(timing).Divisor;
        }

        /// <summary>
        /// Reverses the z world position to the timing value (relative to current chart timing).
        /// </summary>
        /// <param name="z">The z position.</param>
        /// <returns>The timing value corresponding to the value.</returns>
        public int GetTimingFromZPosition(float z)
        {
            double fp = ArcFormula.ZToFloorPosition(z);
            double currentFp = GetFloorPosition(Services.Audio.ChartTiming);
            return GetTimingFromFloorPosition(fp + currentFp);
        }

        /// <summary>
        /// Get the timing value corresponding to a floor position value.
        /// In the case that there are multiple valid timing values, the one closest to the current audio timing will be returned.
        /// </summary>
        /// <param name="fp">The floor position value.</param>
        /// <returns>The timing value.</returns>
        public int GetTimingFromFloorPosition(double fp)
        {
            int length = timings.Count;
            TimingEvent first = timings[0];

            int timing = Services.Audio.ChartTiming;
            int closestMatch = 0;
            int closestDiff = int.MaxValue;

            for (int i = 0; i < length - 1; i++)
            {
                TimingEvent curr = timings[i];
                TimingEvent next = timings[i + 1];

                // Floor position sandwiched between two timing events
                if ((curr.FloorPosition <= fp && next.FloorPosition > fp)
                 || (curr.FloorPosition >= fp && next.FloorPosition < fp))
                {
                    int val = (int)(Math.Round((fp - curr.FloorPosition) / curr.Bpm) + curr.Timing);
                    int diff = Mathf.Abs(val - timing);
                    if (diff < closestDiff)
                    {
                        closestDiff = diff;
                        closestMatch = val;
                    }
                }
            }

            TimingEvent last = timings[length - 1];
            {
                if ((last.FloorPosition <= fp && last.Bpm > 0)
                 || (last.FloorPosition >= fp && last.Bpm < 0))
                {
                    int val = (int)Math.Round(((fp - last.FloorPosition) / last.Bpm) + last.Timing);
                    int diff = Mathf.Abs(val - timing);
                    if (diff < closestDiff)
                    {
                        closestDiff = diff;
                        closestMatch = val;
                    }
                }
            }

            return Mathf.Clamp(closestMatch, 0, Services.Audio.AudioLength);
        }

        /// <summary>
        /// Get the floor position relative to the current audio timing.
        /// </summary>
        /// <param name="timing">The timing to calculate from.</param>
        /// <returns>The floor position.</returns>
        public double GetFloorPositionFromCurrent(int timing)
        {
            double fp = GetFloorPosition(timing);
            double curfp = GetFloorPosition(Services.Audio.ChartTiming);
            return fp - curfp;
        }

        private void AddTimings(IEnumerable<TimingEvent> timings)
        {
            this.timings.AddRange(timings);
            OnTimingListChange();
        }

        private void RemoveTimings(IEnumerable<TimingEvent> timings)
        {
            HashSet<TimingEvent> exclusion = new HashSet<TimingEvent>(timings);
            this.timings.RemoveAll(t => exclusion.Contains(t));
            OnTimingListChange();
        }

        private void UpdateTimings()
        {
            OnTimingListChange();
        }

        private void OnTimingListChange()
        {
            Sort();
            RecalculateFloorPosition();
            RecalculateNoteFloorPosition();
            if (GroupNumber == 0)
            {
                Services.Chart.ReloadBeatline();
            }
        }

        private void Sort()
        {
            timings.Sort((a, b) => a.Timing.CompareTo(b.Timing));
        }

        private void RecalculateFloorPosition()
        {
            double floorPosition = 0;
            for (int i = 0; i < timings.Count - 1; i++)
            {
                TimingEvent curr = timings[i];
                TimingEvent next = timings[i + 1];
                curr.FloorPosition = floorPosition;
                floorPosition += (double)curr.Bpm * (next.Timing - curr.Timing);
            }

            timings[timings.Count - 1].FloorPosition = floorPosition;
        }

        private void RecalculateNoteFloorPosition()
        {
            taps.Notes.ForEach(n =>
            {
                n.Rebuild();
            });
            taps.RebuildList();

            arcTaps.Notes.ForEach(n =>
            {
                n.Rebuild();
            });
            arcTaps.RebuildList();

            holds.Notes.ForEach(n =>
            {
                n.Rebuild();
            });
            holds.RebuildList();

            arcs.Notes.ForEach(n =>
            {
                n.Rebuild();
            });
            arcs.RebuildList();
        }

        private IEnumerable<TimingEvent> FindTimingEventsByTiming(int timing)
        {
            int i = timings.BisectLeft(timing, n => n.Timing);

            while (i >= 0 && i < timings.Count && timings[i].Timing == timing)
            {
                yield return timings[i];
                i++;
            }
        }

        private IEnumerable<TimingEvent> FindTimingEventsWithinRange(int from, int to)
        {
            int fromI = timings.BisectLeft(from, n => n.Timing);
            int toI = timings.BisectRight(to, n => n.Timing);

            for (int i = fromI; i <= toI; i++)
            {
                yield return timings[i];
            }
        }
    }
}