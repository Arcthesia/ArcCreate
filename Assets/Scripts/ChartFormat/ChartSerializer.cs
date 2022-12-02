using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArcCreate.ChartFormat
{
    /// <summary>
    /// Class for serializing a chart into possibly multiple chart files.
    /// </summary>
    public class ChartSerializer
    {
        private readonly IFileAccessWrapper fileAccess;
        private readonly string directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartSerializer"/> class.
        /// </summary>
        /// <param name="fileAccess">The implementation of <see cref="IFileAccessWrapper"/>.</param>
        /// <param name="directory">The target directory to serialize chart file(s) into.</param>
        public ChartSerializer(IFileAccessWrapper fileAccess, string directory)
        {
            this.fileAccess = fileAccess;
            this.directory = directory;
        }

        /// <summary>
        /// Start writing to the target directory.
        /// </summary>
        /// <param name="audioOffset">The audio offset option.</param>
        /// <param name="density">The timing point density factor option.</param>
        /// <param name="groups">List of timing groups,
        /// each being a RawTimingGroup property object, and an IEnumerable of events.</param>
        public void Write(int audioOffset, float density, IEnumerable<(RawTimingGroup properties, IEnumerable<RawEvent> events)> groups)
        {
            var fileMap = new Dictionary<string, List<(RawTimingGroup properties, IEnumerable<RawEvent> events)>>();

            // Organize into files
            foreach (var group in groups)
            {
                if (!fileMap.ContainsKey(group.properties.File))
                {
                    fileMap.Add(group.properties.File, new List<(RawTimingGroup, IEnumerable<RawEvent>)>());
                }

                fileMap[group.properties.File].Add(group);
            }

            foreach (var pair in fileMap)
            {
                string file = pair.Key;
                var group = pair.Value;

                if (group.Any(g => !g.properties.Editable))
                {
                    continue;
                }

                StreamWriter stream = fileAccess.WriteFile(Path.Combine(directory, file));
                IChartFileWriter writer = ChartFileWriterFactory.GetWriterFromFilename(file);
                writer.Write(stream, audioOffset, density, group);
            }
        }
    }
}