using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using ArcCreate.ChartFormat.Grammar;

namespace ArcCreate.ChartFormat
{
    public abstract class ChartReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartReader"/> class.
        /// </summary>
        /// <param name="fileAccess">The implementation of <see cref="IFileAccessWrapper"/> interface.</param>
        /// <param name="relativeDirectory">The directory relative to the base directory of the chart file.
        /// Should be an empty string for the base chart file.</param>
        /// <param name="fullPath">The absolute path to the chart file.</param>
        /// <param name="fileName">The file name of the chart file, should be the same as written in include or fragment aff command.</param>
        public ChartReader(IFileAccessWrapper fileAccess, string relativeDirectory, string fullPath, string fileName)
        {
            FileAccess = fileAccess;
            RelativeDirectory = relativeDirectory;
            FullPath = fullPath;
            FileName = fileName;
        }

        // Output
        public int AudioOffset { get; protected set; } = 0;

        public float TimingPointDensity { get; protected set; } = 1;

        public List<RawEvent> Events { get; private set; } = new List<RawEvent>();

        public List<RawTimingGroup> TimingGroups { get; private set; } = new List<RawTimingGroup>();

        protected string RelativeDirectory { get; set; }

        protected string FullPath { get; set; }

        protected string FileName { get; set; }

        protected IFileAccessWrapper FileAccess { get; set; }

        protected HashSet<string> AllIncludes { get; private set; } = new HashSet<string>();

        protected HashSet<string> AllFragments { get; private set; } = new HashSet<string>();

        protected List<ChartReader> References { get; } = new List<ChartReader>();

        public static AntlrEventSegment ParseEvents(string raw, Action<UniversalAffChartParser> postParserAction = null)
        {
            var antlrInput = new AntlrInputStream(raw);
            var lexer = new UniversalAffChartLexer(antlrInput);
            var tokens = new CommonTokenStream(lexer);
            var parser = new UniversalAffChartParser(tokens);
            var visitor = new UniversalChartVisitor();

            postParserAction?.Invoke(parser);
            
            var segment = visitor.VisitChartTyped(parser.chart());
            return segment;
        }
        
        /// <summary>
        /// Start parsing with the provided <see cref="FullPath"/> and <see cref="FileName"/>.
        /// </summary>
        /// <returns>Result containing any errors found within the chart file.</returns>
        public abstract Result<ChartFileErrors> Parse();

        public abstract Result<(int startLine, Dictionary<string, string>), ChartError> ParseHeader(string[] lines);

        public virtual Result<ChartError> FinalValidity()
        {
            bool foundBaseTiming = false;
            foreach (var ev in Events)
            {
                if (ev is RawTiming && ev.TimingGroup == 0 && ev.Timing == 0)
                {
                    foundBaseTiming = true;
                    break;
                }
            }

            if (!foundBaseTiming)
            {
                return ChartError.Format(RawEventType.Timing, ChartError.Kind.BaseTimingInvalid);
            }

            return Result<ChartError>.Ok();
        }

        /// <summary>
        /// Inject include and fragment references to this reader's blocklist.
        /// </summary>
        /// <param name="includes">List of include references.</param>
        /// <param name="fragments">List of fragment references.</param>
        public void BlockReferences(IEnumerable<string> includes, IEnumerable<string> fragments)
        {
            AllIncludes.UnionWith(includes);
            AllFragments.UnionWith(fragments);
        }

        public IEnumerable<string> GetReferencedFiles()
        {
            HashSet<string> files = new HashSet<string>();
            foreach (var tg in TimingGroups)
            {
                files.Add(tg.File);
            }

            foreach (var ev in Events)
            {
                if (ev is RawArc a && !string.IsNullOrWhiteSpace(a.Sfx) && a.Sfx != "none")
                {
                    string sfx = a.Sfx;
                    if (sfx.EndsWith("_wav"))
                    {
                        sfx = sfx.Substring(0, sfx.Length - "_wav".Length) + ".wav";
                    }

                    if (!sfx.EndsWith(".wav"))
                    {
                        sfx = sfx + ".wav";
                    }

                    files.Add(sfx);
                }
            }

            return files;
        }

        public virtual (RawTimingGroup, List<RawEvent>) ParseTimingGroup(AntlrEvent evt, int currentTimingGroup)
        {
            return (
                ParseTimingGroupProperties(evt.Raw, evt),
                evt.Segment.Events.Select(tgEvent => ParseEvent(tgEvent, currentTimingGroup)).ToList()
            );
        }

        public abstract RawTimingGroup ParseTimingGroupProperties(string raw, AntlrEvent evt);

        public abstract RawEvent ParseEvent(AntlrEvent evt, int timingGroup);

        protected virtual string SwitchFileName(string currentPath, string target)
        {
            string dir = Path.GetDirectoryName(currentPath);
            return Path.Combine(dir, target);
        }
    }
}