using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ArcCreate.ChartFormat;
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

            EditorProjectSettings editorProjectSettings = project.EditorSettings;
            project.EditorSettings = null;
            string projectYaml = serializer.Serialize(project);
            project.EditorSettings = editorProjectSettings;

            try
            {
                using (FileStream zipStream = new FileStream(outputPath, FileMode.Create))
                {
                    using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        ZipArchiveEntry importInfoZipEntry = zip.CreateEntry(ImportInformation.FileName);
                        using (Stream importInfoStream = importInfoZipEntry.Open())
                        {
                            using (StreamWriter writer = new StreamWriter(importInfoStream))
                            {
                                writer.Write(infoYaml);
                                writer.Flush();
                            }
                        }

                        string projectDir = Path.GetDirectoryName(project.Path);
                        HashSet<string> referencedFiles = new HashSet<string>();
                        foreach (var chart in project.Charts)
                        {
                            foreach (var file in chart.GetReferencedFiles())
                            {
                                referencedFiles.Add(file);
                            }

                            string chartPath = Path.Combine(projectDir, chart.ChartPath);
                            ChartReader reader = ChartReaderFactory.GetReader(new PhysicalFileAccess(), chartPath);
                            reader.Parse();
                            foreach (var file in reader.GetReferencedFiles())
                            {
                                referencedFiles.Add(file);
                            }

                            string scJsonPath = Path.ChangeExtension(chart.ChartPath, ".sc.json");
                            string scJsonPathFull = Path.Combine(projectDir, scJsonPath);
                            if (File.Exists(scJsonPathFull))
                            {
                                WriteFileToZip(zip, subdir, scJsonPathFull, scJsonPath);
                            }
                        }

                        AddAllFilesInDirectory(projectDir, "Scenecontrol", referencedFiles);

                        foreach (var file in referencedFiles)
                        {
                            if (file == null)
                            {
                                continue;
                            }

                            string sourcePath = Path.Combine(projectDir, file);
                            if (File.Exists(sourcePath))
                            {
                                WriteFileToZip(zip, subdir, Path.Combine(projectDir, file), file);
                            }
                        }

                        string projectZipPath = Path.Combine(subdir, Path.GetFileName(project.Path)).Replace("\\", "/");
                        ZipArchiveEntry projectZipEntry = zip.CreateEntry(projectZipPath);
                        using (Stream projectStream = projectZipEntry.Open())
                        {
                            using (StreamWriter writer = new StreamWriter(projectStream))
                            {
                                writer.Write(projectYaml);
                                writer.Flush();
                            }
                        }
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

        private void AddAllFilesInDirectory(string projectDir, string includeDir, HashSet<string> referencedFiles)
        {
            projectDir = new DirectoryInfo(projectDir).FullName;
            Stack<DirectoryInfo> stack = new Stack<DirectoryInfo>();
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(projectDir, includeDir));
            if (!dir.Exists)
            {
                return;
            }

            stack.Push(dir);
            while (stack.Count > 0)
            {
                DirectoryInfo d = stack.Pop();
                foreach (var subdir in d.EnumerateDirectories())
                {
                    stack.Push(subdir);
                }

                foreach (var file in d.EnumerateFiles())
                {
                    string relativeToProjectDir = file.FullName.Substring(projectDir.Length + 1, file.FullName.Length - projectDir.Length - 1);
                    referencedFiles.Add(relativeToProjectDir);
                }
            }
        }

        private void WriteFileToZip(ZipArchive zip, string subdir, string sourcePath, string targetPath)
        {
            targetPath = Path.Combine(subdir, targetPath).Replace("\\", "/");
            ZipArchiveEntry entry = zip.CreateEntry(targetPath);
            using (Stream entryStream = entry.Open())
            {
                using (FileStream fileStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }
    }
}