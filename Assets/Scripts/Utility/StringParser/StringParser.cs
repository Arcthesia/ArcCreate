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
            int end = ternimator != null ? str.IndexOf(ternimator, pos) : (str.Length - 1);
            float value = Evaluator.Float(str.Substring(pos, end - pos));
            pos += end - pos + 1;
            return value;
        }

        public int ReadInt(string ternimator = null)
        {
            int end = ternimator != null ? str.IndexOf(ternimator, pos) : (str.Length - 1);
            int value = Evaluator.Int(str.Substring(pos, end - pos));
            pos += end - pos + 1;
            return value;
        }

        public bool ReadBool(string ternimator = null)
        {
            int end = ternimator != null ? str.IndexOf(ternimator, pos) : (str.Length - 1);
            bool value = bool.Parse(str.Substring(pos, end - pos));
            pos += end - pos + 1;
            return value;
        }

        public string ReadString(string ternimator = null)
        {
            int end = ternimator != null ? str.IndexOf(ternimator, pos) : (str.Length - 1);
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