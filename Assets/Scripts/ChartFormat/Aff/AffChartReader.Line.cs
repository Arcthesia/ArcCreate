using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Arc.ChartFormat
{
    /// <summary>
    /// Object for reading a .aff chart file.
    /// </summary>
    public partial class AffChartReader
    {
        /// <summary>
        /// Parse a timing aff line of the format "timing([start],[bpm],[beatPerLine]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <returns>The parsed object.</returns>
        public RawTiming ParseTiming(string line)
        {
            try
            {
                StringParser s = new StringParser(line);
                s.Skip("timing(".Length);
                int tick = s.ReadInt(",");
                float bpm = s.ReadFloat(",");
                float beatsPerLine = s.ReadFloat(")");

                if (beatsPerLine < 0)
                {
                    throw new ChartFormatException(I.S("Format.Exception.BeatsPerLineNegative"));
                }

                if (tick == 0 && bpm == 0)
                {
                    throw new ChartFormatException(I.S("Format.Exception.BaseBPMNegative"));
                }

                return new RawTiming()
                {
                    Timing = tick,
                    BeatsPerLine = beatsPerLine,
                    Bpm = bpm,
                    Type = RawEventType.Timing,
                    TimingGroup = currentTimingGroup,
                };
            }
            catch (ChartFormatException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ChartFormatException(I.S("Format.Exception.SymbolError", ex.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// Parse a tap aff line of the format "([timing],[lane]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <returns>The parsed object.</returns>
        public RawTap ParseTap(string line)
        {
            try
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
                    TimingGroup = currentTimingGroup,
                };
            }
            catch (ChartFormatException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ChartFormatException(I.S("Format.Exception.SymbolError", ex.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// Parse a hold aff line of the format "hold([timing],[endTiming],[lane]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <returns>The parsed object.</returns>
        public RawHold ParseHold(string line)
        {
            try
            {
                StringParser s = new StringParser(line);
                s.Skip("hold(".Length);
                int tick = s.ReadInt(",");
                int endtick = s.ReadInt(",");
                int track = s.ReadInt(")");
                if (endtick <= tick)
                {
                    throw new ChartFormatException(I.S("Format.Exception.DurationNegative"));
                }

                return new RawHold()
                {
                    Timing = tick,
                    EndTiming = endtick,
                    Lane = track,
                    Type = RawEventType.Hold,
                    TimingGroup = currentTimingGroup,
                };
            }
            catch (ChartFormatException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ChartFormatException(I.S("Format.Exception.SymbolError", ex.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// Parse an arc aff line of the format
        /// "arc([start],[end],[startx],[endx],[type],[starty],[endy],[color],[sfx],[istrace])[arctap([timing]),...];".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <returns>The parsed object.</returns>
        public RawArc ParseArc(string line)
        {
            try
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
                List<int> arctap = null;
                if (s.Current != ";")
                {
                    arctap = new List<int>();
                    istrace = true;
                    while (true)
                    {
                        s.Skip("[arctap(".Length);
                        int timing = s.ReadInt(")");
                        if (timing < tick || timing > endtick)
                        {
                            throw new ChartFormatException(I.S("Format.Exception.ArcTapOutOfRange"));
                        }

                        arctap.Add(timing);
                        if (s.Current != ",")
                        {
                            break;
                        }
                    }
                }

                if (endtick < tick)
                {
                    throw new ChartFormatException(I.S("Format.Exception.DurationNegative"));
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
                    TimingGroup = currentTimingGroup,
                };
            }
            catch (ChartFormatException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ChartFormatException(I.S("Format.Exception.SymbolError", ex.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// Parse a camera aff line of the format
        /// "camera([timing],[x],[y],[z],[rotx],[roty],[rotz],[type],[duration]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <returns>The parsed object.</returns>
        public RawCamera ParseCamera(string line)
        {
            try
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
                    throw new ChartFormatException(I.S("Format.Exception.DurationNegative"));
                }

                return new RawCamera()
                {
                    TimingGroup = currentTimingGroup,
                    Timing = tick,
                    Duration = duration,
                    Move = move,
                    Rotate = rotate,
                    CameraType = type,
                    Type = RawEventType.Camera,
                };
            }
            catch (ChartFormatException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ChartFormatException(I.S("Format.Exception.SymbolError", ex.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// Parse a scenecontrol aff line of the format
        /// "scenecontrol([timing],[type],[arg1],[arg2]...);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <returns>The parsed object.</returns>
        public RawSceneControl ParseSceneControl(string line)
        {
            try
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
                            parameters.Add((object)Evaluator.Float(rawparam));
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
                        TimingGroup = currentTimingGroup,
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
                        TimingGroup = currentTimingGroup,
                    };
                }
            }
            catch (ChartFormatException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ChartFormatException(I.S("Format.Exception.SymbolError", ex.Message, ex.StackTrace));
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
            try
            {
                StringParser s = new StringParser(line);
                s.Skip("timinggroup(".Length);
                string properties = s.ReadString(")");
                var prop = new RawTimingGroup(properties) { File = path };
                return prop;
            }
            catch (ChartFormatException ex)
            {
                return new RawTimingGroup() { File = path };
                throw ex;
            }
            catch (Exception ex)
            {
                return new RawTimingGroup() { File = path };
                throw new ChartFormatException(I.S("Format.Exception.SymbolError", ex.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// Parse a include aff line of the format "include([reference]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <returns>The referenced file name.</returns>
        public string ParseInclude(string line)
        {
            try
            {
                StringParser s = new StringParser(line);
                s.Skip("include(".Length);
                string fileName = s.ReadString(")").Trim();
                return fileName;
            }
            catch (Exception ex)
            {
                throw new ChartFormatException(I.S("Format.Exception.SymbolError", ex.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// Parse a fragment aff line of the format "fragment([offset],[reference]);".
        /// </summary>
        /// <param name="line">The string to parse.</param>
        /// <returns>A tuple of the timing offset and referenced file name.</returns>
        public (int timing, string fileName) ParseFragment(string line)
        {
            try
            {
                StringParser s = new StringParser(line);
                s.Skip("fragment(".Length);
                int baseTiming = s.ReadInt(",");
                string fileName = s.ReadString(")").Trim();
                return (baseTiming, fileName);
            }
            catch (Exception ex)
            {
                throw new ChartFormatException(I.S("Format.Exception.SymbolError", ex.Message, ex.StackTrace));
            }
        }

        private string SwitchFileName(string currentPath, string target)
        {
            return Path.Combine(
                Path.GetDirectoryName(currentPath),
                target);
        }
    }
}