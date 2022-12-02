using System.IO;

namespace ArcCreate.ChartFormat
{
    public interface IFileAccessWrapper
    {
        string ReadFile(string path);

        string[] ReadFileByLines(string path);

        StreamWriter WriteFile(string path);
    }
}