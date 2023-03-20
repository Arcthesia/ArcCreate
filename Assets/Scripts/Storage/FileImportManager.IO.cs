using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        public async UniTask ImportArchive(string path, bool isImportingDefaultPackage = false)
        {
            using (FileStream fs = File.OpenRead(path))
            {
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    Debug.Log("Importing package from " + path);
                    await ImportLevelArchive(zip, isImportingDefaultPackage);
                }
            }
        }

        public async UniTask ImportLevelArchive(ZipArchive archive, bool isImportingDefaultPackage = false)
        {
            ClearError();
            ClearSummary();

            if (!isImportingDefaultPackage)
            {
                ShowLoading(I18n.S("Storage.Loading.Archive"));
                await UniTask.NextFrame();
            }

            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                {
                    continue;
                }

                string path = Path.Combine(FileStatics.TempImportPath, entry.FullName);
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

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
                await ImportDirectory(new DirectoryInfo(FileStatics.TempImportPath), isImportingDefaultPackage);
            }
            catch (Exception e)
            {
                if (Directory.Exists(FileStatics.TempImportPath))
                {
                    Directory.Delete(FileStatics.TempImportPath, true);
                }

                DisplayError("Archive file", e);
                print(e.Message + " " + e.StackTrace);
            }
        }

        private async UniTask ImportDirectory(DirectoryInfo dir, bool isImportingDefaultPackage)
        {
            var pendingConflicts = new List<IStorageUnit>();
            var identifierToData = new Dictionary<string, IStorageUnit>();
            var toStore = new List<IStorageUnit>();
            var toDelete = new List<IStorageUnit>();

            if (!isImportingDefaultPackage)
            {
                ShowLoading(I18n.S("Storage.Loading.ValidatePackage"));
                await UniTask.NextFrame();
            }

            var (importingData, importingFileReferences) = ReadItemsFromDirectory(dir);
            foreach (var item in importingData)
            {
                item.IsDefaultAsset = isImportingDefaultPackage;
            }

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
            if (!isImportingDefaultPackage)
            {
                ShowLoading(I18n.S("Storage.Loading.CheckConflict"));
                await UniTask.NextFrame();
            }

            foreach (IStorageUnit data in importingData)
            {
                identifierToData.Add(data.Identifier, data);
                IStorageUnit conflict = data.GetConflictingIdentifier();
                if (conflict != null)
                {
                    pendingConflicts.Add(conflict);
                }
                else
                {
                    toStore.Add(data);
                }
            }

            // Await confirmation
            bool hasConflict = pendingConflicts.Count >= 0;
            if (hasConflict)
            {
                foreach (var conflict in pendingConflicts)
                {
                    IStorageUnit replaceWith = identifierToData[conflict.Identifier];
                    bool shouldReplace =
                        replaceWith.Version >= conflict.Version
                        || await PromptConflictToUser(replaceWith, conflict);
                    if (shouldReplace)
                    {
                        toDelete.Add(conflict);
                        toStore.Add(replaceWith);
                    }
                    else
                    {
                        toStore.Remove(replaceWith);
                    }
                }
            }

            // There should be no issues importing now
            foreach (IStorageUnit item in toDelete)
            {
                if (!isImportingDefaultPackage)
                {
                    ShowLoading(I18n.S("Storage.Loading.DeleteAsset", item.Identifier));
                    await UniTask.NextFrame();
                }

                item.Delete();
            }

            foreach (IStorageUnit item in toStore)
            {
                if (!isImportingDefaultPackage)
                {
                    ShowLoading(I18n.S("Storage.Loading.StoreAsset", item.Identifier));
                    await UniTask.NextFrame();
                }

                item.Insert();
                foreach (string rawVirtualPath in item.FileReferences)
                {
                    string virtualPath = string.Join("/", item.Type, item.Identifier, rawVirtualPath);
                    string realPath = Path.Combine(dir.FullName, importingFileReferences[string.Join("/", item.Identifier, rawVirtualPath)]);
                    FileStorage.ImportFile(realPath, virtualPath);
                }
            }

            if (!isImportingDefaultPackage)
            {
                ShowLoading(I18n.S("Storage.Loading.Finalizing"));
                await UniTask.NextFrame();
            }

            if (Directory.Exists(FileStatics.TempPath))
            {
                Directory.Delete(FileStatics.TempPath, true);
            }

            storageData.NotifyStorageChange();

            if (!isImportingDefaultPackage)
            {
                ShowSummary(toStore);
            }

            HideLoading();
        }

        /// <summary>
        /// Reads the directory and return all items along with their file references.
        /// </summary>
        /// <param name="parentDirectory">The directory to read from.</param>
        /// <returns>List of all importing assets and list of files for each asset.</returnx>
        private (List<IStorageUnit> importingData, Dictionary<string, string> fileReferences)
            ReadItemsFromDirectory(DirectoryInfo parentDirectory)
        {
            // Read import info
            FileInfo importYaml = parentDirectory.EnumerateFiles().First(file => file.Name == ImportInformation.FileName);
            string yaml = File.ReadAllText(importYaml.FullName);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            List<ImportInformation> imports = deserializer.Deserialize<List<ImportInformation>>(yaml);

            Dictionary<string, string> fileReferences = new Dictionary<string, string>();
            List<IStorageUnit> importingData = new List<IStorageUnit>();

            foreach (ImportInformation import in imports)
            {
                IStorageUnit storage = null;
                string settingsContent = File.ReadAllText(Path.Combine(parentDirectory.FullName, import.Directory, import.SettingsFile));

                try
                {
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

                    if (!storage.ValidateSelf(out string reason))
                    {
                        throw new Exception($"Invalid package, Importing was skipped.\n{reason}");
                    }
                }
                catch (Exception e)
                {
                    DisplayError($"Asset ({import.Type}, {import.Identifier})", e);
                    continue;
                }

                if (storage != null)
                {
                    storage.Identifier = import.Identifier;
                    storage.Version = import.Version;
                    storage.FileReferences = new List<string>();
                    storage.AddedDate = DateTime.UtcNow;

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
                            string relativeToRoot = file.FullName.Substring(root.FullName.Length).TrimStart('/', '\\');
                            if (relativeToRoot != import.SettingsFile)
                            {
                                storage.FileReferences.Add(relativeToRoot);
                                fileReferences.Add(string.Join("/", storage.Identifier, relativeToRoot), file.FullName);
                            }
                        }
                    }

                    importingData.Add(storage);
                }
            }

            return (importingData, fileReferences);
        }
    }
}