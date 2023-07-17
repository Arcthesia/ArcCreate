namespace ArcCreate.Utility.Parser
{
    public class StringParser
    {
        private readonly string str;
        private int pos;

        public StringParser(string str)
        {
            this.str = str;
        }

        public char Current
        {
            get
            {
                return str[pos];
            }
        }

        public int Pos => pos;

        public bool HasEnded => pos >= str.Length;

        public void Skip(int length)
        {
            pos += length;
        }

        public Result<TextSpan<float>, ParsingError> ReadFloat(string terminator = null)
        {
            if (!ReadString(terminator).TryUnwrap(out TextSpan<string> s, out ParsingError e))
            {
                return e;
            }

            if (!Evaluator.TryFloat(s, out float value))
            {
                return new ParsingError(s, s.StartPos, s.Length, ParsingError.Kind.InvalidConversionToFloat);
            }

            return new TextSpan<float>(value, s.StartPos, s.Length);
        }

        public Result<TextSpan<int>, ParsingError> ReadInt(string terminator = null)
        {
            if (!ReadString(terminator).TryUnwrap(out TextSpan<string> s, out ParsingError e))
            {
                return e;
            }

            if (!Evaluator.TryInt(s, out int value))
            {
                return new ParsingError(s, s.StartPos, s.Length, ParsingError.Kind.InvalidConversionToInt);
            }

            return new TextSpan<int>(value, s.StartPos, s.Length);
        }

        public Result<TextSpan<bool>, ParsingError> ReadBool(string terminator = null)
        {
            if (!ReadString(terminator).TryUnwrap(out TextSpan<string> s, out ParsingError e))
            {
                return e;
            }

            if (!bool.TryParse(s, out bool value))
            {
                return new ParsingError(s, s.StartPos, s.Length, ParsingError.Kind.InvalidConversionToBool);
            }

            return new TextSpan<bool>(value, s.StartPos, s.Length);
        }

        public Result<TextSpan<string>, ParsingError> ReadString(string terminator = null)
        {
            int end = terminator != null ? str.IndexOf(terminator, pos) : str.Length;
            if (end == -1)
            {
                return new ParsingError(terminator, pos, str.Length - pos, ParsingError.Kind.CharacterNotFound);
            }

            string s = str.Substring(pos, end - pos);
            var result = new TextSpan<string>(s, pos, s.Length);
            pos = end + 1;
            return result;
        }
    }
}