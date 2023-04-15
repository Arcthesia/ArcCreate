using System;
using System.Collections.Generic;

namespace ArcCreate.ChartFormat
{
    public class ChartFormatException : Exception
    {
        public ChartFormatException(string reason, int startCharPos = 0, int endCharPos = -1)
            : base(reason)
        {
            ShouldAbort = false;
            LineNumber = -1;
            Reason = reason;
            StartCharPos = startCharPos;
            EndCharPos = endCharPos;
        }

        public ChartFormatException(
            RawEventType type,
            string content,
            string file,
            int line,
            string reason,
            bool shouldAbort = false,
            int startCharPos = 0,
            int endCharPos = -1)
            : base(I18n.S(
                "Format.Exception.Message",
                new Dictionary<string, object>()
                {
                    { "File", file },
                    { "LineNumber", line + 1 },
                    { "Content", content },
                    { "EventType", type },
                    { "Reason", reason },
                }))
        {
            ShouldAbort = shouldAbort;
            LineNumber = line;
            Reason = reason;
            StartCharPos = startCharPos;
            EndCharPos = endCharPos;
        }

        public bool ShouldAbort { get; set; }

        public int LineNumber { get; set; }

        public int StartCharPos { get; set; }

        public int EndCharPos { get; set; }

        public string Reason { get; set; }
    }
}