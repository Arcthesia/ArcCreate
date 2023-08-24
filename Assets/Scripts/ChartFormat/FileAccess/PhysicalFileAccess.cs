using System;
using System.IO;

namespace ArcCreate.ChartFormat
{
    public class PhysicalFileAccess : IFileAccessWrapper
    {
        public Uri GetFileUri(string path)
        {
            return new Uri(path);
        }

        public Option<string[]> ReadFileByLines(string path)
        {
            if (!File.Exists(path))
            {
                return Option<string[]>.None();
            }

            return File.ReadAllLines(path);
        }

        public StreamWriter WriteFile(string path)
        {
            return new StreamWriter(new FileStream(path, FileMode.Create));
        }
    }
}