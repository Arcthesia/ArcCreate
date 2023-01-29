using System;
using System.Collections.Generic;
using System.IO;
using ArcCreate.ChartFormat;
using ArcCreate.Compose.Navigation;
using ArcCreate.Gameplay;
using ArcCreate.Utility;
using ArcCreate.Utility.Extension;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ArcCreate.Compose.Project
{
    [EditorScope("Project")]
    public class ProjectService : MonoBehaviour, IProjectService
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Button newProjectButton;
        [SerializeField] private Button openProjectButton;
        [SerializeField] private Button saveProjectButton;
        [SerializeField] private Button openFolderButton;
        [SerializeField] private CanvasGroup toggleInteractiveCanvas;
        [SerializeField] private GameObject noProjectLoadedHint;
        [SerializeField] private ChartPicker chartPicker;
        [SerializeField] private TMP_Text currentChartPath;
        [SerializeField] private NewProjectDialog newProjectDialog;
        [SerializeField] private NewChartDialog newChartDialog;
        [SerializeField] private List<Color> defaultDifficultyColors;
        [SerializeField] private List<string> defaultDifficultyNames;

        public event Action<ChartSettings> OnChartLoad;

        public ProjectSettings CurrentProject { get; private set; }

        public ChartSettings CurrentChart { get; private set; }

        public List<Color> DefaultDifficultyColors => defaultDifficultyColors;

        public void CreateNewProject(NewProjectInfo info)
        {
            string dir = Path.GetDirectoryName(info.ProjectFile.FullPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(info.AudioPath.FullPath))
            {
                throw new ComposeException(I18n.S("Compose.Exception.FileDoesNotExist", new Dictionary<string, object>
                {
                    { "Path", info.AudioPath.FullPath },
                }));
            }

            if (info.AudioPath.ShouldCopy)
            {
                File.Copy(info.AudioPath.OriginalPath, info.AudioPath.FullPath);
            }

            if (info.BackgroundPath?.ShouldCopy ?? false)
            {
                File.Copy(info.BackgroundPath.OriginalPath, info.BackgroundPath.FullPath);
            }

            if (info.JacketPath?.ShouldCopy ?? false)
            {
                File.Copy(info.JacketPath.OriginalPath, info.JacketPath.FullPath);
            }

            ChartSettings chart = new ChartSettings()
            {
                ChartPath = info.StartingChartPath,
                BaseBpm = info.BaseBPM,
                AudioPath = info.AudioPath.ShortenedPath,
                JacketPath = info.JacketPath?.ShortenedPath,
                BackgroundPath = info.BackgroundPath?.ShortenedPath,
            };

            ProjectSettings projectSettings = new ProjectSettings()
            {
                Path = info.ProjectFile.FullPath,
                LastOpenedChartPath = info.StartingChartPath,
                Charts = new List<ChartSettings>() { chart },
            };

            AutofillChart(chart);
            Serialize(projectSettings);
            OpenProject(projectSettings.Path);

            Debug.Log(
                I18n.S("Compose.Notify.Project.NewProject", new Dictionary<string, object>()
                {
                    { "Path", projectSettings.Path },
                }));
        }

        public void CreateNewChart(string chartFilePath)
        {
            ChartSettings newChart = CurrentChart.Clone();
            newChart.ChartPath = chartFilePath;
            AutofillChart(newChart);

            CurrentProject.Charts.Add(newChart);
            CurrentProject.Charts.Sort((a, b) => a.ChartPath.CompareTo(b.ChartPath));

            CurrentChart = newChart;
            CurrentProject.LastOpenedChartPath = newChart.ChartPath;

            chartPicker.SetOptions(CurrentProject.Charts, CurrentChart);
            currentChartPath.text = CurrentChart.ChartPath;
            OnChartLoad?.Invoke(CurrentChart);

            Debug.Log(
                I18n.S("Compose.Notify.Project.CreateChart", new Dictionary<string, object>()
                {
                    { "Path", chartFilePath },
                }));
        }

        public void OpenChart(ChartSettings chart)
        {
            CurrentChart = chart;
            CurrentProject.LastOpenedChartPath = CurrentChart.ChartPath;
            currentChartPath.text = CurrentChart.ChartPath;
            LoadChart(CurrentChart.ChartPath);
            OnChartLoad?.Invoke(CurrentChart);
        }

        public void RemoveChart(ChartSettings chart)
        {
            if (chart == CurrentChart)
            {
                return;
            }

            CurrentProject.Charts.Remove(chart);
            chartPicker.SetOptions(CurrentProject.Charts, CurrentChart);

            Debug.Log(
                I18n.S("Compose.Notify.Project.RemoveChart", new Dictionary<string, object>()
                {
                    { "Path", chart.ChartPath },
                }));
        }

        [EditorAction("New", false, "<c-n>")]
        private void OnNewProjectPressed()
        {
            newProjectDialog.Open();
        }

        [EditorAction("Open", false, "<c-o>")]
        private void OnOpenProjectPressed()
        {
            string path = Shell.OpenFileDialog(
                filterName: "ArcCreate Project",
                extension: new string[] { Values.ProjectExtensionWithoutDot },
                title: "Open ArcCreate Project",
                initPath: PlayerPrefs.GetString("LastProjectPath", ""));

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            PlayerPrefs.SetString("LastProjectPath", path);
            OpenProject(path);
        }

        [EditorAction("Save", false, "<c-s>")]
        [RequireGameplayLoaded]
        private void OnSaveProjectPressed()
        {
            if (CurrentProject == null)
            {
                return;
            }

            Serialize(CurrentProject);
        }

        private void OpenProjectFolder()
        {
            if (CurrentProject == null)
            {
                return;
            }

            Shell.OpenExplorer(Path.GetDirectoryName(CurrentProject.Path));
        }

        private void Awake()
        {
            newProjectButton.onClick.AddListener(OnNewProjectPressed);
            openProjectButton.onClick.AddListener(OnOpenProjectPressed);
            saveProjectButton.onClick.AddListener(OnSaveProjectPressed);
            openFolderButton.onClick.AddListener(OpenProjectFolder);
            toggleInteractiveCanvas.interactable = false;
            noProjectLoadedHint.SetActive(true);
        }

        private void OnDestroy()
        {
            newProjectButton.onClick.RemoveListener(OnNewProjectPressed);
            openProjectButton.onClick.RemoveListener(OnOpenProjectPressed);
            saveProjectButton.onClick.RemoveListener(OnSaveProjectPressed);
            openFolderButton.onClick.RemoveListener(OpenProjectFolder);
        }

        private void OpenProject(string path)
        {
            ProjectSettings project = Deserialize(path);
            project.Path = path;
            CurrentProject = project;

            if (project.Charts.Count == 0)
            {
                throw new ComposeException(I18n.S("Compose.Exception.NoChartIncluded"));
            }

            CurrentChart = project.Charts[0];
            foreach (ChartSettings chart in project.Charts)
            {
                if (chart.ChartPath == project.LastOpenedChartPath)
                {
                    CurrentChart = chart;
                }
            }

            chartPicker.SetOptions(CurrentProject.Charts, CurrentChart);

            toggleInteractiveCanvas.interactable = true;
            noProjectLoadedHint.SetActive(false);

            currentChartPath.text = CurrentChart.ChartPath;
            LoadChart(CurrentChart.ChartPath);
            OnChartLoad?.Invoke(CurrentChart);

            Debug.Log(
                I18n.S("Compose.Notify.Project.OpenProject", new Dictionary<string, object>()
                {
                    { "Path", path },
                }));
        }

        private void Serialize(ProjectSettings projectSettings)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            string yaml = serializer.Serialize(projectSettings);
            File.WriteAllText(projectSettings.Path, yaml);

            Debug.Log(
                I18n.S("Compose.Notify.Project.SaveProject", new Dictionary<string, object>()
                {
                    { "Path", projectSettings.Path },
                }));
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

        private void AutofillChart(ChartSettings chart)
        {
            chart.Title = Values.DefaultTitle;
            chart.Composer = Values.DefaultComposer;

            switch (chart.ChartPath.Split('.')[0])
            {
                case "0":
                    chart.DifficultyColor = defaultDifficultyColors[0].ConvertToHexCode();
                    chart.Difficulty = defaultDifficultyNames[0];
                    break;
                case "1":
                    chart.DifficultyColor = defaultDifficultyColors[1].ConvertToHexCode();
                    chart.Difficulty = defaultDifficultyNames[1];
                    break;
                case "2":
                    chart.DifficultyColor = defaultDifficultyColors[2].ConvertToHexCode();
                    chart.Difficulty = defaultDifficultyNames[2];
                    break;
                case "3":
                    chart.DifficultyColor = defaultDifficultyColors[3].ConvertToHexCode();
                    chart.Difficulty = defaultDifficultyNames[3];
                    break;
                default:
                    chart.DifficultyColor = defaultDifficultyColors[2].ConvertToHexCode();
                    chart.Difficulty = defaultDifficultyNames[2];
                    break;
            }
        }

        private void LoadChart(string chartPath)
        {
            string dir = Path.GetDirectoryName(CurrentProject.Path);
            string path = Path.Combine(dir, chartPath);
            ChartReader reader = ChartReaderFactory.GetReader(new PhysicalFileAccess(), path);

            // Maybe move this somewhere else later
            Values.EditingTimingGroup.Value = 0;
            reader.Parse();
            gameplayData.LoadChart(reader);

            Debug.Log(
                I18n.S("Compose.Notify.Project.OpenChart", new Dictionary<string, object>()
                {
                    { "Path", chartPath },
                }));
        }
    }
}