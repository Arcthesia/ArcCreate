using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArcCreate.ChartFormat;

namespace ArcCreate.Compose.Project
{
    internal class BackupHelper
    {
        private readonly string projectPath;

        public BackupHelper(string projectPath)
        {
            this.projectPath = projectPath;
        }

        public void Serialize(int offset, float density, List<(RawTimingGroup groups, IEnumerable<RawEvent> events)> chartData)
        {
            string fileName = Path.GetFileNameWithoutExtension(projectPath);
            string datetime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string backupRoot = Path.Combine(Path.GetDirectoryName(projectPath), $".{fileName}_backup");

            ClearBackupRoot(backupRoot);
            string dir = Path.Combine(backupRoot, datetime);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            new ChartSerializer(new PhysicalFileAccess(), dir).Write(offset, density, chartData);
        }

        public void ClearBackupRoot(string dir)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(dir);
            if (!directoryInfo.Exists)
            {
                return;
            }

            List<DirectoryInfo> subdirs = directoryInfo.GetDirectories().ToList();
            subdirs.Sort((a, b) => a.Name.CompareTo(b.Name));

            int backupCount = Settings.BackupCount.Value;
            for (int i = 0; i < subdirs.Count - backupCount + 1; i++)
            {
                subdirs[i].Delete(true);
            }
        }
    }
}