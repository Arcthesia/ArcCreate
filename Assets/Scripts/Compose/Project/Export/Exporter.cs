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
        private readonly DateTime builtAt;

        public Exporter(ProjectSettings project, string publisher, string package, DateTime builtAt)
        {
            this.project = project;
            this.publisher = publisher;
            this.package = package;
            this.builtAt = builtAt;
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
                    CreatedAt = builtAt,
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
                        WriteDirectoryToZip(projectDir, zip, subdir, outputPath);
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

        private void WriteDirectoryToZip(DirectoryInfo projectDir, ZipArchive zip, string subdir, string blockPath)
        {
            foreach (FileInfo file in projectDir.EnumerateFiles())
            {
                if (file.FullName != blockPath)
                {
                    WriteFileToZip(zip, file.FullName, Path.Combine(subdir, file.Name));
                }
            }

            foreach (DirectoryInfo dir in projectDir.EnumerateDirectories())
            {
                WriteDirectoryToZip(dir, zip, Path.Combine(subdir, dir.Name), blockPath);
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