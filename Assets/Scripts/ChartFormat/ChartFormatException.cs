using System;
using System.Collections.Generic;

namespace ArcCreate.ChartFormat
{
    public class ChartFormatException : Exception
    {
        public ChartFormatException(string reason)
            : base(reason)
        {
        }

        public ChartFormatException(RawEventType type, string content, string file, int line)
            : base(I18n.S(
                "Format.Exception.MessageUnknownReason",
                new Dictionary<string, object>()
                {
                    { "File", file },
                    { "LineNumber", line },
                    { "Content", content },
                    { "EventType", type },
                }))
        {
        }

        public ChartFormatException(RawEventType type, string content, string file, int line, string reason)
            : base(I18n.S(
                "Format.Exception.Message",
                new Dictionary<string, object>()
                {
                    { "File", file },
                    { "LineNumber", line },
                    { "Content", content },
                    { "EventType", type },
                    { "Reason", reason },
                }))
        {
        }
    }
}