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

        public string Current
        {
            get
            {
                return str[pos].ToString();
            }
        }

        public void Skip(int length)
        {
            pos += length;
        }

        public float ReadFloat(string ternimator = null)
        {
            int end = ternimator != null ? str.IndexOf(ternimator, pos) : str.Length;
            if (!Evaluator.TryFloat(str.Substring(pos, end - pos), out float value))
            {
                throw new System.ArgumentException(str.Substring(pos, end - pos));
            }

            pos += end - pos + 1;
            return value;
        }

        public int ReadInt(string ternimator = null)
        {
            int end = ternimator != null ? str.IndexOf(ternimator, pos) : str.Length;
            if (!Evaluator.TryInt(str.Substring(pos, end - pos), out int value))
            {
                throw new System.ArgumentException(str.Substring(pos, end - pos));
            }

            pos += end - pos + 1;
            return value;
        }

        public bool ReadBool(string ternimator = null)
        {
            int end = ternimator != null ? str.IndexOf(ternimator, pos) : str.Length;
            bool value = bool.Parse(str.Substring(pos, end - pos));
            pos += end - pos + 1;
            return value;
        }

        public string ReadString(string ternimator = null)
        {
            int end = ternimator != null ? str.IndexOf(ternimator, pos) : str.Length;
            string value = str.Substring(pos, end - pos);
            pos += end - pos + 1;
            return value;
        }

        public string Peek(int count)
        {
            return str.Substring(pos, count);
        }
    }
}