using System;
using System.Collections.Generic;
using System.IO;
using ArcCreate.Utility.Parser;

namespace ArcCreate.ChartFormat
{
    /// <summary>
    /// Object for reading a .aff chart file.
    /// </summary>
    public partial class AffChartReader : ChartReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AffChartReader"/> class. You should use <see cref="ChartReaderFactory"/> to instantiate instead.
        /// </summary>
        /// <param name="fileAccess">File access wrapper. You should normally use <see cref="PhysicalFileAccess"/>.</param>
        /// <param name="relativeDirectory">The directory relative to the base folder.</param>
        /// <param name="fullPath">The absolute path leading to the file.</param>
        /// <param name="filename">The file name. Passed as-is from include and fragment aff commands.</param>
        public AffChartReader(IFileAccessWrapper fileAccess, string relativeDirectory, string fullPath, string filename)
            : base(fileAccess, relativeDirectory, fullPath, filename)
        {
            TimingPointDensity = 1;
            AudioOffset = 0;
        }

        public override void ParseLine(string line, string path, int lineNumber)
        {
            RawEventType type = DetermineType(line);
            try
            {
                switch (type)
                {
                    case RawEventType.Timing:
                        Events.Add(ParseTiming(line, lineNumber));
                        break;

                    case RawEventType.Tap:
                        Events.Add(ParseTap(line, lineNumber));
                        break;

                    case RawEventType.Hold:
                        Events.Add(ParseHold(line, lineNumber));
                        break;

                    case RawEventType.Arc:
                        Events.Add(ParseArc(line, lineNumber));
                        break;

                    case RawEventType.Camera:
                        Events.Add(ParseCamera(line, lineNumber));
                        break;

                    case RawEventType.SceneControl:
                        Events.Add(ParseSceneControl(line, lineNumber));
                        break;

                    case RawEventType.TimingGroup:
                        TotalTimingGroup++;
                        CurrentTimingGroup = TotalTimingGroup - 1;
                        TimingGroups.Add(ParseTimingGroup(line, Filename));
                        break;

                    case RawEventType.TimingGroupEnd:
                        CurrentTimingGroup = 0;
                        break;

                    case RawEventType.Include:
                        string inclPath = ParseInclude(line);
                        string fullInclPath = SwitchFileName(FullPath, inclPath);

                        if (AllIncludes.Contains(fullInclPath))
                        {
                            throw new ChartFormatException(
                                RawEventType.Include,
                                line,
                                path,
                                lineNumber,
                                I18n.S("Format.Exception.IncludeReferencedMultipleTimes"));
                        }

                        if (AllFragments.Contains(fullInclPath))
                        {
                            throw new ChartFormatException(
                                RawEventType.Fragment,
                                line,
                                path,
                                lineNumber,
                                I18n.S("Format.Exception.IncludeAReferencedFragment"));
                        }

                        AddInclude(inclPath);
                        break;

                    case RawEventType.Fragment:
                        (int timing, string fragPath) = ParseFragment(line);
                        string fullFragPath = SwitchFileName(FullPath, fragPath);

                        if (AllIncludes.Contains(fullFragPath))
                        {
                            throw new ChartFormatException(
                                RawEventType.Fragment,
                                line,
                                path,
                                lineNumber,
                                I18n.S("Format.Exception.IncludeReferencedMultipleTimes"));
                        }

                        AddFragment(timing, fragPath);
                        break;
                }
            }
            catch (ChartFormatException ex)
            {
                throw new ChartFormatException(type, line, path, lineNumber, ex.Reason, ex.ShouldAbort, ex.StartCharPos, ex.EndCharPos);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ChartFormatException(type, line, path, lineNumber, I18n.S("Format.Exception.WrongSyntax"));
            }
            catch (Exception ex)
            {
                throw new ChartFormatException(I18n.S(
                    "Format.Exception.UnknownException",
                    new Dictionary<string, string>()
                    {
                        { "Exception", ex.Message },
                        { "StackTrace", ex.StackTrace },
                    }));
            }
        }

        public override void ParseHeaderLine(string line, string path, int lineNumber, out bool endOfHeader)
        {
            if (line.StartsWith("AudioOffset"))
            {
                if (!Evaluator.TryInt(line.Substring("AudioOffset:".Length), out int offset))
                {
                    throw new ChartFormatException(
                        RawEventType.Header,
                        line,
                        path,
                        lineNumber,
                        I18n.S("Format.Exception.AudioOffsetInvalid"));
                }

                endOfHeader = false;
                AudioOffset = offset;
            }
            else if (line.StartsWith("TimingPointDensityFactor"))
            {
                if (!Evaluator.TryFloat(line.Substring("TimingPointDensityFactor:".Length), out float density))
                {
                    throw new ChartFormatException(
                        RawEventType.Header,
                        line,
                        path,
                        lineNumber,
                        I18n.S("Format.Exception.TimingPointDensityFactorInvalid"));
                }

                endOfHeader = false;
                TimingPointDensity = density;
            }
            else if (line == "-")
            {
                endOfHeader = true;
            }
            else
            {
                throw new ChartFormatException(
                    RawEventType.Header,
                    line,
                    path,
                    lineNumber,
                    I18n.S("Format.Exception.InvalidHeaderLine"),
                    shouldAbort: true);
            }
        }

        public override void FinalValidity()
        {
            base.FinalValidity();

            if (CurrentTimingGroup != 0)
            {
                throw new ChartFormatException(I18n.S("Format.Exception.TimingGroupPairInvalid"));
            }
        }

        private RawEventType DetermineType(string line)
        {
            if (line.StartsWith("("))
            {
                return RawEventType.Tap;
            }
            else if (line.StartsWith("timing("))
            {
                return RawEventType.Timing;
            }
            else if (line.StartsWith("hold("))
            {
                return RawEventType.Hold;
            }
            else if (line.StartsWith("arc("))
            {
                return RawEventType.Arc;
            }
            else if (line.StartsWith("camera("))
            {
                return RawEventType.Camera;
            }
            else if (line.StartsWith("scenecontrol("))
            {
                return RawEventType.SceneControl;
            }
            else if (line.StartsWith("timinggroup("))
            {
                return RawEventType.TimingGroup;
            }
            else if (line.StartsWith("include("))
            {
                return RawEventType.Include;
            }
            else if (line.StartsWith("fragment("))
            {
                return RawEventType.Fragment;
            }
            else if (line.StartsWith("};"))
            {
                return RawEventType.TimingGroupEnd;
            }

            return RawEventType.Unknown;
        }

        private void AddInclude(string file)
        {
            AllIncludes.Add(SwitchFileName(FullPath, file));
            Events.Add(new RawInclude()
            {
                Timing = 0,
                Type = RawEventType.Include,
                TimingGroup = CurrentTimingGroup,
                File = file,
            });

            ChartReader extReader = ChartReaderFactory.GetReader(FileAccess, FullPath, file);
            extReader.BlockReferences(AllIncludes, AllFragments);
            extReader.Parse();
            foreach (RawTimingGroup group in extReader.TimingGroups)
            {
                group.Editable = true;
                group.File = Path.Combine(RelativeDirectory, group.File);
            }

            References.Add(extReader);
        }

        private void AddFragment(int timing, string file)
        {
            AllFragments.Add(SwitchFileName(FullPath, file));
            Events.Add(new RawFragment()
            {
                Timing = timing,
                Type = RawEventType.Fragment,
                TimingGroup = CurrentTimingGroup,
                File = file,
            });

            ChartReader extReader = ChartReaderFactory.GetReader(FileAccess, FullPath, file);
            extReader.BlockReferences(AllIncludes, AllFragments);
            extReader.Parse();
            foreach (RawTimingGroup group in extReader.TimingGroups)
            {
                group.Editable = false;
                group.File = Path.Combine(RelativeDirectory, group.File);
            }

            foreach (RawEvent e in extReader.Events)
            {
                if (!(e is RawTiming && e.Timing == 0))
                {
                    e.Timing += timing;
                }

                if (e is RawHold)
                {
                    (e as RawHold).EndTiming += timing;
                }

                if (e is RawArc)
                {
                    (e as RawArc).EndTiming += timing;
                }
            }

            References.Add(extReader);
        }
    }
}
