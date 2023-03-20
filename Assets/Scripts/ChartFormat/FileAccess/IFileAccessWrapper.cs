using System.IO;

namespace ArcCreate.ChartFormat
{
    public interface IFileAccessWrapper
    {
        string GetPath(string path);

        string GetFileUri(string path);

        string[] ReadFileByLines(string path);

        StreamWriter WriteFile(string path);
    }
}