using System.Collections.Generic;
using System.IO;

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

        /// <summary>
        /// Start parsing with the provided <see cref="FullPath"/> and <see cref="Filename"/>.
        /// </summary>
        public abstract void Parse();

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