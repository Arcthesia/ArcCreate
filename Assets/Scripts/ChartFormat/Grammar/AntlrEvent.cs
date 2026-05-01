using System.Collections.Generic;
using Antlr4.Runtime;

namespace ArcCreate.ChartFormat.Grammar
{
    public class AntlrEvent : IAntlrPositionTrack
    {
        public string Raw { get; }

        public string Name { get; }
        public List<AntlrValue> Values { get; }
        public List<AntlrEvent> SubEvents { get; }
        public Dictionary<string, AntlrValue> Properties { get; }
        public AntlrEventSegment Segment { get; }

        public int LineNumber { get; }
        public int ColumnNumber { get; }

        public AntlrEvent(
            string raw,
            string name,
            List<AntlrValue> values,
            List<AntlrEvent> subEvents,
            Dictionary<string, AntlrValue> properties,
            AntlrEventSegment segment,
            int lineNumber,
            int columnNumber)
        {
            Raw = raw;
            Name = name;
            Values = values ?? new List<AntlrValue>();
            SubEvents = subEvents ?? new List<AntlrEvent>();
            Segment = segment;
            Properties = properties ?? new Dictionary<string, AntlrValue>();

            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public AntlrEvent(
            string raw,
            string name,
            List<AntlrValue> values,
            List<AntlrEvent> subEvents,
            Dictionary<string, AntlrValue> properties,
            AntlrEventSegment segment,
            IToken start
        ) : this(raw,
            name,
            values,
            subEvents,
            properties,
            segment,
            start.Line, start.Column)
        {
        }

        public AntlrEvent(
            string raw,
            string name,
            List<AntlrValue> values,
            List<AntlrEvent> subEvents,
            Dictionary<string, AntlrValue> properties,
            AntlrEventSegment segment,
            IToken start,
            IToken parentStart
        ) : this(raw,
            name,
            values,
            subEvents,
            properties,
            segment,
            start.Line, start.Column + parentStart.Column)
        {
        }

        public override string ToString() => Raw;
    }
}