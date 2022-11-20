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
        private readonly List<ChartReader> references = new List<ChartReader>();
        private int totalTimingGroup = 1;
        private int currentTimingGroup = 0;

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
        }

        /// <summary>
        /// Start parsing with the provided <see cref="FullPath"/> and <see cref="Filename"/>.
        /// </summary>
        public override void Parse()
        {
            totalTimingGroup = 1;
            currentTimingGroup = 0;
            TimingGroups.Add(new RawTimingGroup() { File = Filename });

            string[] lines = FileAccess.ReadFileByLines(FullPath);
            ParseLines(lines, FullPath);
        }

        private void ParseLines(string[] lines, string path)
        {
            try
            {
                AudioOffset = Evaluator.Int(lines[0].Replace("AudioOffset:", ""));
            }
            catch (Exception)
            {
                throw new ChartFormatException(
                    RawEventType.Unknown,
                    lines[0],
                    path,
                    1,
                    I.S("Format.Exception.AudioOffsetInvalid"));
            }

            try
            {
                if (lines[1].Contains(":") && lines[1].Substring(0, lines[1].IndexOf(":")) == "TimingPointDensityFactor")
                {
                    bool valid = Evaluator.TryFloat(lines[1].Replace("TimingPointDensityFactor:", ""), out float density);
                    if (!valid)
                    {
                        TimingPointDensity = 1;
                    }
                    else
                    {
                        TimingPointDensity = density;
                    }
                }
            }
            catch (Exception)
            {
                TimingPointDensity = 1;
                throw new ChartFormatException(
                    RawEventType.Unknown,
                    lines[1],
                    path,
                    2,
                    I.S("Format.Exception.TimingPointDensityFactorInvalid"));
            }

            int chartBeginning = 2;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "-")
                {
                    chartBeginning = i + 1;
                    break;
                }
            }

            try
            {
                ParseTiming(lines[chartBeginning]);
            }
            catch (Exception)
            {
                throw new ChartFormatException(RawEventType.Timing, lines[2], path, 3, I.S("Format.Exception.BaseTimingInvalid"));
            }

            for (int i = chartBeginning + 1; i < lines.Length; ++i)
            {
                string line = lines[i].Trim();
                RawEventType type = DetermineType(line);
                try
                {
                    switch (type)
                    {
                        case RawEventType.Timing:
                            Events.Add(ParseTiming(line));
                            break;

                        case RawEventType.Tap:
                            Events.Add(ParseTap(line));
                            break;

                        case RawEventType.Hold:
                            Events.Add(ParseHold(line));
                            break;

                        case RawEventType.Arc:
                            Events.Add(ParseArc(line));
                            break;

                        case RawEventType.Camera:
                            Events.Add(ParseCamera(line));
                            break;

                        case RawEventType.SceneControl:
                            Events.Add(ParseSceneControl(line));
                            break;

                        case RawEventType.TimingGroup:
                            totalTimingGroup++;
                            currentTimingGroup = totalTimingGroup - 1;
                            TimingGroups.Add(ParseTimingGroup(line, Filename));
                            line = lines[++i].Trim();
                            type = RawEventType.Timing;
                            Events.Add(ParseTiming(line));
                            break;

                        case RawEventType.TimingGroupEnd:
                            currentTimingGroup = 0;
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
                                    i,
                                    I.S("Format.Exception.IncludeReferencedMultipleTimes"));
                            }

                            if (AllFragments.Contains(fullInclPath))
                            {
                                throw new ChartFormatException(
                                    RawEventType.Fragment,
                                    line,
                                    path,
                                    i,
                                    I.S("Format.Exception.IncludeAReferencedFragment"));
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
                                    i,
                                    I.S("Format.Exception.IncludeReferencedMultipleTimes"));
                            }

                            AddFragment(timing, fragPath);
                            break;
                    }
                }
                catch (ChartFormatException ex)
                {
                    throw new ChartFormatException(type, path, line, i + 1, ex.Message);
                }
                catch (Exception)
                {
                    throw new ChartFormatException(type, path, line, i + 1);
                }
            }

            foreach (ChartReader reference in references)
            {
                foreach (RawEvent e in reference.Events)
                {
                    e.TimingGroup += TimingGroups.Count;
                }

                Events.AddRange(reference.Events);
                TimingGroups.AddRange(reference.TimingGroups);
            }

            Events.Sort((RawEvent a, RawEvent b) => { return a.Timing.CompareTo(b.Timing); });
            if (currentTimingGroup != 0)
            {
                throw new ChartFormatException(I.S("Format.Exception.TimingGroupPairInvalid"));
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
                TimingGroup = currentTimingGroup,
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

            references.Add(extReader);
        }

        private void AddFragment(int timing, string file)
        {
            AllFragments.Add(SwitchFileName(FullPath, file));
            Events.Add(new RawFragment()
            {
                Timing = timing,
                Type = RawEventType.Include,
                TimingGroup = currentTimingGroup,
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
                e.Timing += timing;
                if (e is RawHold)
                {
                    (e as RawHold).EndTiming += timing;
                }

                if (e is RawArc)
                {
                    (e as RawArc).EndTiming += timing;
                }
            }

            references.Add(extReader);
        }
    }
}