using System;
using System.Collections.Generic;
using System.Threading;
using ArcCreate.ChartFormat;
using ArcCreate.Compose.Popups;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Compose.Project
{
    public class ChartAnalyzer
    {
        private readonly Queue<ChartFault> faultQueue = new Queue<ChartFault>();
        private Thread analysisThread;

        public bool IsComplete { get; set; } = true;

        public bool HasError { get; set; } = false;

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

        public void Stop()
        {
            analysisThread?.Abort();
            analysisThread = null;
        }

        public void Start(string chart, string path)
        {
            HasError = false;
            IsComplete = false;
            analysisThread = new Thread(new ThreadStart(() => Analyze(chart, path)));
            analysisThread.Start();
        }

        private void Analyze(string chart, string path)
        {
            try
            {
                ChartReader reader = ChartReaderFactory.GetReader(new VirtualFileAccess(chart), path);
                Result<ChartFileErrors> parseResult = reader.Parse();
                if (parseResult.IsError)
                {
                    foreach (var e in parseResult.Error.Errors)
                    {
                        faultQueue.Enqueue(new ChartFault
                        {
                            Severity = Severity.Error,
                            LineNumber = e.LineNumber,
                            StartCharPos = e.StartPosition,
                            Length = e.Length,
                            Description = e.Reason,
                        });
                    }
                }

                Dictionary<int, List<Vector2>> requireTaps = new Dictionary<int, List<Vector2>>();

                List<RawTimingGroup> rawTimingGroups = reader.TimingGroups;
                List<RawEvent> rawEvents = reader.Events;
                foreach (var ev in rawEvents)
                {
                    if (ev is RawHold h)
                    {
                        if (h.EndTiming - h.Timing <= 1000 / 60f && !rawTimingGroups[ev.TimingGroup].NoInput)
                        {
                            faultQueue.Enqueue(new ChartFault
                            {
                                Severity = Severity.Warning,
                                LineNumber = h.Line,
                                StartCharPos = h.CharacterStart,
                                Length = h.Length,
                                Description = I18n.S("Format.Warning.HoldTooShort"),
                            });
                        }

                        Vector2 pos = new Vector2(ArcFormula.LaneToWorldX(h.Lane), 0);
                        if (!requireTaps.ContainsKey(h.Timing))
                        {
                            requireTaps.Add(h.Timing, new List<Vector2> { pos });
                        }
                        else
                        {
                            foreach (var p in requireTaps[h.Timing])
                            {
                                if ((p - pos).sqrMagnitude <= Values.TapOverlapWarningThreshold)
                                {
                                    faultQueue.Enqueue(new ChartFault
                                    {
                                        Severity = Severity.Warning,
                                        LineNumber = h.Line,
                                        StartCharPos = h.CharacterStart,
                                        Length = h.Length,
                                        Description = I18n.S("Format.Warning.TapJudgementOverlap"),
                                    });
                                    break;
                                }
                            }

                            requireTaps[h.Timing].Add(pos);
                        }
                    }

                    if (ev is RawTap t)
                    {
                        Vector2 pos = new Vector2(ArcFormula.LaneToWorldX(t.Lane), 0);
                        if (!requireTaps.ContainsKey(t.Timing))
                        {
                            requireTaps.Add(t.Timing, new List<Vector2> { pos });
                        }
                        else
                        {
                            foreach (var p in requireTaps[t.Timing])
                            {
                                if ((p - pos).sqrMagnitude <= Values.TapOverlapWarningThreshold)
                                {
                                    faultQueue.Enqueue(new ChartFault
                                    {
                                        Severity = Severity.Warning,
                                        LineNumber = t.Line,
                                        StartCharPos = t.CharacterStart,
                                        Length = t.Length,
                                        Description = I18n.S("Format.Warning.TapJudgementOverlap"),
                                    });
                                    break;
                                }
                            }

                            requireTaps[t.Timing].Add(pos);
                        }
                    }

                    if (ev is RawArc a && a.ArcTaps != null)
                    {
                        foreach (var at in a.ArcTaps)
                        {
                            float percent = Mathf.Clamp((float)(at.Timing - a.Timing) / (a.EndTiming - a.Timing), 0, 1);
                            Vector2 pos = a.EndTiming == a.Timing ?
                                new Vector2(
                                    ArcFormula.ArcXToWorld(at.Timing <= a.Timing ? a.XStart : a.XEnd),
                                    ArcFormula.ArcYToWorld(at.Timing <= a.Timing ? a.YStart : a.YEnd)) :
                                new Vector2(
                                    ArcFormula.ArcXToWorld(ArcFormula.X(a.XStart, a.XEnd, percent, a.LineType.ToArcLineType())),
                                    ArcFormula.ArcYToWorld(ArcFormula.Y(a.YStart, a.YEnd, percent, a.LineType.ToArcLineType())));
                            if (!requireTaps.ContainsKey(at.Timing))
                            {
                                requireTaps.Add(at.Timing, new List<Vector2> { pos });
                            }
                            else
                            {
                                foreach (var p in requireTaps[at.Timing])
                                {
                                    if ((p - pos).sqrMagnitude <= Values.TapOverlapWarningThreshold)
                                    {
                                        faultQueue.Enqueue(new ChartFault
                                        {
                                            Severity = Severity.Warning,
                                            LineNumber = at.Line,
                                            StartCharPos = at.CharacterStart,
                                            Length = at.Length,
                                            Description = I18n.S("Format.Warning.TapJudgementOverlap"),
                                        });
                                        break;
                                    }
                                }

                                requireTaps[at.Timing].Add(pos);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                IsComplete = true;
            }
        }
    }
}