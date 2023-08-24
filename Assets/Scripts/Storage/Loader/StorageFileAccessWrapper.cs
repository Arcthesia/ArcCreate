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

        public Uri GetFileUri(string path)
        {
            Option<string> realPath = level.GetRealPath(path);
            if (!realPath.HasValue)
            {
                return null;
            }

            return new Uri(realPath.Value);
        }

        public Option<string[]> ReadFileByLines(string path)
        {
            Option<string> realPath = level.GetRealPath(path);
            if (!realPath.HasValue)
            {
                return Option<string[]>.None();
            }

            return File.ReadAllLines(realPath.Value);
        }

        public StreamWriter WriteFile(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}