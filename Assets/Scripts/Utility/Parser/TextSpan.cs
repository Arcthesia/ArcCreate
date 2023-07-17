namespace ArcCreate.Utility.Parser
{
    public struct TextSpan<T>
    {
        public T Value;

        public int StartPos;

        public int Length;

        public TextSpan(T value, int startPos, int length)
        {
            Value = value;
            StartPos = startPos;
            Length = length;
        }

        public static implicit operator T(TextSpan<T> original) => original.Value;
    }
}