using System;
using ArcCreate.ChartFormat.Grammar;

namespace ArcCreate.ChartFormat
{
    public class ChartReaderValidator
    {
        private AntlrEvent Event { get; }
        private RawEventType EventType { get; }

        public ChartReaderValidator(AntlrEvent evt, RawEventType eventType)
        {
            Event = evt;
            EventType = eventType;
        }

        public void Require(bool condition, string raw = null, ChartError.Kind errorKind = ChartError.Kind.Parsing)
        {
            if (!condition)
            {
                throw new ChartReaderException(raw ?? Event.Raw, EventType, Event, errorKind);
            }
        }
    }

    public class ChartReaderException : Exception, IAntlrPositionTrack
    {
        public string Raw { get; }
        public RawEventType EventType { get; }
        public ChartError.Kind ErrorKind { get; }

        public int LineNumber { get; }
        public int ColumnNumber { get; }

        public ChartReaderException(string raw, RawEventType type, int lineNumber, int columnNumber, ChartError.Kind errorKind)
        {
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;

            Raw = raw;
            EventType = type;
            ErrorKind = errorKind;
        }

        public ChartReaderException(string raw, RawEventType type, AntlrEvent evt, ChartError.Kind errorKind) : this(raw, type,
            evt.LineNumber, evt.ColumnNumber, errorKind)
        {
        }
    }
}