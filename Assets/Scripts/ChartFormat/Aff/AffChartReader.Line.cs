using System.Collections.Generic;
using System.IO;
using ArcCreate.Utility.Parser;
using UnityEngine;

namespace ArcCreate.ChartFormat
{
    /// <summary>
    /// Object for reading a .aff chart file.
    /// </summary>
    public partial class AffChartReader
    {
        /// <summary>
        /// Parse a timing aff line of the format "timing([start],[bpm],[divisor]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <param name="lineNumber">The line number of the event.</param>
        /// <returns>The parsed object.</returns>
        public Result<RawTiming, ChartError> ParseTiming(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip("timing(".Length);
            if (!s.ReadInt(",").TryUnwrap(out TextSpan<int> tick, out ParsingError e)
             || !s.ReadFloat(",").TryUnwrap(out TextSpan<float> bpm, out e)
             || !s.ReadFloat(")").TryUnwrap(out TextSpan<float> divisor, out e))
            {
                return ChartError.Parsing(line, lineNumber, RawEventType.Timing, e);
            }

            if (divisor < 0)
            {
                return ChartError.Property(line, lineNumber, RawEventType.Timing, divisor.StartPos, divisor.Length, ChartError.Kind.DivisorNegative);
            }

            return new RawTiming()
            {
                Timing = tick,
                Divisor = divisor,
                Bpm = bpm,
                Type = RawEventType.Timing,
                TimingGroup = CurrentTimingGroup,
                Line = lineNumber,
            };
        }

        /// <summary>
        /// Parse a tap aff line of the format "([timing],[lane]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <param name="lineNumber">The line number of the event.</param>
        /// <returns>The parsed object.</returns>
        public Result<RawTap, ChartError> ParseTap(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip(1);
            if (!s.ReadInt(",").TryUnwrap(out TextSpan<int> tick, out ParsingError e)
             || !s.ReadInt(")").TryUnwrap(out TextSpan<int> lane, out e))
            {
                return ChartError.Parsing(line, lineNumber, RawEventType.Tap, e);
            }

            return new RawTap()
            {
                Timing = tick,
                Lane = lane,
                Type = RawEventType.Tap,
                TimingGroup = CurrentTimingGroup,
                Line = lineNumber,
            };
        }

        /// <summary>
        /// Parse a hold aff line of the format "hold([timing],[endTiming],[lane]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <param name="lineNumber">The line number of the event.</param>
        /// <returns>The parsed object.</returns>
        public Result<RawHold, ChartError> ParseHold(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip("hold(".Length);
            if (!s.ReadInt(",").TryUnwrap(out TextSpan<int> tick, out ParsingError e)
             || !s.ReadInt(",").TryUnwrap(out TextSpan<int> endtick, out e)
             || !s.ReadInt(")").TryUnwrap(out TextSpan<int> track, out e))
            {
                return ChartError.Parsing(line, lineNumber, RawEventType.Hold, e);
            }

            if (endtick <= tick)
            {
                return ChartError.Property(
                    line,
                    lineNumber,
                    RawEventType.Hold,
                    tick.StartPos,
                    endtick.StartPos + endtick.Length - tick.StartPos,
                    endtick == tick ? ChartError.Kind.DurationZero : ChartError.Kind.DurationNegative);
            }

            return new RawHold()
            {
                Timing = tick,
                EndTiming = endtick,
                Lane = track,
                Type = RawEventType.Hold,
                TimingGroup = CurrentTimingGroup,
                Line = lineNumber,
            };
        }

        /// <summary>
        /// Parse an arc aff line of the format
        /// "arc([start],[end],[startx],[endx],[type],[starty],[endy],[color],[sfx],[istrace])[arctap([timing]),...];".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <param name="lineNumber">The line number of the event.</param>
        /// <returns>The parsed object.</returns>
        public Result<RawArc, ChartError> ParseArc(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip("arc(".Length);
            if (!s.ReadInt(",").TryUnwrap(out TextSpan<int> tick, out ParsingError e)
             || !s.ReadInt(",").TryUnwrap(out TextSpan<int> endtick, out e)
             || !s.ReadFloat(",").TryUnwrap(out TextSpan<float> startx, out e)
             || !s.ReadFloat(",").TryUnwrap(out TextSpan<float> endx, out e)
             || !s.ReadString(",").TryUnwrap(out TextSpan<string> linetype, out e)
             || !s.ReadFloat(",").TryUnwrap(out TextSpan<float> starty, out e)
             || !s.ReadFloat(",").TryUnwrap(out TextSpan<float> endy, out e)
             || !s.ReadInt(",").TryUnwrap(out TextSpan<int> color, out e)
             || !s.ReadString(",").TryUnwrap(out TextSpan<string> effect, out e)
             || !s.ReadBool(")").TryUnwrap(out TextSpan<bool> istrace, out e))
            {
                return ChartError.Parsing(line, lineNumber, RawEventType.Arc, e);
            }

            List<RawArcTap> arctap = null;

            if (color < 0)
            {
                return ChartError.Property(line, lineNumber, RawEventType.Arc, color.StartPos, color.Length, ChartError.Kind.ArcColorNegative);
            }

            if (s.Current != ';')
            {
                arctap = new List<RawArcTap>();
                while (true)
                {
                    int startCharPos = s.Pos + 1;
                    s.Skip("[arctap(".Length);
                    if (!s.ReadInt(")").TryUnwrap(out TextSpan<int> timing, out ParsingError ae))
                    {
                        return ChartError.Parsing(line, lineNumber, RawEventType.ArcTap, ae);
                    }

                    int length = s.Pos - startCharPos;
                    if (timing < tick || timing > endtick)
                    {
                        return ChartError.Property(line, lineNumber, RawEventType.ArcTap, timing.StartPos, timing.Length, ChartError.Kind.ArcTapOutOfRange);
                    }

                    arctap.Add(new RawArcTap
                    {
                        Type = RawEventType.ArcTap,
                        Timing = timing,
                        TimingGroup = CurrentTimingGroup,
                        Line = lineNumber,
                        CharacterStart = startCharPos,
                        Length = length,
                    });

                    if (s.Current != ',')
                    {
                        break;
                    }
                }
            }

            if (endtick < tick)
            {
                return ChartError.Property(
                    line,
                    lineNumber,
                    RawEventType.Arc,
                    tick.StartPos,
                    endtick.StartPos + endtick.Length - tick.StartPos,
                    ChartError.Kind.DurationNegative);
            }

            return new RawArc()
            {
                Timing = tick,
                EndTiming = endtick,
                XStart = startx,
                XEnd = endx,
                LineType = linetype,
                YStart = starty,
                YEnd = endy,
                Color = color,
                IsTrace = istrace,
                Type = RawEventType.Arc,
                ArcTaps = arctap,
                Sfx = effect,
                TimingGroup = CurrentTimingGroup,
                Line = lineNumber,
            };
        }

        /// <summary>
        /// Parse a camera aff line of the format
        /// "camera([timing],[x],[y],[z],[rotx],[roty],[rotz],[type],[duration]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <param name="lineNumber">The line number of the event.</param>
        /// <returns>The parsed object.</returns>
        public Result<RawCamera, ChartError> ParseCamera(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip("camera(".Length);
            if (!s.ReadInt(",").TryUnwrap(out TextSpan<int> tick, out ParsingError e)
             || !s.ReadFloat(",").TryUnwrap(out TextSpan<float> mx, out e)
             || !s.ReadFloat(",").TryUnwrap(out TextSpan<float> my, out e)
             || !s.ReadFloat(",").TryUnwrap(out TextSpan<float> mz, out e)
             || !s.ReadFloat(",").TryUnwrap(out TextSpan<float> rx, out e)
             || !s.ReadFloat(",").TryUnwrap(out TextSpan<float> ry, out e)
             || !s.ReadFloat(",").TryUnwrap(out TextSpan<float> rz, out e)
             || !s.ReadString(",").TryUnwrap(out TextSpan<string> type, out e)
             || !s.ReadInt(")").TryUnwrap(out TextSpan<int> duration, out e))
            {
                return ChartError.Parsing(line, lineNumber, RawEventType.Camera, e);
            }

            Vector3 move = new Vector3(mx, my, mz);
            Vector3 rotate = new Vector3(rx, ry, rz);

            if (duration < 0)
            {
                return ChartError.Property(line, lineNumber, RawEventType.Camera, duration.StartPos, duration.Length, ChartError.Kind.DurationNegative);
            }

            return new RawCamera()
            {
                TimingGroup = CurrentTimingGroup,
                Timing = tick,
                Duration = duration,
                Move = move,
                Rotate = rotate,
                CameraType = type,
                Type = RawEventType.Camera,
                Line = lineNumber,
            };
        }

        /// <summary>
        /// Parse a scenecontrol aff line of the format
        /// "scenecontrol([timing],[type],[arg1],[arg2]...);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <param name="lineNumber">The line number of the event.</param>
        /// <returns>The parsed object.</returns>
        public Result<RawSceneControl, ChartError> ParseSceneControl(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip("scenecontrol(".Length);
            if (!s.ReadInt(",").TryUnwrap(out TextSpan<int> tick, out ParsingError e)
             || !s.ReadString(")").TryUnwrap(out TextSpan<string> parameters, out e))
            {
                return ChartError.Parsing(line, lineNumber, RawEventType.SceneControl, e);
            }

            string parametersString = parameters.Value + ",";
            StringParser p = new StringParser(parametersString);
            string currentString = "";
            List<object> args = new List<object>();

            if (!p.ReadString(",").TryUnwrap(out TextSpan<string> type, out e))
            {
                return ChartError.Parsing(line, lineNumber, RawEventType.SceneControl, e);
            }

            while (!p.HasEnded)
            {
                if (!p.ReadString(",").TryUnwrap(out TextSpan<string> rawParamSpan, out e))
                {
                    return ChartError.Parsing(line, lineNumber, RawEventType.SceneControl, e);
                }

                string rawParam = rawParamSpan.Value;
                bool isStart = rawParam[0] == '\"';
                bool isEnd = rawParam.Length >= 2
                          && rawParam[rawParam.Length - 1] == '\"'
                          && rawParam[rawParam.Length - 2] != '\\';

                if (isStart)
                {
                    currentString = rawParam;
                }
                else if (currentString.Length > 0)
                {
                    currentString += "," + rawParam;
                }

                if (currentString.Length == 0)
                {
                    if (!Evaluator.TryFloat(rawParam, out float val))
                    {
                        return ChartError.Parsing(
                            line,
                            lineNumber,
                            RawEventType.SceneControl,
                            new ParsingError(
                                rawParamSpan,
                                rawParamSpan.StartPos,
                                rawParamSpan.Length,
                                ParsingError.Kind.InvalidConversionToFloat));
                    }

                    args.Add(val);
                }
                else if (isEnd)
                {
                    string param = currentString.Substring(1, currentString.Length - 2);
                    param = param.Replace("\\\"", "\"");
                    args.Add(param);
                    currentString = "";
                }
            }

            return new RawSceneControl()
            {
                Timing = tick,
                Type = RawEventType.SceneControl,
                Arguments = args,
                SceneControlTypeName = type,
                TimingGroup = CurrentTimingGroup,
                Line = lineNumber,
            };
        }

        /// <summary>
        /// Parse a timing group aff line of the format "timinggroup([property],...){".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <param name="lineNumber">Line number of the string.</param>
        /// <param name="path">The path of the file this group was read from.</param>
        /// <returns>The parsed object.</returns>
        public Result<RawTimingGroup, ChartError> ParseTimingGroup(string line, int lineNumber, string path = "")
        {
            StringParser s = new StringParser(line);
            s.Skip("timinggroup(".Length);
            if (!s.ReadString(")").TryUnwrap(out TextSpan<string> properties, out ParsingError e))
            {
                return ChartError.Parsing(line, lineNumber, RawEventType.TimingGroup, e);
            }

            if (!RawTimingGroup.Parse(properties, lineNumber).TryUnwrap(out RawTimingGroup tg, out ChartError ce))
            {
                return ce;
            }

            tg.File = path;
            return tg;
        }

        /// <summary>
        /// Parse a include aff line of the format "include([reference]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <param name="lineNumber">Line number of the string.</param>
        /// <returns>The referenced file name.</returns>
        public Result<string, ChartError> ParseInclude(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip("include(".Length);
            if (!s.ReadString(")").TryUnwrap(out TextSpan<string> fileName, out ParsingError e))
            {
                return ChartError.Parsing(line, lineNumber, RawEventType.Include, e);
            }

            return fileName.Value.Trim();
        }

        /// <summary>
        /// Parse a fragment aff line of the format "fragment([offset],[reference]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <param name="lineNumber">Line number of the string.</param>
        /// <returns>A tuple of the timing offset and referenced file name.</returns>
        public Result<RawFragment, ChartError> ParseFragment(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip("fragment(".Length);
            if (!s.ReadInt(",").TryUnwrap(out TextSpan<int> baseTiming, out ParsingError e)
             || !s.ReadString(")").TryUnwrap(out TextSpan<string> fileName, out e))
            {
                return ChartError.Parsing(line, lineNumber, RawEventType.Fragment, e);
            }

            return new RawFragment
            {
                Timing = baseTiming,
                File = fileName.Value.Trim(),
            };
        }

        private string SwitchFileName(string currentPath, string target)
        {
            string dir = Path.GetDirectoryName(currentPath);
            return Path.Combine(dir, target);
        }
    }
}