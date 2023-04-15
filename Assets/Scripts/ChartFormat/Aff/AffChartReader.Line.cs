using System;
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
        public RawTiming ParseTiming(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip("timing(".Length);
            int tick = s.ReadInt(",");
            float bpm = s.ReadFloat(",");
            float divisor = s.ReadFloat(")");

            if (divisor < 0)
            {
                throw new ChartFormatException(I18n.S("Format.Exception.DivisorNegative"));
            }

            if (tick == 0 && bpm == 0)
            {
                throw new ChartFormatException(I18n.S("Format.Exception.BaseBpmNegative"));
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
        public RawTap ParseTap(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip(1);
            int tick = s.ReadInt(",");
            int track = s.ReadInt(")");
            return new RawTap()
            {
                Timing = tick,
                Lane = track,
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
        public RawHold ParseHold(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip("hold(".Length);
            int tick = s.ReadInt(",");
            int endtick = s.ReadInt(",");
            int track = s.ReadInt(")");
            if (endtick <= tick)
            {
                throw new ChartFormatException(I18n.S("Format.Exception.DurationNegative"));
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
        public RawArc ParseArc(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip("arc(".Length);
            int tick = s.ReadInt(",");
            int endtick = s.ReadInt(",");
            float startx = s.ReadFloat(",");
            float endx = s.ReadFloat(",");
            string linetype = s.ReadString(",");
            float starty = s.ReadFloat(",");
            float endy = s.ReadFloat(",");
            int color = s.ReadInt(",");
            string effect = s.ReadString(",");
            bool istrace = s.ReadBool(")");
            List<RawArcTap> arctap = null;

            if (color < 0)
            {
                throw new ChartFormatException(I18n.S("Format.Exception.ArcColorNegative"));
            }

            if (s.Current != ";")
            {
                arctap = new List<RawArcTap>();
                while (true)
                {
                    int startCharPos = s.Pos + 1;
                    s.Skip("[arctap(".Length);
                    int timing = s.ReadInt(")");
                    int endCharPos = s.Pos;
                    if (timing < tick || timing > endtick)
                    {
                        throw new ChartFormatException(I18n.S("Format.Exception.ArcTapOutOfRange"), startCharPos, endCharPos);
                    }

                    arctap.Add(new RawArcTap
                    {
                        Type = RawEventType.ArcTap,
                        Timing = timing,
                        TimingGroup = CurrentTimingGroup,
                        Line = lineNumber,
                        CharacterStart = startCharPos,
                        CharacterEnd = endCharPos,
                    });
                    if (s.Current != ",")
                    {
                        break;
                    }
                }
            }

            if (endtick < tick)
            {
                throw new ChartFormatException(I18n.S("Format.Exception.DurationNegative"));
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
        public RawCamera ParseCamera(string line, int lineNumber)
        {
            StringParser s = new StringParser(line);
            s.Skip("camera(".Length);
            int tick = s.ReadInt(",");
            Vector3 move = new Vector3(s.ReadFloat(","), s.ReadFloat(","), s.ReadFloat(","));
            Vector3 rotate = new Vector3(s.ReadFloat(","), s.ReadFloat(","), s.ReadFloat(","));
            string type = s.ReadString(",");
            int duration = s.ReadInt(")");

            if (duration < 0)
            {
                throw new ChartFormatException(I18n.S("Format.Exception.DurationNegative"));
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
        public RawSceneControl ParseSceneControl(string line, int lineNumber)
        {
            try
            {
                StringParser s = new StringParser(line);
                s.Skip("scenecontrol(".Length);
                int tick = s.ReadInt(",");
                string type = s.ReadString(",").Trim();

                string parameterString = s.ReadString();
                parameterString = parameterString.Substring(0, parameterString.LastIndexOf(')'));
                string[] split = parameterString.Split(',');

                List<object> parameters = new List<object>();

                string currentString = "";
                foreach (string rawparam in split)
                {
                    bool isStart = rawparam[0] == '\"';
                    bool isEnd = rawparam[rawparam.Length - 1] == '\"'
                              && rawparam.Length >= 2
                              && rawparam[rawparam.Length - 2] != '\\';

                    if (isStart)
                    {
                        currentString = rawparam;
                    }
                    else if (currentString.Length > 0)
                    {
                        currentString += "," + rawparam;
                    }

                    if (currentString.Length == 0)
                    {
                        if (!Evaluator.TryFloat(rawparam, out float val))
                        {
                            throw new ArgumentException(rawparam);
                        }

                        parameters.Add(val);
                    }
                    else if (isEnd)
                    {
                        string param = currentString.Substring(1, currentString.Length - 2);
                        param = param.Replace("\\\"", "\"");
                        parameters.Add(param);
                        currentString = "";
                    }
                }

                return new RawSceneControl()
                {
                    Timing = tick,
                    Type = RawEventType.SceneControl,
                    Arguments = parameters,
                    SceneControlTypeName = type,
                    TimingGroup = CurrentTimingGroup,
                    Line = lineNumber,
                };
            }
            catch (Exception)
            {
                // 0 arguments
                StringParser s = new StringParser(line);
                s.Skip("scenecontrol(".Length);
                int tick = s.ReadInt(",");
                string type = s.ReadString(")");
                List<object> parameters = new List<object>();
                return new RawSceneControl()
                {
                    Timing = tick,
                    Type = RawEventType.SceneControl,
                    Arguments = parameters,
                    SceneControlTypeName = type,
                    TimingGroup = CurrentTimingGroup,
                    Line = lineNumber,
                };
            }
        }

        /// <summary>
        /// Parse a timing group aff line of the format "timinggroup([property],...){".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <param name="path">The path of the file this group was read from.</param>
        /// <returns>The parsed object.</returns>
        public RawTimingGroup ParseTimingGroup(string line, string path = "")
        {
            StringParser s = new StringParser(line);
            s.Skip("timinggroup(".Length);
            string properties = s.ReadString(")");
            var prop = new RawTimingGroup(properties) { File = path };
            return prop;
        }

        /// <summary>
        /// Parse a include aff line of the format "include([reference]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <returns>The referenced file name.</returns>
        public string ParseInclude(string line)
        {
            StringParser s = new StringParser(line);
            s.Skip("include(".Length);
            string fileName = s.ReadString(")").Trim();
            return fileName;
        }

        /// <summary>
        /// Parse a fragment aff line of the format "fragment([offset],[reference]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <returns>A tuple of the timing offset and referenced file name.</returns>
        public (int timing, string fileName) ParseFragment(string line)
        {
            StringParser s = new StringParser(line);
            s.Skip("fragment(".Length);
            int baseTiming = s.ReadInt(",");
            string fileName = s.ReadString(")").Trim();
            return (baseTiming, fileName);
        }

        private string SwitchFileName(string currentPath, string target)
        {
            string dir = Path.GetDirectoryName(currentPath);
            return Path.Combine(dir, target);
        }
    }
}