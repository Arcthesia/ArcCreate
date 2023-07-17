using ArcCreate.Utility.Parser;

namespace ArcCreate.ChartFormat
{
    /// <summary>
    /// Error class for one single error within a file.
    /// </summary>
    public class ChartError : Error
    {
        private string message;

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
        }

        public Option<string> LineContent { get; private set; }

        public Option<int> LineNumber { get; private set; }

        public RawEventType EventType { get; private set; }

        public Option<int> StartPosition { get; private set; }

        public Option<int> Length { get; private set; }

        public Kind ErrorKind { get; private set; }

        public override string Message => message;

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
                message = I18n.S($"Format.Exception.{kind}"),
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
                message = I18n.S($"Format.Exception.{kind}"),
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
                message = parsingError.Message,
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
                message = I18n.S($"Format.Exception.ReferencedFileError", errors.Message),
            };
        }
    }
}