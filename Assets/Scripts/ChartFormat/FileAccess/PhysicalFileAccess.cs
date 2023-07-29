using System;
using System.IO;

namespace ArcCreate.ChartFormat
{
    public class PhysicalFileAccess : IFileAccessWrapper
    {
        public string GetFileUri(string path)
        {
            return "file:///" + Uri.EscapeUriString(path.Replace("\\", "/"));
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