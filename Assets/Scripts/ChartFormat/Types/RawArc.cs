using System.Collections.Generic;

namespace Arc.ChartFormat
{
    public class RawArc : RawEvent
    {
        public int EndTiming { get; set; }

        public float XStart { get; set; }

        public float XEnd { get; set; }

        public string LineType { get; set; }

        public float YStart { get; set; }

        public float YEnd { get; set; }

        public int Color { get; set; }

        public bool IsTrace { get; set; }

        public string Sfx { get; set; }

        public List<int> ArcTaps { get; set; }
    }
}