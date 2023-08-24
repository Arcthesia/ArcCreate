using System;
using System.IO;

namespace ArcCreate.ChartFormat
{
    public interface IFileAccessWrapper
    {
        Uri GetFileUri(string path);

        Option<string[]> ReadFileByLines(string path);

        StreamWriter WriteFile(string path);
    }
}