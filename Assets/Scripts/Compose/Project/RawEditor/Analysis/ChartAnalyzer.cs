using System.Collections.Generic;
using System.Threading;
using ArcCreate.ChartFormat;
using ArcCreate.Compose.Popups;

namespace ArcCreate.Compose.Project
{
    public class ChartAnalyzer
    {
        private readonly Queue<ChartFault> faultQueue = new Queue<ChartFault>();
        private Thread analysisThread;

        public bool IsComplete { get; set; } = true;

        public bool CheckQueue(out ChartFault fault)
        {
            if (faultQueue.Count > 0)
            {
                fault = faultQueue.Dequeue();
                return true;
            }

            fault = default;
            return false;
        }

        public bool PeekQueue(out ChartFault fault)
        {
            if (faultQueue.Count > 0)
            {
                fault = faultQueue.Peek();
                return true;
            }

            fault = default;
            return false;
        }

        public void Stop()
        {
            analysisThread?.Abort();
            analysisThread = null;
        }

        public void Start(string chart, string path)
        {
            IsComplete = false;
            analysisThread = new Thread(new ThreadStart(() => Analyze(chart, path)));
            analysisThread.Start();
        }

        private void Analyze(string chart, string path)
        {
            int i = 0;
            int lineNumber = 0;
            bool atHeader = true;
            ChartReader reader = ChartReaderFactory.GetReader(new VirtualFileAccess(chart), path);

            while (true)
            {
                int nextLineBreak = chart.IndexOf('\n', i);
                if (nextLineBreak < 0 || nextLineBreak >= chart.Length)
                {
                    IsComplete = true;
                    break;
                }

                string line = chart.Substring(i, nextLineBreak - i).Trim();
                i = nextLineBreak + 1;
                lineNumber++;

                try
                {
                    if (atHeader)
                    {
                        reader.ParseHeaderLine(line, path, lineNumber, out bool endOfHeader);
                        if (endOfHeader)
                        {
                            atHeader = false;
                        }
                    }
                    else
                    {
                        reader.ParseLine(line, path, lineNumber);
                    }
                }
                catch (ChartFormatException ex)
                {
                    faultQueue.Enqueue(new ChartFault
                    {
                        Severity = Severity.Error,
                        LineNumber = ex.LineNumber,
                        StartCharPos = ex.StartCharPos,
                        EndCharPos = ex.EndCharPos,
                        Description = ex.Reason,
                    });

                    if (ex.ShouldAbort)
                    {
                        IsComplete = true;
                        return;
                    }
                }
            }

            try
            {
                reader.FinalValidity();
            }
            catch (ChartFormatException ex)
            {
                faultQueue.Enqueue(new ChartFault
                {
                    Severity = Severity.Error,
                    LineNumber = ex.LineNumber,
                    StartCharPos = ex.StartCharPos,
                    EndCharPos = ex.EndCharPos,
                    Description = ex.Reason,
                });

                if (ex.ShouldAbort)
                {
                    IsComplete = true;
                    return;
                }
            }
        }
    }
}