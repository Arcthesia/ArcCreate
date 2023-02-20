using System.Collections.Generic;
using ArcCreate.ChartFormat;

namespace ArcCreate.Gameplay.Data
{
    public class ChartTimingGroup
    {
        public RawTimingGroup Properties { get; set; }

        public List<Tap> Taps { get; set; } = new List<Tap>();

        public List<Hold> Holds { get; set; } = new List<Hold>();

        public List<Arc> Arcs { get; set; } = new List<Arc>();

        public List<ArcTap> ArcTaps { get; set; } = new List<ArcTap>();

        public List<TimingEvent> Timings { get; set; } = new List<TimingEvent>();

        public List<ArcEvent> ReferenceEvents { get; set; } = new List<ArcEvent>();
    }
}
