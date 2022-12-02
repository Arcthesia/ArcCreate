using System.IO;

namespace ArcCreate.ChartFormat
{
    /// <summary>
    /// Create a writer based on target file name.
    /// </summary>
    public class ChartFileWriterFactory
    {
        /// <summary>
        /// Create a writer based on the target file name.
        /// </summary>
        /// <param name="file">The target file name.</param>
        /// <returns>The writer suitable for the file's extension.</returns>
        public static IChartFileWriter GetWriterFromFilename(string file)
        {
            string extension = Path.GetExtension(file);
            switch (extension)
            {
                case "aff":
                    return new AffChartFileWriter();
                default:
                    return new AffChartFileWriter();
            }
        }
    }
}