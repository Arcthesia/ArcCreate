using System.IO;

namespace ArcCreate.ChartFormat
{
    public interface IFileAccessWrapper
    {
        string[] ReadFileByLines(string path);

        StreamWriter WriteFile(string path);
    }
}