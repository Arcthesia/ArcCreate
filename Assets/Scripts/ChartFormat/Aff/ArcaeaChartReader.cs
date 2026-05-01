using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using ArcCreate.ChartFormat.Grammar;
using ArcCreate.Utility.Parser;
using UnityEngine;

namespace ArcCreate.ChartFormat
{
    /// <summary>
    /// Object for reading a .aff chart file.
    /// </summary>
    public class ArcaeaChartReader : ArcCreateChartReader
    {
        public new static ArcaeaChartReader Instance = new(null, string.Empty, string.Empty, string.Empty);

        public static readonly Color DesignantColor = new Color32(240, 41, 97, byte.MaxValue);

        public ArcaeaChartReader(IFileAccessWrapper fileAccess, string relativeDirectory, string fullPath,
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

            var antlrInput = new AntlrInputStream(string.Join("\n", lines.Value.Skip(headerLineNumber)));
            var lexer = new UniversalAffChartLexer(antlrInput);
            var tokens = new CommonTokenStream(lexer);
            var parser = new UniversalAffChartParser(tokens);

            parser.RemoveErrorListeners();
            parser.AddErrorListener(new AntlrChartErrorListener(lines.Value));

            var visitor = new UniversalChartVisitor();

            try
            {
                var chartSegment = visitor.VisitChartTyped(parser.chart());

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

        public override RawTimingGroup ParseTimingGroupProperties(string raw, AntlrEvent evt)
        {
            var validator = new ChartReaderValidator(evt, RawEventType.TimingGroup);

            // timing group properties are connected by '_' in Arcaea
            // an empty AntlrValue takes one place in Values list, so its count should be constant 1 
            // but for generality we assert its count to be <= 1
            validator.Require(evt.Values.Count <= 1, GetTimingGroupPropertiesRaw(raw));

            var propDict = new Dictionary<string, string>();

            if (evt.Values.Count > 0 && !evt.Values[0].IsEmpty)
            {
                // https://regex101.com/r/wTAqy8/2
                const string pattern = @"([a-zA-Z]+)(-?(0|([1-9][0-9]*))(\.\d+)?)?";
                var regex = new Regex(pattern, RegexOptions.Compiled);

                foreach (var propRaw in evt.Values[0].GetStringValue().Split("_"))
                {
                    var match = regex.Match(propRaw);

                    if (!match.Success)
                    {
                        throw new AntlrParseException("Invalid timing group properties", evt.Raw,
                                                      RawEventType.AntlrValue, evt.LineNumber, evt.ColumnNumber);
                    }

                    string name = match.Groups[1].Value;
                    string value = match.Groups[2].Success ? match.Groups[2].Value : null;

                    propDict.Add(name, value);
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
                        // https://github.com/freeze-dolphin/aff-compose/blob/17d0948c3f3726336661df4b68b0e5e2a86e3ef6/src/commonMain/kotlin/com/tairitsu/compose/filter/ShimFilter.kt#L41-L45
                        case "anglex":
                            valid = Evaluator.TryFloat(value, out val);
                            prop.AngleX = valid ? val / 10 : 0;
                            break;
                        case "angley":
                            valid = Evaluator.TryFloat(value, out val);
                            prop.AngleY = valid ? val / -10 : 0;
                            break;

                        // don't throw exceptions to allow user add other identifiers for tg (but we don't parse them)
                        /*
                        default:
                            throw new ChartReaderException(raw, RawEventType.TimingGroup, evt,
                                ChartError.Kind.TimingGroupPropertiesInvalid);
                        */
                    }
                }
                else
                {
                    switch (type.ToLower())
                    {
                        case "noinput":
                            prop.NoInput = true;
                            break;
                        case "fadingholds":
                            prop.FadingHolds = true;
                            break;

                        // don't throw exceptions to allow user add other identifiers for tg (but we don't parse them)
                        /*
                        default:
                            throw new ChartReaderException(raw, RawEventType.TimingGroup, evt,
                                ChartError.Kind.TimingGroupPropertiesInvalid);
                        */
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

            _ => throw new ChartReaderException(evt.Raw, RawEventType.Unknown, evt, ChartError.Kind.Parsing)
        };

        public override RawTap ParseTap(AntlrEvent evt, int timingGroup)
        {
            var validator = new ChartReaderValidator(evt, RawEventType.Tap);

            validator.Require(evt.Values.Count == 2);
            validator.Require(evt.Values[0].TryGetIntegerValue(out var tick));

            float lane;
            if (evt.Values[1].TryGetIntegerValue(out int intLane))
            {
                lane = intLane;
            }
            else
            {
                validator.Require(evt.Values[1].TryGetAlgebraicValue(out double floatedLane));
                lane = (float)ParsingFormula.ArcaeaFloatedLaneToLane(floatedLane);
            }

            return new RawTap
            {
                Type = RawEventType.Tap,
                Timing = tick,
                Lane = lane,
                TimingGroup = timingGroup,
                Line = evt.LineNumber
            };
        }

        public override RawHold ParseHold(AntlrEvent evt, int timingGroup)
        {
            var validator = new ChartReaderValidator(evt, RawEventType.Hold);

            validator.Require(evt.Values.Count == 3);
            validator.Require(evt.Values[0].TryGetIntegerValue(out var tick));
            validator.Require(evt.Values[1].TryGetIntegerValue(out var endTick));

            float lane;
            if (evt.Values[2].TryGetIntegerValue(out int intLane))
            {
                lane = intLane;
            }
            else
            {
                validator.Require(evt.Values[2].TryGetAlgebraicValue(out double floatedLane));
                lane = (float)ParsingFormula.ArcaeaFloatedLaneToLane(floatedLane);
            }

            if (Mathf.Approximately(endTick, tick))
                throw new ChartReaderException(evt.Raw, RawEventType.Hold, evt, ChartError.Kind.DurationZero);

            if (endTick < tick)
                throw new ChartReaderException(evt.Raw, RawEventType.Hold, evt, ChartError.Kind.DurationNegative);

            return new RawHold
            {
                Type = RawEventType.Hold,
                Timing = tick,
                EndTiming = endTick,
                Lane = lane,
                TimingGroup = timingGroup,
                Line = evt.LineNumber
            };
        }

        public override RawArc ParseArc(AntlrEvent evt, int timingGroup)
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

            var isDesignant = arcType == "designant";
            var isTrace = arcType is "true" or "designant";

            RawArc arc;

            if (tick == endTick &&
                Mathf.Approximately((float)yStart, (float)yEnd) &&
                color == 3)
            {
                // var-len arctap

                var xCenter = (float)(xStart + xEnd) / 2;
                var yCenter = (float)((yStart + yEnd) / 2);

                var width = Mathf.Abs((float)(xStart - xEnd)) * 2;

                arc = new RawArc
                {
                    Type = RawEventType.Arc,
                    Timing = tick,
                    EndTiming = tick + 1,
                    XStart = xCenter,
                    XEnd = xCenter,
                    LineType = lineType,
                    YStart = yCenter,
                    YEnd = yCenter,
                    Color = color,
                    IsTrace = true,
                    ArcTaps = new List<RawArcTap>
                    {
                        new()
                        {
                            Type = RawEventType.ArcTap,
                            Timing = tick,
                            TimingGroup = timingGroup,
                            Width = width,
                            Line = evt.LineNumber,
                            CharacterStart = evt.ColumnNumber,
                            Length = evt.Raw.Length
                        }
                    },
                    Sfx = hitSound,
                    TimingGroup = timingGroup,
                    Line = evt.LineNumber
                };
            }
            else
            {
                // normal arc

                arc = new RawArc
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
            }

            TryApplyDesignant(arc, isDesignant);

            return arc;

            void TryApplyDesignant(RawArc rawArc, bool shouldApply)
            {
                rawArc.TraceColor = shouldApply ? DesignantColor : null;
            }
        }

        protected override RawArcTap ParseArcTap(AntlrEvent evt, int timingGroup, int parentTick, int parentEndTick)
        {
            if (evt.Name != "arctap")
            {
                throw new ChartReaderException(evt.Raw, RawEventType.ArcTap, evt, ChartError.Kind.Parsing);
            }

            var validator = new ChartReaderValidator(evt, RawEventType.ArcTap);
            validator.Require(evt.Values.Count is 1);
            validator.Require(evt.Values[0].TryGetIntegerValue(out var tick));

            if (tick < parentTick || tick > parentEndTick)
            {
                throw new ChartReaderException(evt.Raw, RawEventType.ArcTap, evt, ChartError.Kind.ArcTapOutOfRange);
            }

            return new RawArcTap
            {
                Type = RawEventType.ArcTap,
                Timing = tick,
                TimingGroup = timingGroup,
                Width = 1,
                Line = evt.LineNumber,
                CharacterStart = evt.ColumnNumber,
                Length = evt.Raw.Length
            };
        }

        public override RawSceneControl ParseSceneControl(AntlrEvent evt, int timingGroup)
        {
            const string trackDisplay = "trackdisplay";

            var validator = new ChartReaderValidator(evt, RawEventType.SceneControl);

            validator.Require(evt.Values.Count >= 2);
            validator.Require(evt.Values[0].TryGetIntegerValue(out var tick));
            validator.Require(evt.Values[1].TryGetStringValue(out var type));

            // parameter-less
            if (evt.Values.Count == 2)
                return type.ToLower() switch
                {
                    // https://github.com/freeze-dolphin/aff-compose/blob/17d0948c3f3726336661df4b68b0e5e2a86e3ef6/src/commonMain/kotlin/com/tairitsu/compose/filter/ShimFilter.kt#L29
                    "trackhide" => new RawSceneControl
                    {
                        Type = RawEventType.SceneControl,
                        Timing = tick,
                        Arguments = new List<object>
                        {
                            1000,
                            0
                        },
                        SceneControlTypeName = trackDisplay,
                        TimingGroup = timingGroup,
                        Line = evt.LineNumber
                    },

                    // https://github.com/freeze-dolphin/aff-compose/blob/17d0948c3f3726336661df4b68b0e5e2a86e3ef6/src/commonMain/kotlin/com/tairitsu/compose/filter/ShimFilter.kt#L30
                    "trackshow" => new RawSceneControl
                    {
                        Type = RawEventType.SceneControl,
                        Timing = tick,
                        Arguments = new List<object>
                        {
                            1000,
                            255
                        },
                        SceneControlTypeName = trackDisplay,
                        TimingGroup = timingGroup,
                        Line = evt.LineNumber
                    },

                    _ => new RawSceneControl
                    {
                        Type = RawEventType.SceneControl,
                        Timing = tick,
                        Arguments = new List<object>(),
                        SceneControlTypeName = type,
                        TimingGroup = timingGroup,
                        Line = evt.LineNumber
                    }
                };

            // cast types
            var param = evt.Values.GetRange(2, evt.Values.Count - 2);

            var typedParam = param.Select(x => x.Type switch
            {
                AntlrValueType.String => (object)x.GetStringValue(),
                AntlrValueType.Integer => (object)(float)x.GetIntegerValue(),
                AntlrValueType.Algebraic => (object)(float)x.GetAlgebraicValue(),

                _ => throw new ChartReaderException(evt.Raw, RawEventType.SceneControl, evt, ChartError.Kind.Parsing)
            }).ToList();

            switch (type.ToLower())
            {
                // https://github.com/freeze-dolphin/aff-compose/blob/17d0948c3f3726336661df4b68b0e5e2a86e3ef6/src/commonMain/kotlin/com/tairitsu/compose/filter/ShimFilter.kt#L32-L36
                case trackDisplay:
                    validator.Require(typedParam.Count == 2);
                    validator.Require(float.TryParse(typedParam[0].ToString(), out var durationInSec));
                    validator.Require(float.TryParse(typedParam[1].ToString(), out var alpha));

                    var duration = 1000 * (Mathf.Approximately(durationInSec, 0)
                        ? 1 // defaults to 1 sec
                        : durationInSec);

                    return new RawSceneControl
                    {
                        Timing = tick,
                        Type = RawEventType.SceneControl,
                        Arguments = new List<object>
                        {
                            Mathf.RoundToInt(duration),
                            Mathf.Clamp(alpha, 0, 255)
                        },
                        SceneControlTypeName = trackDisplay,
                        TimingGroup = timingGroup,
                        Line = evt.LineNumber
                    };
                default:
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
        }
    }
}