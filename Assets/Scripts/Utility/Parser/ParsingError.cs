namespace ArcCreate.Utility.Parser
{
    public class ParsingError : Error
    {
        public ParsingError(string cause, int startCharPos, int length, Kind kind)
        {
            Cause = cause;
            StartCharPos = startCharPos;
            Length = length;
            ErrorKind = kind;
        }

        public enum Kind
        {
            CharacterNotFound,
            InvalidConversionToInt,
            InvalidConversionToBool,
            InvalidConversionToFloat,
        }

        public string Cause { get; private set; }

        public int StartCharPos { get; private set; }

        public int Length { get; private set; }

        public override string Message
            => I18n.S($"Parsing.Exception.{ErrorKind}", Cause);

        public Kind ErrorKind { get; private set; }
    }
}