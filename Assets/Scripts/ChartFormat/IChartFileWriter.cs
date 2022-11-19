using System.Collections.Generic;
using System.IO;

namespace Arc.ChartFormat
{
    public interface IChartFileWriter
    {
        void Write(StreamWriter stream, int audioOffset, float density, IEnumerable<(RawTimingGroup properties, IEnumerable<RawEvent> events)> groups);
    }
}