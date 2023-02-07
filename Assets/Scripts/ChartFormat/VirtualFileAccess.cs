using System.IO;

namespace ArcCreate.ChartFormat
{
    public class VirtualFileAccess : IFileAccessWrapper
    {
        private readonly string[] data;

        public VirtualFileAccess(string data)
        {
            this.data = data.Split('\n');
        }

        public string[] ReadFileByLines(string path) => data;

        public StreamWriter WriteFile(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}