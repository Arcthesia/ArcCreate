using System;
using System.IO;
using ArcCreate.ChartFormat;

namespace ArcCreate.Compose.Project
{
    public class RawEditorFileAccess : IFileAccessWrapper
    {
        private readonly string chartPath;
        private readonly PhysicalFileAccess physical = new PhysicalFileAccess();
        private readonly VirtualFileAccess virt;

        public RawEditorFileAccess(string chartData, string chartPath)
        {
            this.chartPath = chartPath;
            virt = new VirtualFileAccess(chartData);
        }

        public Uri GetFileUri(string path)
        {
            return new Uri(path);
        }

        public Option<string[]> ReadFileByLines(string path)
        {
            if (path == chartPath)
            {
                return virt.ReadFileByLines(path);
            }

            return physical.ReadFileByLines(path);
        }

        public StreamWriter WriteFile(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}