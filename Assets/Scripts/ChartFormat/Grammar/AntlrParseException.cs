using System;
using Antlr4.Runtime;

namespace ArcCreate.ChartFormat.Grammar
{
    public class AntlrParseException : Exception, IAntlrPositionTrack
    {
        public string Raw { get; }
        public RawEventType EventType { get; }
        public int LineNumber { get; }
        public int ColumnNumber { get; }

        public AntlrParseException(string message, string raw, RawEventType eventType, int lineNumber, int columnNumber) :
            base(message)
        {
            Raw = raw;
            EventType = eventType;

            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public AntlrParseException(string message, string raw, RawEventType eventType, IToken start) : this(message, raw,
            eventType, start.Line, start.Column)
        {
        }

        public AntlrParseException(string message, string raw, RawEventType eventType, IToken start, IToken parentStart
        ) : this(message, raw, eventType, parentStart.Line, start.Column + parentStart.Column)
        {
        }
    }
}