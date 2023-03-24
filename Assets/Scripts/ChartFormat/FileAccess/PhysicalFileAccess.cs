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

        public string GetPath(string path) => path;

        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public string[] ReadFileByLines(string path)
        {
            return File.ReadAllLines(path);
        }

        public StreamWriter WriteFile(string path)
        {
            return new StreamWriter(new FileStream(path, FileMode.Create));
        }
    }
}