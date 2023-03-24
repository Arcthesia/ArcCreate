using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ArcCreate.Data;
using ArcCreate.Storage;
using ArcCreate.Utility;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ArcCreate.Compose.Project
{
    public class Exporter
    {
        private readonly ProjectSettings project;
        private readonly string publisher;
        private readonly string package;
        private readonly int version;

        public Exporter(ProjectSettings project, string publisher, string package, int version)
        {
            this.project = project;
            this.publisher = publisher;
            this.package = package;
            this.version = version;
        }

        public void Export(string outputPath)
        {
            string subdir = package;
            List<ImportInformation> info = new List<ImportInformation>()
            {
                new ImportInformation
                {
                    Directory = subdir,
                    Identifier = $"{publisher}.{package}",
                    SettingsFile = Path.GetFileName(project.Path),
                    Version = version,
                    Type = ImportInformation.LevelType,
                },
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            string infoYaml = serializer.Serialize(info);

            try
            {
                using (FileStream zipStream = new FileStream(outputPath, FileMode.Create))
                {
                    using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        ZipArchiveEntry importInfo = zip.CreateEntry(ImportInformation.FileName);
                        using (Stream importStream = importInfo.Open())
                        {
                            using (StreamWriter writer = new StreamWriter(importStream))
                            {
                                writer.Write(infoYaml);
                                writer.Flush();
                            }
                        }

                        var projectDir = new DirectoryInfo(Path.GetDirectoryName(project.Path));
                        WriteDirectoryToZip(projectDir, zip, subdir, new List<string>()
                        {
                            outputPath,
                            "remote.aff",
                            "remote.sc.json",
                        });
                    }
                }

                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.Export.Package.Complete", outputPath));
                Shell.OpenExplorer(Path.GetDirectoryName(outputPath));
            }
            catch (Exception e)
            {
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }

                Debug.LogError(e);
            }
        }

        private void WriteDirectoryToZip(DirectoryInfo projectDir, ZipArchive zip, string subdir, List<string> blockedPaths)
        {
            foreach (FileInfo file in projectDir.EnumerateFiles())
            {
                if (!blockedPaths.Contains(file.FullName) && !file.FullName.EndsWith(".arcpkg") && !file.FullName.EndsWith(".mp4"))
                {
                    WriteFileToZip(zip, file.FullName, Path.Combine(subdir, file.Name));
                }
            }

            foreach (DirectoryInfo dir in projectDir.EnumerateDirectories())
            {
                WriteDirectoryToZip(dir, zip, Path.Combine(subdir, dir.Name), blockedPaths);
            }
        }

        private void WriteFileToZip(ZipArchive zip, string sourcePath, string targetPath)
        {
            ZipArchiveEntry entry = zip.CreateEntry(targetPath);
            using (Stream entryStream = entry.Open())
            {
                using (FileStream fileStream = new FileStream(sourcePath, FileMode.Open))
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }
    }
}