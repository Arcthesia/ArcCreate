using System;
using System.IO;

namespace ArcCreate.ChartFormat
{
    public class VirtualFileAccess : IFileAccessWrapper
    {
        private readonly StreamWriter streamWriter;
        private readonly string[] data;

        public VirtualFileAccess(string data)
        {
            this.data = data.Split('\n');
        }

        public VirtualFileAccess(Stream stream)
        {
            streamWriter = new StreamWriter(stream);
        }

        public string GetFileUri(string path)
        {
            return "file:///" + Uri.EscapeUriString(path.Replace("\\", "/"));
        }

        public string GetPath(string path) => path;

        public string[] ReadFileByLines(string path) => data;

        public StreamWriter WriteFile(string path) => streamWriter;
    }
}