using System;

namespace ArcCreate.ChartFormat
{
    public class ChartFormatException : Exception
    {
        public ChartFormatException(string reason)
            : base(reason)
        {
        }

        public ChartFormatException(RawEventType type, string content, string file, int line)
            : base(I.S("Format.Exception.MessageUnknownReason", file, line, type, content))
        {
        }

        public ChartFormatException(RawEventType type, string content, string file, int line, string reason)
            : base(I.S("Format.Exception.Message", file, line, type, content, reason))
        {
        }
    }
}