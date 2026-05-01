using System;
using Antlr4.Runtime;

namespace ArcCreate.ChartFormat.Grammar
{
    public enum AntlrValueType
    {
        String,
        Algebraic,
        Integer,
        KeyValuePair
    }

    public class AntlrValue : IAntlrPositionTrack
    {
        public string Raw { get; }
        public AntlrValueType Type { get; }

        public int LineNumber { get; }
        public int ColumnNumber { get; }

        public Option<string> StringValue { private get; set; } = Option<string>.None();
        public Option<double> AlgebraicValue { private get; set; } = Option<double>.None();
        public Option<int> IntegerValue { private get; set; } = Option<int>.None();

        public Option<Tuple<string, AntlrValue>> KeyValuePair { private get; set; } =
            Option<Tuple<string, AntlrValue>>.None();

        public bool IsEmpty => TryGetStringValue(out string raw) && raw == "";
        public bool IsStringValue => StringValue.HasValue;
        public bool IsAlgebraicValue => AlgebraicValue.HasValue;
        public bool IsIntegerValue => IntegerValue.HasValue;
        public bool IsKeyValuePair => KeyValuePair.HasValue;

        public static AntlrValue GetEmpty(IToken start) =>
            new("", AntlrValueType.String, start.Line, start.Column) { StringValue = "" };

        public static AntlrValue GetEmpty(int lineNumber, int columnNumber) =>
            new("", AntlrValueType.String, lineNumber, columnNumber) { StringValue = "" };

        private AntlrValue(string raw, AntlrValueType type, int lineNumber, int columnNumber)
        {
            Raw = raw ?? throw new ArgumentNullException(nameof(raw));
            Type = type;

            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public static AntlrValue FromString(string raw, string value, int lineNumber, int columnNumber) =>
            new(raw, AntlrValueType.String, lineNumber, columnNumber)
            {
                StringValue = value
            };

        public static AntlrValue FromAlgebraic(string raw, double value, int lineNumber, int columnNumber) =>
            new(raw, AntlrValueType.Algebraic, lineNumber, columnNumber)
            {
                AlgebraicValue = value,
            };

        public static AntlrValue FromInteger(string raw, int value, int lineNumber, int columnNumber) =>
            new(raw, AntlrValueType.Integer, lineNumber, columnNumber)
            {
                IntegerValue = value,
            };

        public static AntlrValue FromKeyValuePair(string raw, Tuple<string, AntlrValue> value, int lineNumber,
            int columnNumber) =>
            new(raw, AntlrValueType.KeyValuePair, lineNumber, columnNumber)
            {
                KeyValuePair = value,
            };

        public string GetStringValue() =>
            IsStringValue ? StringValue.Value : throw new InvalidOperationException();

        public double GetAlgebraicValue()
        {
            if (IsAlgebraicValue) return AlgebraicValue.Value;
            if (IsIntegerValue) return IntegerValue.Value;

            throw new InvalidOperationException();
        }

        public int GetIntegerValue() =>
            IsIntegerValue ? IntegerValue.Value : throw new InvalidOperationException();

        public Tuple<string, AntlrValue> GetKeyValuePair() =>
            IsKeyValuePair ? KeyValuePair.Value : throw new InvalidOperationException();

        public bool TryGetStringValue(out string stringValue)
        {
            stringValue = StringValue.Value;
            return IsStringValue;
        }

        public bool TryGetAlgebraicValue(out double algebraicValue)
        {
            bool success = IsAlgebraicValue;

            if (success)
            {
                algebraicValue = AlgebraicValue.Value;
            }
            else
            {
                success = IsIntegerValue;
                algebraicValue = IntegerValue.Value;
            }

            return success;
        }

        public bool TryGetIntegerValue(out int integerValue)
        {
            integerValue = IntegerValue.Value;
            return IsIntegerValue;
        }

        public bool TryGetKeyValuePair(out Tuple<string, AntlrValue> keyValuePair)
        {
            keyValuePair = KeyValuePair.Value;
            return IsKeyValuePair;
        }

        public override string ToString() =>
            Type switch
            {
                AntlrValueType.String => $"Value(String, \"{StringValue.Value}\")",
                AntlrValueType.Algebraic => $"Value(Algebraic, {AlgebraicValue.Value})",
                AntlrValueType.Integer => $"Value(Integer, {IntegerValue.Value})",
                AntlrValueType.KeyValuePair => $"Value(KeyValuePair, {KeyValuePair.Value.Item1} → ...)",
                _ => "Value(unknown)"
            };
    }
}