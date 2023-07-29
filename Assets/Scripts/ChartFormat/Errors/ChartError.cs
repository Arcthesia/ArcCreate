using System.Collections.Generic;
using ArcCreate.Utility.Parser;

namespace ArcCreate.ChartFormat
{
    /// <summary>
    /// Error class for one single error within a file.
    /// </summary>
    public class ChartError : Error
    {
        private string reason;

        public enum Kind
        {
            Parsing,
            ReferencedFileError,
            DivisorNegative,
            BaseBpmZero,
            DurationNegative,
            ArcColorNegative,
            ArcTapOutOfRange,
            DurationZero,
            InvalidHeader,
            TimingGroupPairInvalid,
            IncludeReferencedMultipleTimes,
            IncludeAReferencedFragment,
            TimingGroupPropertiesInvalid,
            BaseTimingInvalid,
            FileDoesNotExist,
        }

        public Option<string> LineContent { get; private set; }

        public Option<int> LineNumber { get; private set; }

        public RawEventType EventType { get; private set; }

        public Option<int> StartPosition { get; private set; }

        public Option<int> Length { get; private set; }

        public Kind ErrorKind { get; private set; }

        public override string Message
            => LineNumber.HasValue && LineContent.HasValue ? MessageFull : MessageShort;

        public string MessageShort
            => I18n.S("Format.Exception.SingleErrorShortFormat", new Dictionary<string, object>()
            {
                { "EventType", EventType },
                { "Reason", reason },
            });

        public string MessageFull
            => I18n.S("Format.Exception.SingleErrorFormat", new Dictionary<string, object>()
            {
                { "LineNumber", LineNumber.Map(x => x.ToString()).Or("Unknown") },
                { "EventType", EventType },
                { "Content", LineContent.Or("Unknown") },
                { "Reason", reason },
            });

        public string Reason => reason;

        public static ChartError Property(string lineContent, int lineNumber, RawEventType eventType, Option<int> startPosition, Option<int> length, Kind kind)
        {
            return new ChartError
            {
                LineContent = lineContent,
                LineNumber = lineNumber,
                EventType = eventType,
                StartPosition = startPosition,
                Length = length,
                ErrorKind = kind,
                reason = I18n.S($"Format.Exception.{kind}"),
            };
        }

        public static ChartError Format(RawEventType eventType, Kind kind)
        {
            return new ChartError
            {
                LineContent = Option<string>.None(),
                LineNumber = Option<int>.None(),
                EventType = eventType,
                StartPosition = Option<int>.None(),
                Length = Option<int>.None(),
                ErrorKind = kind,
                reason = I18n.S($"Format.Exception.{kind}"),
            };
        }

        public static ChartError Parsing(string lineContent, int lineNumber, RawEventType eventType, ParsingError parsingError)
        {
            return new ChartError
            {
                LineContent = lineContent,
                LineNumber = lineNumber,
                EventType = eventType,
                StartPosition = parsingError.StartCharPos,
                Length = parsingError.Length,
                ErrorKind = Kind.Parsing,
                reason = parsingError.Message,
            };
        }

        public static ChartError ReferencedFile(string lineContent, int lineNumber, RawEventType eventType, ChartFileErrors errors)
        {
            return new ChartError
            {
                LineContent = lineContent,
                LineNumber = lineNumber,
                EventType = eventType,
                StartPosition = 0,
                Length = lineContent.Length,
                ErrorKind = Kind.ReferencedFileError,
                reason = I18n.S($"Format.Exception.ReferencedFileError", errors.Message),
            };
        }
    }
}