using System.IO;

namespace Arc.ChartFormat
{
    /// <summary>
    /// Create and initialize objects of type <see cref="ChartReader"/> based on the provided path.
    /// </summary>
    public class ChartReaderFactory
    {
        /// <summary>
        /// Create reader from full path to a chart file.
        /// </summary>
        /// <param name="fileAccess">Implementation of <see cref="IFileAccessWrapper"/>.</param>
        /// <param name="fullPath">The full path to the chart file.</param>
        /// <returns>The <see cref="ChartReader"/> set up to parse the provided chart file.</returns>
        public static ChartReader GetReader(IFileAccessWrapper fileAccess, string fullPath)
        {
            string extension = Path.GetExtension(fullPath);
            ChartReader reader = GetReaderFromExtension(
                fileAccess,
                extension,
                relativeDirectory: "",
                fullPath: fullPath,
                filename: Path.GetFileName(fullPath));
            return reader;
        }

        /// <summary>
        /// Create reader from the currently parsing file path and a relative path to the new file to parse.\n
        /// Example:\n
        /// currentPath = D:/testchart/2.aff; file = 3.aff\n
        /// Means the reader will read at D:/testchart/3.aff.
        /// </summary>
        /// <param name="fileAccess">Implementation of <see cref="IFileAccessWrapper"/>.</param>
        /// <param name="currentPath">The path to the current file.</param>
        /// <param name="file">The relative path to the file to parse.</param>
        /// <returns>The <see cref="ChartReader"/> set up to parse the new file.</returns>
        public static ChartReader GetReader(IFileAccessWrapper fileAccess, string currentPath, string file)
        {
            string extension = Path.GetExtension(file);
            ChartReader reader = GetReaderFromExtension(
                fileAccess,
                extension,
                relativeDirectory: Path.GetDirectoryName(file),
                fullPath: Path.Combine(Path.GetDirectoryName(currentPath), file),
                filename: file);
            return reader;
        }

        private static ChartReader GetReaderFromExtension(
            IFileAccessWrapper fileAccess,
            string extension,
            string relativeDirectory,
            string fullPath,
            string filename)
        {
            switch (extension)
            {
                case "aff":
                    return new AffChartReader(fileAccess, relativeDirectory, fullPath, filename);
                default:
                    return new AffChartReader(fileAccess, relativeDirectory, fullPath, filename);
            }
        }
    }
}