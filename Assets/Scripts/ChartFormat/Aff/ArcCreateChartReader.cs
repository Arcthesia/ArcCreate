using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ArcCreate.ChartFormat.Grammar;
using ArcCreate.Utility.Parser;
using UnityEngine;

namespace ArcCreate.ChartFormat
{
    /// <summary>
    /// Object for reading a .aff chart file.
    /// </summary>
    public class ArcCreateChartReader : ChartReader
    {
        public static ArcCreateChartReader Instance => new(null, string.Empty, string.Empty, string.Empty);

        /// <summary>
        /// Initializes a new instance of the <see cref="ArcCreateChartReader"/> class. You should use <see cref="ChartReaderFactory"/> to instantiate instead.
        /// </summary>
        /// <param name="fileAccess">File access wrapper. You should normally use <see cref="PhysicalFileAccess"/>.</param>
        /// <param name="relativeDirectory">The directory relative to the base folder.</param>
        /// <param name="fullPath">The absolute path leading to the file.</param>
        /// <param name="fileName">The file name. Passed as-is from include and fragment aff commands.</param>
        public ArcCreateChartReader(IFileAccessWrapper fileAccess, string relativeDirectory, string fullPath,
            string fileName)
            : base(fileAccess, relativeDirectory, fullPath, fileName)
        {
            TimingPointDensity = 1;
            AudioOffset = 0;
        }

        public override Result<ChartFileErrors> Parse()
        {
            var errors = new List<ChartError>();

            TimingGroups.Add(new RawTimingGroup { File = FileName });
            AllIncludes.Add(FileName);

            var lines = FileAccess.ReadFileByLines(FullPath);
            if (!lines.HasValue)
            {
                errors.Add(ChartError.Format(RawEventType.Unknown, ChartError.Kind.FileDoesNotExist));
                return new ChartFileErrors(FileName, errors);
            }

            #region Header

            if (!ParseHeader(lines.Value).TryUnwrap(out var headerParseResult, out var error))
            {
                errors.Add(error);
                return new ChartFileErrors(FileName, errors);
            }

            var (headerLineNumber, headerDict) = headerParseResult;

            int audioOffset = AudioOffset;
            if (headerDict.TryGetValue("AudioOffset", out string audioOffsetRaw))
            {
                Evaluator.TryInt(audioOffsetRaw, out audioOffset);
            }

            AudioOffset = audioOffset;

            float density = TimingPointDensity;
            if (headerDict.TryGetValue("TimingPointsDensityFactor", out var tpdfRaw) ||
                headerDict.TryGetValue("TimingPointDensityFactor", out tpdfRaw))
            {
                Evaluator.TryFloat(tpdfRaw, out density);
            }

            TimingPointDensity = density;

            #endregion

            var chartSegment = ParseEvents(string.Join("\n", lines.Value.Skip(headerLineNumber)), parser =>
            {
                parser.RemoveErrorListeners();
                parser.AddErrorListener(new AntlrChartErrorListener(lines.Value));
            });

            try
            {
                foreach (var evt in chartSegment.Events)
                {
                    if (evt.Name == "timinggroup")
                    {
                        var (tg, events) = ParseTimingGroup(evt, TimingGroups.Count);
                        TimingGroups.Add(tg);
                        Events.AddRange(events);
                    }
                    else
                    {
                        Events.Add(ParseEvent(evt, 0));
                    }
                }

                foreach (var rawEvent in Events)
                {
                    switch (rawEvent)
                    {
                        case RawInclude rawInclude:
                        {
                            string fullInclPath = SwitchFileName(FullPath, rawInclude.File);

                            if (AllIncludes.Contains(fullInclPath))
                            {
                                errors.Add(ChartError.Property(
                                    lines.Value[rawInclude.Line],
                                    rawInclude.Line,
                                    RawEventType.Include,
                                    0,
                                    lines.Value[rawInclude.Line].Length,
                                    ChartError.Kind.IncludeReferencedMultipleTimes));

                                break;
                            }

                            if (AllFragments.Contains(fullInclPath))
                            {
                                errors.Add(ChartError.Property(
                                    lines.Value[rawInclude.Line],
                                    rawInclude.Line,
                                    RawEventType.Fragment,
                                    0,
                                    lines.Value[rawInclude.Line].Length,
                                    ChartError.Kind.IncludeAReferencedFragment));

                                break;
                            }

                            var includeResult = AddInclude(rawInclude);
                            if (includeResult.IsError)
                            {
                                errors.Add(ChartError.ReferencedFile(
                                    lines.Value[rawInclude.Line],
                                    rawInclude.Line,
                                    RawEventType.Include,
                                    includeResult.Error));
                            }

                            break;
                        }
                        case RawFragment rawFragment:
                        {
                            string fullFragPath = SwitchFileName(FullPath, rawFragment.File);

                            if (AllIncludes.Contains(fullFragPath))
                            {
                                errors.Add(ChartError.Property(
                                    lines.Value[rawFragment.Line],
                                    rawFragment.Line,
                                    RawEventType.Include,
                                    0,
                                    lines.Value[rawFragment.Line].Length,
                                    ChartError.Kind.IncludeReferencedMultipleTimes));

                                break;
                            }

                            var fragmentResult = AddFragment(rawFragment);
                            if (fragmentResult.IsError)
                            {
                                errors.Add(ChartError.ReferencedFile(lines.Value[rawFragment.Line],
                                    rawFragment.Line, RawEventType.Fragment, fragmentResult.Error));
                            }

                            break;
                        }
                    }
                }
            }
            catch (AntlrParseException ex)
            {
                errors.Add(ChartError.Parsing(ex.Raw,
                    ex.LineNumber + headerLineNumber, // skip headers and separator
                    ex.EventType,
                    new ParsingError(ex.Message,
                        0,
                        ex.Raw.Length,
                        ParsingError.Kind.Antlr)));
            }
            catch (ChartReaderException ex)
            {
                errors.Add(ChartError.Property(ex.Raw,
                    ex.LineNumber + headerLineNumber, // skip headers and separator
                    ex.EventType,
                    0,
                    ex.Raw.Length,
                    ex.ErrorKind));
            }

            foreach (ChartReader reference in References)
            {
                int removedBaseGroup = 0;
                int referenceBaseGroupCount = reference.Events.Count(e => e.TimingGroup == 0);

                if (referenceBaseGroupCount <= 1)
                {
                    reference.TimingGroups.RemoveAt(0);
                    removedBaseGroup = 1;
                    for (int i = reference.Events.Count - 1; i >= 0; i--)
                    {
                        if (reference.Events[i].TimingGroup == 0)
                        {
                            reference.Events.RemoveAt(i);
                        }
                    }
                }

                foreach (RawEvent e in reference.Events)
                {
                    e.TimingGroup += TimingGroups.Count - removedBaseGroup;
                }

                Events.AddRange(reference.Events);
                TimingGroups.AddRange(reference.TimingGroups);
            }

            var validation = FinalValidity();
            if (validation.IsError) errors.Add(validation.Error);

            Events.Sort((a, b) => a.Timing.CompareTo(b.Timing));

            return errors.Count > 0
                ? new ChartFileErrors(FileName, errors)
                : Result<ChartFileErrors>.Ok();
        }

        public override Result<(int startLine, Dictionary<string, string>), ChartError> ParseHeader(string[] lines)
        {
            var headerItems = new Dictionary<string, string>();

            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                var line = lines[lineNumber];
                if (line.StartsWith('-'))
                {
                    return (lineNumber + 1, headerItems);
                }

                StringParser s = new StringParser(line);
                if (!s.ReadString(":").TryUnwrap(out TextSpan<string> headerType, out ParsingError e) ||
                    !s.ReadString().TryUnwrap(out var value, out e))
                {
                    return ChartError.Parsing(line, lineNumber, RawEventType.Header, e);
                }

                headerItems.Add(headerType, value);
            }

            return ChartError.Parsing("-", 0, RawEventType.Header,
                new ParsingError("No header found", 0, lines[0].Length, ParsingError.Kind.CharacterNotFound));
        }

        protected static string GetTimingGroupPropertiesRaw(string eventRaw)
        {
            var rawMatch = Regex.Match(eventRaw, @"timinggroup\((.*?)\)");
            var timingGroupPropRaw = rawMatch.Success 
                ? $"timinggroup({rawMatch.Groups[1].Value}){{...}}"
                : "timinggroup(){...}";
            
            return timingGroupPropRaw;
        }
        
        public override RawTimingGroup ParseTimingGroupProperties(string raw, AntlrEvent evt)
        {
            var propDict = new Dictionary<string, string>();

            foreach (var antlrValue in evt.Values)
            {
                switch (antlrValue.Type)
                {
                    case AntlrValueType.String:
                    {
                        var value = antlrValue.GetStringValue();
                        if (!string.IsNullOrWhiteSpace(value)) propDict.Add(value, null);
                        break;
                    }
                    case AntlrValueType.KeyValuePair:
                    {
                        var (key, aValue) = antlrValue.GetKeyValuePair();

                        if (aValue.IsStringValue)
                        {
                            propDict.Add(key, aValue.GetStringValue());
                        }
                        else if (aValue.IsIntegerValue)
                        {
                            propDict.Add(key, aValue.GetIntegerValue().ToString());
                        }
                        else if (aValue.IsAlgebraicValue)
                        {
                            propDict.Add(key, aValue.GetAlgebraicValue().ToString(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            throw new AntlrParseException("Nested key-value pair is not allowed", evt.Raw,
                                RawEventType.AntlrValue, evt.LineNumber, evt.ColumnNumber);
                        }

                        break;
                    }
                    default:
                        throw new AntlrParseException("Algebraic value is not allowed", evt.Raw,
                            RawEventType.AntlrValue, evt.LineNumber, evt.ColumnNumber);
                }
            }

            var prop = new RawTimingGroup
            {
                File = FileName
            };

            foreach (var (type, value) in propDict)
            {
                if (value != null)
                {
                    bool valid;
                    float val;
                    switch (type.ToLower())
                    {
                        case "name":
                            prop.Name = value; // no need to trim quotes, it has been trimmed during antlr
                            break;
                        case "anglex":
                            valid = Evaluator.TryFloat(value, out val);
                            prop.AngleX = valid ? val : 0;
                            break;
                        case "angley":
                            valid = Evaluator.TryFloat(value, out val);
                            prop.AngleY = valid ? val : 0;
                            break;
                        case "judgesizex":
                            valid = Evaluator.TryFloat(value, out val);
                            prop.JudgementSizeX = valid ? val : 1;
                            break;
                        case "judgesizey":
                            valid = Evaluator.TryFloat(value, out val);
                            prop.JudgementSizeY = valid ? val : 1;
                            break;
                        case "judgeoffsetx":
                            valid = Evaluator.TryFloat(value, out val);
                            prop.JudgementOffsetX = valid ? val : 1;
                            break;
                        case "judgeoffsety":
                            valid = Evaluator.TryFloat(value, out val);
                            prop.JudgementOffsetY = valid ? val : 1;
                            break;
                        case "judgeoffsetz":
                            valid = Evaluator.TryFloat(value, out val);
                            prop.JudgementOffsetZ = valid ? val : 1;
                            break;
                        case "arcresolution":
                            valid = Evaluator.TryFloat(value, out val);
                            val = Mathf.Clamp(val, 0.1f, 10);
                            prop.ArcResolution = valid ? val : 1;
                            break;
                        case "droprate":
                            valid = Evaluator.TryFloat(value, out val);
                            prop.DropRate = valid ? val : 0;
                            break;
                        case "max":
                            prop.AddRemapRules(value, JudgementMap.Max);
                            break;
                        case "perfect":
                            prop.AddRemapRules(value, JudgementMap.PerfectEarly, JudgementMap.PerfectLate);
                            break;
                        case "perfectearly":
                            prop.AddRemapRules(value, JudgementMap.PerfectEarly);
                            break;
                        case "perfectlate":
                            prop.AddRemapRules(value, JudgementMap.PerfectLate);
                            break;
                        case "good":
                            prop.AddRemapRules(value, JudgementMap.GoodEarly, JudgementMap.GoodLate);
                            break;
                        case "goodearly":
                            prop.AddRemapRules(value, JudgementMap.GoodEarly);
                            break;
                        case "goodlate":
                            prop.AddRemapRules(value, JudgementMap.GoodLate);
                            break;
                        case "miss":
                            prop.AddRemapRules(value, JudgementMap.MissEarly, JudgementMap.MissLate);
                            break;
                        case "missearly":
                            prop.AddRemapRules(value, JudgementMap.MissEarly);
                            break;
                        case "misslate":
                            prop.AddRemapRules(value, JudgementMap.MissLate);
                            break;
                        default:
                            throw new ChartReaderException(GetTimingGroupPropertiesRaw(raw), RawEventType.TimingGroup, evt,
                                ChartError.Kind.TimingGroupPropertiesInvalid);
                    }
                }
                else
                {
                    switch (type.ToLower())
                    {
                        case "noinput":
                            prop.NoInput = true;
                            break;
                        case "noclip":
                            prop.NoClip = true;
                            break;
                        case "noheightindicator":
                            prop.NoHeightIndicator = true;
                            break;
                        case "nohead":
                            prop.NoHead = true;
                            break;
                        case "noshadow":
                            prop.NoShadow = true;
                            break;
                        case "noarccap":
                            prop.NoArcCap = true;
                            break;
                        case "noconnection":
                            prop.NoConnection = true;
                            break;
                        case "light":
                            prop.Side = SideOverride.Light;
                            break;
                        case "conflict":
                            prop.Side = SideOverride.Conflict;
                            break;
                        case "fadingholds":
                            prop.FadingHolds = true;
                            break;
                        case "ignoremirror":
                            prop.IgnoreMirror = true;
                            break;
                        case "autoplay":
                            prop.Autoplay = true;
                            break;
                        default:
                            throw new ChartReaderException(raw, RawEventType.TimingGroup, evt,
                                ChartError.Kind.TimingGroupPropertiesInvalid);
                    }
                }
            }

            return prop;
        }

        public override RawEvent ParseEvent(AntlrEvent evt, int timingGroup) => evt.Name switch
        {
            null or "" or "tap" => ParseTap(evt, timingGroup),
            "hold" => ParseHold(evt, timingGroup),
            "timing" => ParseTiming(evt, timingGroup),
            "arc" => ParseArc(evt, timingGroup),
            "scenecontrol" => ParseSceneControl(evt, timingGroup),
            "camera" => ParseCamera(evt, timingGroup),
            "include" => ParseInclude(evt),
            "fragment" => ParseFragment(evt),

            _ => throw new ChartReaderException(evt.Raw, RawEventType.Unknown, evt, ChartError.Kind.Parsing)
        };

        public virtual RawTap ParseTap(AntlrEvent evt, int timingGroup)
        {
            var validator = new ChartReaderValidator(evt, RawEventType.Tap);

            validator.Require(evt.Values.Count == 2);
            validator.Require(evt.Values[0].TryGetIntegerValue(out var tick));
            validator.Require(evt.Values[1].TryGetAlgebraicValue(out var lane));

            return new RawTap
            {
                Type = RawEventType.Tap,
                Timing = tick,
                Lane = (float)lane,
                TimingGroup = timingGroup,
                Line = evt.LineNumber
            };
        }

        public virtual RawHold ParseHold(AntlrEvent evt, int timingGroup)
        {
            var validator = new ChartReaderValidator(evt, RawEventType.Hold);

            validator.Require(evt.Values.Count == 3);
            validator.Require(evt.Values[0].TryGetIntegerValue(out var tick));
            validator.Require(evt.Values[1].TryGetIntegerValue(out var endTick));
            validator.Require(evt.Values[2].TryGetAlgebraicValue(out var track));

            if (endTick == tick)
                throw new ChartReaderException(evt.Raw, RawEventType.Hold, evt, ChartError.Kind.DurationZero);

            if (endTick < tick)
                throw new ChartReaderException(evt.Raw, RawEventType.Hold, evt, ChartError.Kind.DurationNegative);

            return new RawHold
            {
                Type = RawEventType.Hold,
                Timing = tick,
                EndTiming = endTick,
                Lane = (float)track,
                TimingGroup = timingGroup,
                Line = evt.LineNumber
            };
        }

        public virtual RawTiming ParseTiming(AntlrEvent evt, int timingGroup)
        {
            var validator = new ChartReaderValidator(evt, RawEventType.Timing);

            validator.Require(evt.Values.Count == 3);
            validator.Require(evt.Values[0].TryGetIntegerValue(out var tick));
            validator.Require(evt.Values[1].TryGetAlgebraicValue(out var bpm));
            validator.Require(evt.Values[2].TryGetAlgebraicValue(out var divisor) && divisor >= 0, errorKind: ChartError.Kind.DivisorNegative);

            return new RawTiming
            {
                Type = RawEventType.Timing,
                Timing = tick,
                Divisor = (float)divisor,
                Bpm = (float)bpm,
                TimingGroup = timingGroup,
                Line = evt.LineNumber
            };
        }

        public virtual RawArc ParseArc(AntlrEvent evt, int timingGroup)
        {
            var validator = new ChartReaderValidator(evt, RawEventType.Arc);

            validator.Require(evt.Values.Count is >= 10 and <= 11);
            validator.Require(evt.Values[0].TryGetIntegerValue(out var tick));
            validator.Require(evt.Values[1].TryGetIntegerValue(out var endTick));
            validator.Require(evt.Values[2].TryGetAlgebraicValue(out var xStart));
            validator.Require(evt.Values[3].TryGetAlgebraicValue(out var xEnd));
            validator.Require(evt.Values[4].TryGetStringValue(out var lineType));
            validator.Require(evt.Values[5].TryGetAlgebraicValue(out var yStart));
            validator.Require(evt.Values[6].TryGetAlgebraicValue(out var yEnd));
            validator.Require(evt.Values[7].TryGetIntegerValue(out var color) && color >= 0, errorKind: ChartError.Kind.ArcColorNegative);
            validator.Require(evt.Values[8].TryGetStringValue(out var hitSound));
            validator.Require(evt.Values[9].TryGetStringValue(out var arcType));

            if (endTick < tick)
                throw new ChartReaderException(evt.Raw, RawEventType.Arc, evt, ChartError.Kind.DurationNegative);

            double arcResolution = 1.0;
            if (!(evt.Properties.TryGetValue("arcresolution", out var arcResolutionRaw) &&

                  // try get arcResolution from properties first
                  arcResolutionRaw.TryGetAlgebraicValue(out arcResolution)) &&
                evt.Values.Count >= 11)
            {
                // if not presented in properties, try parse from Arc parameters
                evt.Values[10].TryGetAlgebraicValue(out arcResolution);
            }

            Color stainedColor = Color.black;
            bool hasStainedColor =
                evt.Properties.TryGetValue("tracecolor", out var stainedColorRaw) &&
                stainedColorRaw.TryGetStringValue(out string stainedColorStr) &&
                ColorUtility.TryParseHtmlString("#" + stainedColorStr.TrimStart('#'), out stainedColor);

            var isTrace = arcType is "true" or "designant";

            var arc = new RawArc
            {
                Type = RawEventType.Arc,
                Timing = tick,
                EndTiming = endTick,
                XStart = (float)xStart,
                XEnd = (float)xEnd,
                LineType = lineType,
                YStart = (float)yStart,
                YEnd = (float)yEnd,
                Color = color,
                IsTrace = isTrace,
                ArcTaps = evt.SubEvents.Select(x => ParseArcTap(x, timingGroup, tick, endTick)).ToList(),
                Sfx = hitSound,
                TimingGroup = timingGroup,
                Line = evt.LineNumber,
                ArcResolution = (float)arcResolution
            };

            arc.TraceColor = hasStainedColor ? stainedColor : null;

            return arc;
        }

        protected virtual RawArcTap ParseArcTap(AntlrEvent evt, int timingGroup, int parentTick, int parentEndTick)
        {
            if (evt.Name != "arctap")
            {
                throw new ChartReaderException(evt.Raw, RawEventType.ArcTap, evt, ChartError.Kind.Parsing);
            }

            var validator = new ChartReaderValidator(evt, RawEventType.ArcTap);
            validator.Require(evt.Values.Count is 1 or 2);
            validator.Require(evt.Values[0].TryGetIntegerValue(out var tick));

            if (tick < parentTick || tick > parentEndTick)
            {
                throw new ChartReaderException(evt.Raw, RawEventType.ArcTap, evt, ChartError.Kind.ArcTapOutOfRange);
            }

            double width = 1;
            if (evt.Values.Count >= 2) evt.Values[1].TryGetAlgebraicValue(out width);

            return new RawArcTap
            {
                Type = RawEventType.ArcTap,
                Timing = tick,
                TimingGroup = timingGroup,
                Width = (float)width,
                Line = evt.LineNumber,
                CharacterStart = evt.ColumnNumber,
                Length = evt.Raw.Length
            };
        }

        public virtual RawSceneControl ParseSceneControl(AntlrEvent evt, int timingGroup)
        {
            var validator = new ChartReaderValidator(evt, RawEventType.SceneControl);

            validator.Require(evt.Values.Count >= 2);
            validator.Require(evt.Values[0].TryGetIntegerValue(out var tick));
            validator.Require(evt.Values[1].TryGetStringValue(out var type));

            // parameter-less
            if (evt.Values.Count == 2)
            {
                return new RawSceneControl
                {
                    Type = RawEventType.SceneControl,
                    Timing = tick,
                    Arguments = new List<object>(),
                    SceneControlTypeName = type,
                    TimingGroup = timingGroup,
                    Line = evt.LineNumber
                };
            }

            // cast types
            var param = evt.Values.GetRange(2, evt.Values.Count - 2);

            var typedParam = param.Select(x => x.Type switch
            {
                AntlrValueType.String => (object)x.GetStringValue(),
                AntlrValueType.Integer => (object)(float)x.GetIntegerValue(),
                AntlrValueType.Algebraic => (object)(float)x.GetAlgebraicValue(),

                _ => throw new ChartReaderException(evt.Raw, RawEventType.SceneControl, evt, ChartError.Kind.Parsing)
            }).ToList();

            return new RawSceneControl
            {
                Type = RawEventType.SceneControl,
                Timing = tick,
                Arguments = typedParam,
                SceneControlTypeName = type,
                TimingGroup = timingGroup,
                Line = evt.LineNumber
            };
        }

        public virtual RawCamera ParseCamera(AntlrEvent evt, int timingGroup)
        {
            var validator = new ChartReaderValidator(evt, RawEventType.Camera);

            validator.Require(evt.Values.Count == 9);
            validator.Require(evt.Values[0].TryGetIntegerValue(out var tick));
            validator.Require(evt.Values[1].TryGetAlgebraicValue(out var mx));
            validator.Require(evt.Values[2].TryGetAlgebraicValue(out var my));
            validator.Require(evt.Values[3].TryGetAlgebraicValue(out var mz));
            validator.Require(evt.Values[4].TryGetAlgebraicValue(out var rx));
            validator.Require(evt.Values[5].TryGetAlgebraicValue(out var ry));
            validator.Require(evt.Values[6].TryGetAlgebraicValue(out var rz));
            validator.Require(evt.Values[7].TryGetStringValue(out var type));
            validator.Require(evt.Values[8].TryGetIntegerValue(out var duration) && duration >= 0, errorKind: ChartError.Kind.DurationNegative);

            return new RawCamera
            {
                Type = RawEventType.Camera,
                TimingGroup = timingGroup,
                Timing = tick,
                Duration = duration,
                Move = new Vector3((float)mx, (float)my, (float)mz),
                Rotate = new Vector3((float)rx, (float)ry, (float)rz),
                CameraType = type,
                Line = evt.LineNumber
            };
        }

        public virtual RawInclude ParseInclude(AntlrEvent evt)
        {
            var validator = new ChartReaderValidator(evt, RawEventType.Include);

            validator.Require(evt.Values.Count == 1);
            validator.Require(evt.Values[0].TryGetStringValue(out var file));

            return new RawInclude
            {
                Type = RawEventType.Include,
                File = file
            };
        }

        public virtual RawFragment ParseFragment(AntlrEvent evt)
        {
            var validator = new ChartReaderValidator(evt, RawEventType.Fragment);

            validator.Require(evt.Values.Count == 2);
            validator.Require(evt.Values[0].TryGetIntegerValue(out var tick));
            validator.Require(evt.Values[1].TryGetStringValue(out var file));

            return new RawFragment
            {
                Type = RawEventType.Fragment,
                Timing = tick,
                File = file
            };
        }

        private Result<ChartFileErrors> AddInclude(RawInclude include)
        {
            AllIncludes.Add(SwitchFileName(FullPath, include.File));

            ChartReader extReader = ChartReaderFactory.GetReader(FileAccess, FullPath, include.File);
            extReader.BlockReferences(AllIncludes, AllFragments);
            Result<ChartFileErrors> parseResult = extReader.Parse();
            if (parseResult.IsError)
            {
                return parseResult.Error;
            }

            foreach (RawTimingGroup group in extReader.TimingGroups)
            {
                group.Editable = true;
                group.File = Path.Combine(RelativeDirectory, group.File);
            }

            References.Add(extReader);
            return Result<ChartFileErrors>.Ok();
        }

        private Result<ChartFileErrors> AddFragment(RawFragment fragment)
        {
            AllFragments.Add(SwitchFileName(FullPath, fragment.File));

            ChartReader extReader = ChartReaderFactory.GetReader(FileAccess, FullPath, fragment.File);
            extReader.BlockReferences(AllIncludes, AllFragments);
            Result<ChartFileErrors> parseResult = extReader.Parse();
            if (parseResult.IsError)
            {
                return parseResult.Error;
            }

            foreach (RawTimingGroup group in extReader.TimingGroups)
            {
                group.Editable = false;
                group.File = Path.Combine(RelativeDirectory, group.File);
            }

            foreach (RawEvent e in extReader.Events)
            {
                if (!(e is RawTiming && e.Timing == 0))
                {
                    e.Timing += fragment.Timing;
                }

                if (e is RawHold hold)
                {
                    hold.EndTiming += fragment.Timing;
                }

                if (e is RawArc arc)
                {
                    arc.EndTiming = arc.EndTiming + fragment.Timing;

                    if (arc.ArcTaps.Count > 0)
                    {
                        foreach (var arcArcTap in arc.ArcTaps)
                        {
                            arcArcTap.Timing = arcArcTap.Timing + fragment.Timing;
                        }
                    }
                }
            }

            References.Add(extReader);
            return Result<ChartFileErrors>.Ok();
        }
    }
}