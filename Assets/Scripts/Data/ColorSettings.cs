using System;
using System.Collections.Generic;

namespace ArcCreate.Data
{
    public class ColorSettings
    {
        public string Trace { get; set; } = null;

        public string Shadow { get; set; } = null;

        public List<string> Arc { get; set; } = new List<string>();

        public List<string> ArcLow { get; set; } = new List<string>();

        public ColorSettings Clone()
        {
            return new ColorSettings
            {
                Trace = Trace,
                Shadow = Shadow,
                Arc = new List<string>(Arc),
                ArcLow = new List<string>(ArcLow),
            };
        }
    }
}