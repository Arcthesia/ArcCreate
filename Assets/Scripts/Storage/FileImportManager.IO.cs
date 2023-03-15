using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ArcCreate.Data;
using ArcCreate.Storage.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ArcCreate.Storage
{
    /// <summary>
    /// Partial class for handling file IO while importing.
    /// </summary>
    public partial class FileImportManager : MonoBehaviour
    {
        private async UniTask ImportArchive(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            {
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    Debug.Log("Importing package from " + path);
                    await ImportLevelArchive(zip);
                }
            }
        }

        private async UniTask ImportLevelArchive(ZipArchive archive)
        {
            foreach (var entry in archive.Entries)
            {
                string path = Path.Combine(FileStatics.TempImportPath, entry.FullName);
                using (FileStream fs = File.OpenWrite(path))
                {
                    using (Stream zs = entry.Open())
                    {
                        zs.CopyTo(fs);
                    }
                }
            }

            try
            {
                await ImportDirectory(new DirectoryInfo(FileStatics.TempImportPath));
            }
            catch (Exception e)
            {
                Directory.Delete(FileStatics.TempImportPath, true);
                throw e;
            }
        }

        private async UniTask ImportDirectory(DirectoryInfo dir)
        {
            var pendingConflicts = new List<IStorageUnit>();
            var identifierToData = new Dictionary<string, IStorageUnit>();
            var toStore = new List<IStorageUnit>();
            var toReplace = new List<(IStorageUnit, IStorageUnit)>();

            var (importingData, importingFileReferences) = ReadItemsFromDirectory(dir);

            // Detect duplicate identifier within the same importing package file
            HashSet<string> ids = new HashSet<string>();
            foreach (IStorageUnit item in importingData)
            {
                string id = item.Identifier;
                if (ids.Contains(id))
                {
                    throw new FileLoadException($"Invalid package: Duplicate identifier deteced within the same package: {id}");
                }

                ids.Add(id);
            }

            // Detect duplicate identifier with existing package (i.e import conflict)
            foreach (IStorageUnit data in importingData)
            {
                IStorageUnit conflict = data.GetConflictingIdentifier();
                if (conflict != null)
                {
                    pendingConflicts.Add(conflict);
                }

                identifierToData.Add(data.Identifier, data);
                toStore.Add(data);
            }

            // Await confirmation
            bool hasConflict = pendingConflicts.Count == 0;
            if (hasConflict)
            {
                foreach (var conflict in pendingConflicts)
                {
                    IStorageUnit replaceWith = identifierToData[conflict.Identifier];
                    bool shouldReplace =
                        replaceWith.CreatedAt > conflict.CreatedAt
                        || await PromptConflictToUser(replaceWith, conflict);
                    if (shouldReplace)
                    {
                        toReplace.Add((conflict, replaceWith));
                        toStore.Remove(replaceWith);
                    }
                }
            }

            // There should be no issues importing now
            foreach ((IStorageUnit replaced, IStorageUnit replaceWith) in toReplace)
            {
                replaced.Update(replaceWith);
            }

            foreach (IStorageUnit item in toStore)
            {
                item.Insert();
            }

            foreach (IStorageUnit data in importingData)
            {
                foreach (string rawVirtualPath in data.FileReferences)
                {
                    string virtualPath = Path.Combine(data.Type, data.Identifier, rawVirtualPath);
                    string realPath = Path.Combine(dir.FullName, importingFileReferences[(data, rawVirtualPath)]);
                    FileStorage.ImportFile(realPath, virtualPath);
                }
            }

            Directory.Delete(FileStatics.TempPath, true);
        }

        /// <summary>
        /// Reads the directory and add detected items to importingLevel and importingData.
        /// </summary>
        /// <param name="parentDirectory">The directory to read from.</param>
        /// <returns>List of all importing assets and list of files for each asset.</returnx>
        private (List<IStorageUnit> importingData, Dictionary<(IStorageUnit, string), string> fileReferences)
            ReadItemsFromDirectory(DirectoryInfo parentDirectory)
        {
            // Read import info
            FileInfo importYaml = parentDirectory.EnumerateFiles().First(file => file.Name == ImportInformation.FileName);
            string yaml = File.ReadAllText(importYaml.FullName);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            List<ImportInformation> imports = deserializer.Deserialize<List<ImportInformation>>(yaml);

            Dictionary<(IStorageUnit, string), string> fileReferences = new Dictionary<(IStorageUnit, string), string>();
            List<IStorageUnit> importingData = new List<IStorageUnit>();

            foreach (ImportInformation import in imports)
            {
                IStorageUnit storage = null;
                string settingsContent = File.ReadAllText(Path.Combine(parentDirectory.FullName, import.Directory, import.SettingsFile));
                switch (import.Type)
                {
                    case ImportInformation.LevelType:
                        storage = new LevelStorage()
                        {
                            Settings = deserializer.Deserialize<ProjectSettings>(settingsContent),
                        };
                        break;
                    case ImportInformation.PackType:
                        var packInfo = deserializer.Deserialize<PackStorage.PackImportInformation>(settingsContent);
                        storage = new PackStorage()
                        {
                            PackName = packInfo.PackName,
                            ImagePath = packInfo.ImagePath,
                            LevelIdentifiers = packInfo.LevelIdentifiers,
                        };
                        break;
                }

                if (storage != null)
                {
                    storage.CreatedAt = import.CreatedAt;
                    storage.FileReferences = new List<string>();

                    Stack<DirectoryInfo> directoriesToImport = new Stack<DirectoryInfo>();
                    DirectoryInfo root = new DirectoryInfo(Path.Combine(parentDirectory.FullName, import.Directory));
                    directoriesToImport.Push(root);

                    while (directoriesToImport.Count > 0)
                    {
                        DirectoryInfo dir = directoriesToImport.Pop();
                        foreach (var subdir in dir.EnumerateDirectories())
                        {
                            directoriesToImport.Push(subdir);
                        }

                        foreach (var file in dir.EnumerateFiles())
                        {
                            string relativeToRoot = file.FullName.Substring(root.FullName.Length);
                            if (relativeToRoot != import.SettingsFile)
                            {
                                storage.FileReferences.Add(relativeToRoot);
                                fileReferences.Add((storage, relativeToRoot), file.FullName);
                            }
                        }
                    }

                    storage.Insert();
                    importingData.Add(storage);
                }
            }

            return (importingData, fileReferences);
        }
    }
}