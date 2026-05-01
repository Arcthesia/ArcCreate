using System.Collections.Generic;
using Antlr4.Runtime;

namespace ArcCreate.ChartFormat.Grammar
{
    public class AntlrEventSegment : IAntlrPositionTrack
    {
        public IEnumerable<AntlrEvent> Events { get; }

        public int LineNumber { get; }
        public int ColumnNumber { get; }

        public AntlrEventSegment(IEnumerable<AntlrEvent> events, int lineNumber, int columnNumber)
        {
            Events = events;
            
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public AntlrEventSegment(IEnumerable<AntlrEvent> events, IToken start) : this(events, start.Line, start.Column)
        {
        }

        public AntlrEventSegment(IEnumerable<AntlrEvent> events, IToken start, IToken parentStart) : this(events,
            start.Line, start.Column + parentStart.Column)
        {
        }
    }
}