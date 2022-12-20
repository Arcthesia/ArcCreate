using System.Collections.Generic;
using System.IO;
using ArcCreate.Compose.Components;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class NewProjectDialog : MonoBehaviour
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

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void OnConfirm()
        {
            ProjectSettings project = new ProjectSettings()
            {
                Path = projectFileField.CurrentPath,
                LastOpenedChartPath = StartingChartFile,
                Charts = new List<ChartSettings>()
                {
                    new ChartSettings()
                    {
                        ChartPath = StartingChartFile,
                        BaseBpm = Evaluator.Float(baseBPMField.text),
                        AudioPath = audioFileField.CurrentPath,
                        JacketPath = jacketArtField.CurrentPath,
                        BackgroundPath = backgroundField.CurrentPath,
                    },
                },
            };

            Services.Project.CreateNewProject(project);

            Close();
            ClearFields();
        }

        private void OnFolderSelect(string folder)
        {
            currentFolder = Path.GetDirectoryName(folder);
            AutofillFields();
        }

        private void OnChartFile(string path)
        {
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
                new string[] { Strings.BaseFileName } :
                new string[]
                {
                    Strings.BaseFileName,
                    Path.GetFileNameWithoutExtension(StartingChartFile),
                };

            string[] backgroundPrefixes =
                string.IsNullOrEmpty(StartingChartFile) ?
                new string[] { Strings.BackgroundFileName } :
                new string[]
                {
                    Strings.BackgroundFileName,
                    Path.GetFileNameWithoutExtension(Strings.BackgroundFilePrefix + StartingChartFile),
                };

            AutofillFilefield(prefixes, Strings.AudioExtensions, audioFileField);
            AutofillFilefield(prefixes, Strings.ImageExtensions, jacketArtField);
            AutofillFilefield(backgroundPrefixes, Strings.ImageExtensions, backgroundField);
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
            projectFileField.OnValueChanged += OnFolderSelect;
            startingChartFileField.onValueChanged.AddListener(OnChartFile);
            ClearFields();
        }

        private void OnDestroy()
        {
            confirmButton.onClick.RemoveListener(OnConfirm);
            closeButton.onClick.RemoveListener(Close);
            projectFileField.OnValueChanged -= OnFolderSelect;
            startingChartFileField.onValueChanged.RemoveListener(OnChartFile);
        }

        private void ClearFields()
        {
            projectFileField.ClearContent();
            startingChartFileField.text = Strings.DefaultChartFileName;
            audioFileField.ClearContent();
            baseBPMField.text = Strings.DefaultBpm;
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