using System;
using System.IO;
using ArcCreate.ChartFormat;
using ArcCreate.Storage.Data;

namespace ArcCreate.Storage
{
    public class StorageFileAccessWrapper : IFileAccessWrapper
    {
        private readonly LevelStorage level;

        public StorageFileAccessWrapper(LevelStorage level)
        {
            this.level = level;
        }

        public string GetFileUri(string path)
        {
            return "file:///" + Uri.EscapeUriString(level.GetRealPath(path).Replace("\\", "/"));
        }

        public string GetPath(string path)
        {
            return level.GetRealPath(path);
        }

        public string[] ReadFileByLines(string path)
        {
            return File.ReadAllLines(level.GetRealPath(path));
        }

        public StreamWriter WriteFile(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}