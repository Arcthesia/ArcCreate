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
            if (audioFileField.CurrentPath == null
             || projectFileField.CurrentPath == null
             || startingChartFileField.text == null)
            {
                return;
            }

            Services.Project.CreateNewProject(new NewProjectInfo()
            {
                ProjectFile = projectFileField.CurrentPath,
                StartingChartPath = StartingChartFile,
                BaseBPM = Evaluator.Float(baseBPMField.text),
                AudioPath = audioFileField.CurrentPath,
                JacketPath = jacketArtField.CurrentPath,
                BackgroundPath = backgroundField.CurrentPath,
            });

            Close();
            ClearFields();
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
                startingChartFileField.text = Strings.DefaultChartFileName;
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