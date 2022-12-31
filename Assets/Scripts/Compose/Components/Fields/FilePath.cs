using System.IO;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Class representing a file path.
    /// Note that this does not handle any actual I/O operation, and will only serve as a convenient way to
    /// generate path values.
    /// </summary>
    public class FilePath
    {
        private FilePath()
        {
        }

        /// <summary>
        /// Gets the full path, meant to point to a file that exists.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath { get; private set; }

        /// <summary>
        /// Gets the shortened path, relative to <see cref="LocalToDirectory"/>.
        /// In the case that this path is global, this value will be the same as <see cref="FullPath"/>.
        /// Example: If the <see cref="FullPath"/> is "C:/Folder/File.ext", and <see cref="LocalToDirectory"/> is "C:/Folder",
        /// then this value will be "File.ext".
        /// </summary>
        /// <value><The shortened path.</value>
        public string ShortenedPath { get; private set; }

        /// <summary>
        /// Gets the directory that this path is relative to.
        /// In the case that this path is global, this value will be null.
        /// </summary>
        /// <value>The directory path.</value>
        public string LocalToDirectory { get; private set; }

        /// <summary>
        /// Gets original path this path was generated from.
        /// In the case that this path is global, this value will be the same as <see cref="FullPath"/>.
        /// </summary>
        /// <value>The original path.</value>
        public string OriginalPath { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not the file should be copied to the new path.
        /// </summary>
        /// <value>The boolean value.</value>
        public bool ShouldCopy { get; private set; }

        /// <summary>
        /// Generate a file path that's local to a directory.
        /// If the provided path is not already local to the directory, then a new path that's local to the directory
        /// will be generateed.
        /// Specifically, the new path is calculated as: The directory + Filename of the original path.
        /// </summary>
        /// <param name="directory">The directory that the file is local to.</param>
        /// <param name="originalPath">The original path to generate from.</param>
        /// <returns>A file path instance.</returns>
        public static FilePath Local(string directory, string originalPath)
        {
            string fullDirectory = Path.GetFullPath(directory);
            string fullOriginal = Path.GetFullPath(originalPath);

            string fullPath = string.Empty;
            string shortenedPath = string.Empty;
            bool shouldCopy = false;

            if (fullOriginal.StartsWith(fullDirectory))
            {
                fullPath = fullOriginal;
                shortenedPath = fullOriginal.Substring(fullDirectory.Length);
                shortenedPath = shortenedPath.TrimStart('/');
                shouldCopy = false;
            }
            else
            {
                string fileName = Path.GetFileName(fullOriginal);
                fullPath = Path.Combine(fullDirectory, fileName);
                shortenedPath = fileName;
                shouldCopy = true;
            }

            FilePath filePath = new FilePath
            {
                FullPath = fullPath,
                ShortenedPath = shortenedPath,
                LocalToDirectory = directory,
                OriginalPath = originalPath,
                ShouldCopy = shouldCopy,
            };
            return filePath;
        }

        /// <summary>
        /// Generate a global file path instance.
        /// </summary>
        /// <param name="path">The path to generate from.</param>
        /// <returns>A file path instance.</returns>
        public static FilePath Global(string path)
        {
            string fullPath = Path.GetFullPath(path);
            FilePath filePath = new FilePath
            {
                FullPath = fullPath,
                ShortenedPath = fullPath,
                OriginalPath = fullPath,
                LocalToDirectory = null,
                ShouldCopy = false,
            };
            return filePath;
        }
    }
}