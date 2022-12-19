using System;
using System.Collections.Generic;
using System.IO;
using ArcCreate.Utility;
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
        [SerializeField] private Dropdown chartSelectDropdown;
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

                CopyIfNotLocal(chart.AudioPath, dir);
                CopyIfNotLocal(chart.BackgroundPath, dir);
                CopyIfNotLocal(chart.JacketPath, dir);
            }

            Serialize(projectSettings);
            OpenProject(projectSettings.Path);
        }

        public void CreateNewChart(string chartFilePath)
        {
            ChartSettings newChart = CurrentChart.Clone();
            newChart.ChartPath = chartFilePath;
            CurrentProject.Charts.Add(newChart);
        }

        private void OnNewProjectPressed()
        {
            newProjectDialog.Open();
        }

        private void OnOpenProjectPressed()
        {
            string path = Shell.OpenFileDialog(
                filterName: "ArcCreate Project",
                extension: new string[] { Strings.ProjectExtension },
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
                newChartDialog.Open();
                return;
            }

            CurrentChart = CurrentProject.Charts[dropdownIndex];
            OnChartLoad.Invoke(CurrentChart);
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

        private void CopyIfNotLocal(string path, string dir)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            path = Path.GetFullPath(path);
            dir = Path.GetFullPath(dir);

            if (Path.GetDirectoryName(path) != dir)
            {
                string fileName = Path.GetFileName(path);

                File.Copy(path, Path.Combine(dir, fileName));
            }
        }

        private void OpenProject(string path)
        {
            ProjectSettings project = Deserialize(path);
            CurrentProject = project;

            var dropdownOptions = new List<Dropdown.OptionData>();
            foreach (var chart in project.Charts)
            {
                dropdownOptions.Add(new Dropdown.OptionData(chart.ChartPath));

                if (chart.ChartPath == project.LastOpenedChartPath)
                {
                    CurrentChart = chart;
                }
            }

            dropdownOptions.Add(new Dropdown.OptionData(I18n.S("Compose.UI.Project.Label.NewChart")));

            OnChartLoad.Invoke(CurrentChart);
        }

        private void Serialize(ProjectSettings projectSettings)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            string yaml = serializer.Serialize(projectSettings);
            File.WriteAllText(yaml, projectSettings.Path);
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
    }
}