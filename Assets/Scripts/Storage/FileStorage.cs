using System.Collections.Generic;
using System.IO;
using ArcCreate.Storage.Data;
using UltraLiteDB;

namespace ArcCreate.Storage
{
    public class FileStorage
    {
        public static string StoragePathLegacy => FileStatics.FileStoragePathLegacy;

        public static string StoragePath => FileStatics.FileStoragePath;

        public static UltraLiteCollection<FileReference> Collection => Database.Current.GetCollection<FileReference>();

        public static void MigrateToNewStorage()
        {
            if (!Directory.Exists(StoragePathLegacy))
            {
                return;
            }

            IEnumerable<FileReference> allFileEntries = Collection.Find(Query.All());
            foreach (var file in allFileEntries)
            {
                string realLegacyPath = Path.Combine(StoragePathLegacy, file.RealPath);
                if (!File.Exists(realLegacyPath))
                {
                    realLegacyPath = Path.Combine(StoragePathLegacy, AddDirectoryPrefix(file.RealPath));
                }

                if (!File.Exists(realLegacyPath))
                {
                    continue;
                }

                string targetPath = Path.Combine(StoragePath, file.VirtualPath);
                if (!Directory.Exists(Path.GetDirectoryName(targetPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                }

                File.Copy(realLegacyPath, targetPath);
            }

            Collection.Delete(Query.All());
            Directory.Delete(StoragePathLegacy, true);
        }

        private static string AddDirectoryPrefix(string path)
        {
            return Path.Combine(path.Substring(0, 1), path.Substring(1, 1), path);
        }
    }
}