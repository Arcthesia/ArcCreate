using System.Collections.Generic;

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
        /// <param name="filename">The file name of the chart file, should be the same as written in include or fragment aff command.</param>
        public ChartReader(IFileAccessWrapper fileAccess, string relativeDirectory, string fullPath, string filename)
        {
            FileAccess = fileAccess;
            RelativeDirectory = relativeDirectory;
            FullPath = fullPath;
            Filename = filename;
        }

        // Output
        public int AudioOffset { get; protected set; } = 0;

        public float TimingPointDensity { get; protected set; } = 1;

        public List<RawEvent> Events { get; private set; } = new List<RawEvent>();

        public List<RawTimingGroup> TimingGroups { get; private set; } = new List<RawTimingGroup>();

        protected string RelativeDirectory { get; set; }

        protected string FullPath { get; set; }

        protected string Filename { get; set; }

        protected IFileAccessWrapper FileAccess { get; set; }

        protected HashSet<string> AllIncludes { get; private set; } = new HashSet<string>();

        protected HashSet<string> AllFragments { get; private set; } = new HashSet<string>();

        protected int TotalTimingGroup { get; set; } = 1;

        protected int CurrentTimingGroup { get; set; } = 0;

        protected List<ChartReader> References { get; } = new List<ChartReader>();

        /// <summary>
        /// Start parsing with the provided <see cref="FullPath"/> and <see cref="Filename"/>.
        /// </summary>
        public void Parse()
        {
            TotalTimingGroup = 1;
            CurrentTimingGroup = 0;
            TimingGroups.Add(new RawTimingGroup() { File = Filename });

            string[] lines = FileAccess.ReadFileByLines(FullPath);
            bool atHeader = true;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (atHeader)
                {
                    ParseHeaderLine(line, FullPath, i, out bool endOfHeader);
                    if (endOfHeader)
                    {
                        atHeader = false;
                    }
                }
                else
                {
                    ParseLine(line, FullPath, i);
                }
            }

            foreach (ChartReader reference in References)
            {
                int referenceBaseGroupCount = 0;
                int removedBaseGroup = 0;
                foreach (RawEvent e in reference.Events)
                {
                    if (e.TimingGroup == 0)
                    {
                        referenceBaseGroupCount += 1;
                    }
                }

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

            FinalValidity();
            Events.Sort((RawEvent a, RawEvent b) => { return a.Timing.CompareTo(b.Timing); });
        }

        public abstract void ParseLine(string line, string path, int lineNumber);

        public abstract void ParseHeaderLine(string line, string path, int lineNumber, out bool endOfHeader);

        public virtual void FinalValidity()
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
                throw new ChartFormatException(I18n.S("Format.Exception.BaseTimingInvalid"));
            }
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
    }
}