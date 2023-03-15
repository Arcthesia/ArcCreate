using System;
using System.Collections.Generic;
using System.IO;
using ArcCreate.ChartFormat;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Popups;
using ArcCreate.Data;
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
            string projPath = info.ProjectFile.FullPath;
            if (!projPath.EndsWith(".arcproj"))
            {
                projPath = projPath + ".arcproj";
            }

            string dir = Path.GetDirectoryName(projPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (info.AudioPath.ShouldCopy)
            {
                if (!File.Exists(info.AudioPath.OriginalPath))
                {
                    throw new ComposeException(I18n.S("Compose.Exception.FileDoesNotExist", new Dictionary<string, object>
                    {
                        { "Path", info.AudioPath.OriginalPath },
                    }));
                }

                info.AudioPath.RenameUntilNoOverwrite();
                File.Copy(info.AudioPath.OriginalPath, info.AudioPath.FullPath);
            }
            else
            {
                if (!File.Exists(info.AudioPath.FullPath))
                {
                    throw new ComposeException(I18n.S("Compose.Exception.FileDoesNotExist", new Dictionary<string, object>
                    {
                        { "Path", info.AudioPath.FullPath },
                    }));
                }
            }

            if (info.BackgroundPath?.ShouldCopy ?? false)
            {
                info.BackgroundPath.RenameUntilNoOverwrite();
                File.Copy(info.BackgroundPath.OriginalPath, info.BackgroundPath.FullPath);
            }

            if (info.JacketPath?.ShouldCopy ?? false)
            {
                info.JacketPath.RenameUntilNoOverwrite();
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
                Path = projPath,
                LastOpenedChartPath = info.StartingChartPath,
                Charts = new List<ChartSettings>() { chart },
            };

            AutofillChart(chart);
            SerializeProject(projectSettings);
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

            LoadChart(CurrentChart);
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
            LoadChart(CurrentChart);
            Services.Gameplay.Audio.AudioTiming = chart.LastWorkingTiming;
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
        public void StartCreatingNewProject()
        {
            OpenUnsavedChangesDialog(newProjectDialog.Open);
        }

        [EditorAction("Open", false, "<c-o>")]
        public void StartOpeningProject()
        {
            OpenUnsavedChangesDialog(OnOpenConfirmed);
        }

        [EditorAction("Save", false, "<c-s>")]
        [RequireGameplayLoaded]
        public void SaveProject()
        {
            if (CurrentProject == null)
            {
                return;
            }

            CurrentChart.LastWorkingTiming = Services.Gameplay.Audio.AudioTiming;
            SerializeChart(CurrentProject);
            SerializeProject(CurrentProject);
        }

        private void OnOpenConfirmed()
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
            newProjectButton.onClick.AddListener(StartCreatingNewProject);
            openProjectButton.onClick.AddListener(StartOpeningProject);
            saveProjectButton.onClick.AddListener(SaveProject);
            openFolderButton.onClick.AddListener(OpenProjectFolder);
            toggleInteractiveCanvas.interactable = false;
            noProjectLoadedHint.SetActive(true);
        }

        private void OnDestroy()
        {
            newProjectButton.onClick.RemoveListener(StartCreatingNewProject);
            openProjectButton.onClick.RemoveListener(StartOpeningProject);
            saveProjectButton.onClick.RemoveListener(SaveProject);
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
            LoadChart(CurrentChart);

            Debug.Log(
                I18n.S("Compose.Notify.Project.OpenProject", new Dictionary<string, object>()
                {
                    { "Path", path },
                }));
        }

        private void SerializeProject(ProjectSettings projectSettings)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            string yaml = serializer.Serialize(projectSettings);
            File.WriteAllText(projectSettings.Path, yaml);

            Values.ProjectModified = false;

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
            chart.Title = string.IsNullOrEmpty(chart.Title) ? Values.DefaultTitle : chart.Title;
            chart.Composer = string.IsNullOrEmpty(chart.Composer) ? Values.DefaultComposer : chart.Composer;
            chart.SyncBaseBpm = true;

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

        private void LoadChart(ChartSettings chart)
        {
            string dir = Path.GetDirectoryName(CurrentProject.Path);
            string path = Path.Combine(dir, chart.ChartPath);

            if (!File.Exists(path))
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                var writer = ChartFileWriterFactory.GetWriterFromFilename(path);
                using (FileStream fileStream = File.OpenWrite(path))
                {
                    writer.Write(
                        new StreamWriter(fileStream),
                        0,
                        1,
                        new List<(RawTimingGroup, IEnumerable<RawEvent>)>()
                        {
                            (new RawTimingGroup(), new List<RawEvent>()
                                {
                                    new RawTiming()
                                    {
                                        Timing = 0,
                                        TimingGroup = 0,
                                        Bpm = chart.BaseBpm,
                                        Divisor = 4,
                                    },
                                }),
                        });
                }
            }

            ChartReader reader = ChartReaderFactory.GetReader(new PhysicalFileAccess(), path);

            reader.Parse();
            gameplayData.LoadChart(reader, "file:///" + Path.GetDirectoryName(path));
            OnChartLoad?.Invoke(chart);
            Values.ProjectModified = false;

            Debug.Log(
                I18n.S("Compose.Notify.Project.OpenChart", new Dictionary<string, object>()
                {
                    { "Path", chart.ChartPath },
                }));
        }

        private void SerializeChart(ProjectSettings projectSettings)
        {
            string dir = Path.Combine(Path.GetDirectoryName(projectSettings.Path), Path.GetDirectoryName(CurrentChart.ChartPath));
            var chartData = new RawEventsBuilder().GetEvents();
            new ChartSerializer(new PhysicalFileAccess(), dir).Write(
                gameplayData.AudioOffset.Value,
                gameplayData.TimingPointDensityFactor.Value,
                chartData);

            string scJson = Services.Gameplay.Scenecontrol.Export();
            if (scJson != "[]")
            {
                string scPath = Path.Combine(dir, Path.GetFileNameWithoutExtension(CurrentChart.ChartPath) + ".sc.json");
                File.WriteAllText(scPath, scJson);
            }
        }

        private void OpenUnsavedChangesDialog(Action onConfirm)
        {
            if (Values.ProjectModified == false)
            {
                onConfirm.Invoke();
                return;
            }

            Services.Popups.CreateTextDialog(
                title: I18n.S("Compose.Dialog.UnsavedChanges.Title"),
                content: I18n.S("Compose.Dialog.UnsavedChanges.Content"),
                buttonSettings: new ButtonSetting[]
                {
                    new ButtonSetting
                    {
                        Text = I18n.S("Compose.Dialog.UnsavedChanges.Yes"),
                        Callback = () =>
                        {
                            Services.Project.SaveProject();
                            onConfirm.Invoke();
                            Values.ProjectModified = false;
                        },
                        ButtonColor = ButtonColor.Highlight,
                    },
                    new ButtonSetting
                    {
                        Text = I18n.S("Compose.Dialog.UnsavedChanges.No"),
                        Callback = () =>
                        {
                            onConfirm.Invoke();
                            Values.ProjectModified = false;
                        },
                        ButtonColor = ButtonColor.Danger,
                    },
                    new ButtonSetting
                    {
                        Text = I18n.S("Compose.Dialog.UnsavedChanges.Cancel"),
                        Callback = null,
                        ButtonColor = ButtonColor.Default,
                    },
                });
        }
    }
}