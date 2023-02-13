using System.IO;
using ArcCreate.Compose.Components;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class NewProjectDialog : Dialog
    {
        [SerializeField] private FileSelectField projectFileField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_InputField startingChartFileField;
        [SerializeField] private TMP_Dropdown startingChartFileExtension;
        [SerializeField] private FileSelectField audioFileField;
        [SerializeField] private TMP_InputField baseBPMField;
        [SerializeField] private ImageFileSelectField jacketArtField;
        [SerializeField] private ImageFileSelectField backgroundField;

        private string currentFolder;

        private string StartingChartFile
            => startingChartFileField.text + startingChartFileExtension.options[startingChartFileExtension.value].text;

        private void OnConfirm()
        {
            if (audioFileField.CurrentPath == null
             || projectFileField.CurrentPath == null
             || startingChartFileField.text == null)
            {
                return;
            }

            if (Evaluator.TryFloat(baseBPMField.text, out float bpm))
            {
                string dir = Path.GetDirectoryName(projectFileField.CurrentPath.FullPath);
                Services.Project.CreateNewProject(new NewProjectInfo()
                {
                    ProjectFile = projectFileField.CurrentPath,
                    StartingChartPath = StartingChartFile,
                    BaseBPM = bpm,
                    AudioPath = FilePath.Local(dir, audioFileField.CurrentPath.FullPath),
                    JacketPath = jacketArtField.CurrentPath != null ? FilePath.Local(dir, jacketArtField.CurrentPath.FullPath) : null,
                    BackgroundPath = backgroundField.CurrentPath != null ? FilePath.Local(dir, backgroundField.CurrentPath.FullPath) : null,
                });

                Close();
                ClearFields();
            }
            else
            {
                Services.Popups.Notify(
                    Popups.Severity.Error,
                    I18n.S(
                        "Compose.Exception.InvalidBaseBPM",
                        baseBPMField.text));
            }
        }

        private void OnProjectFileSelect(FilePath path)
        {
            if (path == null)
            {
                return;
            }

            currentFolder = Path.GetDirectoryName(path.FullPath);
            AutofillFields();
        }

        private void OnChartFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                startingChartFileField.text = Values.DefaultChartFileName;
            }

            AutofillFields();
        }

        private void AutofillFields()
        {
            if (string.IsNullOrEmpty(currentFolder))
            {
                return;
            }

            string[] prefixes =
                string.IsNullOrEmpty(StartingChartFile) ?
                new string[] { Values.BaseFileName } :
                new string[]
                {
                    Values.BaseFileName,
                    Path.GetFileNameWithoutExtension(StartingChartFile),
                };

            string[] backgroundPrefixes =
                string.IsNullOrEmpty(StartingChartFile) ?
                new string[] { Values.BackgroundFileName } :
                new string[]
                {
                    Values.BackgroundFileName,
                    Path.GetFileNameWithoutExtension(Values.BackgroundFilePrefix + StartingChartFile),
                };

            AutofillFilefield(prefixes, Values.AudioExtensions, audioFileField);
            AutofillFilefield(prefixes, Values.ImageExtensions, jacketArtField);
            AutofillFilefield(backgroundPrefixes, Values.ImageExtensions, backgroundField);
        }

        private void AutofillFilefield(string[] prefixes, string[] exts, FileSelectField field)
        {
            foreach (string ext in exts)
            {
                foreach (string prefix in prefixes)
                {
                    if (LocalFileExists(prefix, ext, out string absolutePath))
                    {
                        field.SetPath(absolutePath);
                        return;
                    }
                }
            }
        }

        private void Awake()
        {
            confirmButton.onClick.AddListener(OnConfirm);
            closeButton.onClick.AddListener(Close);
            projectFileField.OnValueChanged += OnProjectFileSelect;
            startingChartFileField.onValueChanged.AddListener(OnChartFile);
            ClearFields();
        }

        private void OnDestroy()
        {
            confirmButton.onClick.RemoveListener(OnConfirm);
            closeButton.onClick.RemoveListener(Close);
            projectFileField.OnValueChanged -= OnProjectFileSelect;
            startingChartFileField.onValueChanged.RemoveListener(OnChartFile);
        }

        private void ClearFields()
        {
            projectFileField.ClearContent();
            startingChartFileField.text = Values.DefaultChartFileName;
            audioFileField.ClearContent();
            baseBPMField.text = Values.DefaultBpm;
            jacketArtField.ClearContent();
            backgroundField.ClearContent();
        }

        private bool LocalFileExists(string path, string ext, out string absolutePath)
        {
            absolutePath = Path.Combine(currentFolder, path, ext);
            return File.Exists(absolutePath);
        }
    }
}