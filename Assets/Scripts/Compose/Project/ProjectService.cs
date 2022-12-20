using System;
using System.Collections.Generic;
using System.IO;
using ArcCreate.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ArcCreate.Compose.Project
{
    public class ProjectService : MonoBehaviour, IProjectService
    {
        [SerializeField] private Button newProjectButton;
        [SerializeField] private Button openProjectButton;
        [SerializeField] private Button saveProjectButton;
        [SerializeField] private TMP_Dropdown chartSelectDropdown;
        [SerializeField] private NewProjectDialog newProjectDialog;
        [SerializeField] private NewChartDialog newChartDialog;

        public event Action<ChartSettings> OnChartLoad;

        public ProjectSettings CurrentProject { get; private set; }

        public ChartSettings CurrentChart { get; private set; }

        public void CreateNewProject(ProjectSettings projectSettings)
        {
            string dir = Path.GetDirectoryName(projectSettings.Path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            foreach (ChartSettings chart in projectSettings.Charts)
            {
                if (!File.Exists(chart.AudioPath))
                {
                    throw new ComposeException(I18n.S("Compose.Exception.FileDoesNotExist", new Dictionary<string, object>
                    {
                        { "Path", chart.AudioPath },
                    }));
                }

                chart.AudioPath = CopyIfNotLocal(chart.AudioPath, dir);
                chart.BackgroundPath = CopyIfNotLocal(chart.BackgroundPath, dir);
                chart.JacketPath = CopyIfNotLocal(chart.JacketPath, dir);
            }

            Serialize(projectSettings);
            OpenProject(projectSettings.Path);
        }

        public void CreateNewChart(string chartFilePath)
        {
            ChartSettings newChart = CurrentChart.Clone();
            newChart.ChartPath = chartFilePath;

            CurrentProject.Charts.Add(newChart);
            CurrentProject.Charts.Sort((a, b) => a.ChartPath.CompareTo(b.ChartPath));

            CurrentChart = newChart;
            CurrentProject.LastOpenedChartPath = newChart.ChartPath;

            PopulateSelectDropdown(CurrentProject);
            OnChartLoad?.Invoke(CurrentChart);
        }

        private void OnNewProjectPressed()
        {
            newProjectDialog.Open();
        }

        private void OnOpenProjectPressed()
        {
            string path = Shell.OpenFileDialog(
                filterName: "ArcCreate Project",
                extension: new string[] { Strings.ProjectExtensionWithoutDot },
                title: "Open ArcCreate Project",
                initPath: PlayerPrefs.GetString("LastProjectPath", ""));

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            PlayerPrefs.SetString("LastProjectPath", path);
            OpenProject(path);
        }

        private void OnSaveProjectPressed()
        {
            Serialize(CurrentProject);
        }

        private void OnChartSelect(int dropdownIndex)
        {
            // Last option in list
            if (dropdownIndex == CurrentProject.Charts.Count)
            {
                for (int i = 0; i < CurrentProject.Charts.Count; i++)
                {
                    ChartSettings chart = CurrentProject.Charts[i];

                    if (chart.ChartPath == CurrentChart.ChartPath)
                    {
                        chartSelectDropdown.SetValueWithoutNotify(i);
                        break;
                    }
                }

                newChartDialog.Open();
                return;
            }

            CurrentChart = CurrentProject.Charts[dropdownIndex];
            CurrentProject.LastOpenedChartPath = CurrentChart.ChartPath;
            OnChartLoad?.Invoke(CurrentChart);
        }

        private void Awake()
        {
            newProjectButton.onClick.AddListener(OnNewProjectPressed);
            openProjectButton.onClick.AddListener(OnOpenProjectPressed);
            saveProjectButton.onClick.AddListener(OnSaveProjectPressed);
            chartSelectDropdown.onValueChanged.AddListener(OnChartSelect);
        }

        private void OnDestroy()
        {
            newProjectButton.onClick.RemoveListener(OnNewProjectPressed);
            openProjectButton.onClick.RemoveListener(OnOpenProjectPressed);
            saveProjectButton.onClick.RemoveListener(OnSaveProjectPressed);
            chartSelectDropdown.onValueChanged.RemoveListener(OnChartSelect);
        }

        private string CopyIfNotLocal(string path, string dir)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            path = Path.GetFullPath(path);
            dir = Path.GetFullPath(dir);

            if (path.StartsWith(dir))
            {
                return path.Substring(dir.Length);
            }
            else
            {
                string fileName = Path.GetFileName(path);
                File.Copy(path, Path.Combine(dir, fileName), true);
                return fileName;
            }
        }

        private void OpenProject(string path)
        {
            ProjectSettings project = Deserialize(path);
            project.Path = path;
            CurrentProject = project;
            PopulateSelectDropdown(project);

            OnChartLoad?.Invoke(CurrentChart);
        }

        private void Serialize(ProjectSettings projectSettings)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            string yaml = serializer.Serialize(projectSettings);
            File.WriteAllText(projectSettings.Path, yaml);
        }

        private ProjectSettings Deserialize(string path)
        {
            try
            {
                string content = File.ReadAllText(path);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(new CamelCaseNamingConvention())
                    .Build();
                return deserializer.Deserialize<ProjectSettings>(content);
            }
            catch (Exception e)
            {
                throw new ComposeException(I18n.S("Compose.Exception.LoadProject", new Dictionary<string, object>
                {
                    { "Path", path },
                    { "Error", e.Message },
                }));
            }
        }

        private void PopulateSelectDropdown(ProjectSettings project)
        {
            var dropdownOptions = new List<TMP_Dropdown.OptionData>();
            int currentChartIndex = 0;
            for (int i = 0; i < project.Charts.Count; i++)
            {
                ChartSettings chart = project.Charts[i];
                dropdownOptions.Add(new TMP_Dropdown.OptionData(chart.ChartPath));

                if (chart.ChartPath == project.LastOpenedChartPath)
                {
                    CurrentChart = chart;
                    currentChartIndex = i;
                }
            }

            dropdownOptions.Add(new TMP_Dropdown.OptionData(I18n.S("Compose.UI.Project.Label.NewChart")));
            chartSelectDropdown.options = dropdownOptions;
            chartSelectDropdown.SetValueWithoutNotify(currentChartIndex);
        }
    }
}