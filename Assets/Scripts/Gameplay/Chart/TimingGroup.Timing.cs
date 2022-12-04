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
        // Maybe a cached bisect here would be better. Too lazy to implement one for BisectRight though.
        private List<TimingEvent> timings = new List<TimingEvent>();

        public TimingEvent GetEventAt(int timing)
        {
            int index = timings.BisectRight(timing, ev => ev.Timing);
            return timings[index];
        }

        public float GetBpm(int timing)
        {
            return GetEventAt(timing).Bpm;
        }

        public float GetCurrentBpm()
        {
            return GetBpm(Services.Audio.Timing);
        }

        public double GetFloorPosition(int timing)
        {
            TimingEvent note = GetEventAt(timing);
            double baseFloorPosition = note.FloorPosition;
            return baseFloorPosition + (note.Bpm * (timing - note.Timing));
        }

        public float GetDivisor(int timing)
        {
            return GetEventAt(timing).Divisor;
        }

        /// <summary>
        /// Reverses the z world position to the timing value.
        /// </summary>
        /// <param name="z">The z position.</param>
        /// <returns>The timing value corresponding to the value.</returns>
        public int GetTimingFromZPosition(float z) => GetTimingFromFloorPosition(ArcFormula.ZToFloorPosition(z));

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

            int timing = Services.Audio.Timing;
            int closestMatch = 0;
            int closestDiff = int.MaxValue;

            for (int i = 0; i < length - 1; i++)
            {
                TimingEvent curr = timings[i];
                TimingEvent next = timings[i + 1];

                // Floor position sandwiched between two timing events
                if ((curr.FloorPosition < fp && next.FloorPosition > fp)
                 || (curr.FloorPosition > fp && next.FloorPosition < fp))
                {
                    int val = (int)((Math.Round(fp - curr.FloorPosition) / curr.Bpm) + curr.Timing);
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
                int val = (int)Math.Round(((fp - last.FloorPosition) / last.Bpm) + last.Timing);
                int diff = Mathf.Abs(val - timing);
                if (diff < closestDiff)
                {
                    closestDiff = diff;
                    closestMatch = val;
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
            return GetFloorPosition(timing) - GetFloorPosition(Services.Audio.Timing);
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
                floorPosition += curr.Bpm * (next.Timing - curr.Timing);
            }

            timings[timings.Count - 1].FloorPosition = floorPosition;
        }

        private void RecalculateNoteFloorPosition()
        {
            taps.Notes.ForEach(n =>
            {
                n.TimingGroupInstance = this;
                n.FloorPosition = GetFloorPosition(n.Timing);
            });
            taps.RebuildList();

            // holds.Notes.ForEach(n => n.RecalculateJudgeTimings());
            // arcs.Notes.ForEach(n => n.RecalculateJudgeTimings());
        }
    }
}