using System;
using System.IO;
using ArcCreate.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Field for selecting a file on the file system.
    /// This will open the operating system's file picker.
    /// </summary>
    public class FileSelectField : MonoBehaviour
    {
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private TMP_Text placeholderText;
        [SerializeField] private Button openButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private GameObject invalidIndicator;
        [SerializeField] private bool required;
        [SerializeField] private bool isSaveFile;
        [SerializeField] private bool isLocalFileSelector;
        [SerializeField] private string title;
        [SerializeField] private string initPathPrefKey;
        [SerializeField] private bool modifyPref = true;
        [SerializeField] private string extensionFilterName;
        [SerializeField] private string[] acceptedExtensions;
        [SerializeField] private string defaultSaveFileName;

        /// <summary>
        /// Event invoked after a file path has been set.
        /// </summary>
        public event Action<FilePath> OnValueChanged;

        /// <summary>
        /// Gets the currently selected file path.
        /// </summary>
        public FilePath CurrentPath { get; private set; } = null;

        /// <summary>
        /// Gets a value indicating whether or not the current path exists on the file system.
        /// </summary>
        public bool IsValidPath => File.Exists(CurrentPath.FullPath);

        public string[] AcceptedExtensions
        {
            get => acceptedExtensions;
            set => acceptedExtensions = value;
        }

        private string CurrentProjectFolder => Path.GetDirectoryName(Services.Project.CurrentProject.Path);

        /// <summary>
        /// Clear this field.
        /// This will invoke <see cref="OnValueChanged"/> event with a null value.
        /// </summary>
        public void ClearContent()
        {
            ClearContentWithoutNotify();
            OnValueChanged?.Invoke(null);
        }

        public void ClearContentWithoutNotify()
        {
            CurrentPath = null;
            OnInvalidFilePath();

            contentText.text = string.Empty;
            contentText.gameObject.SetActive(false);
            placeholderText.gameObject.SetActive(true);

            invalidIndicator.SetActive(required);
            clearButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Set the path for this field and invoke <see cref="OnValueChangeed"/> event.
        /// If the provided path is invalid, no change will be made.
        /// </summary>
        /// <param name="path">The file path.</param>
        public void SetPath(string path)
        {
            FilePath oldPath = CurrentPath;
            SetPathWithoutNotify(path);

            if (oldPath != CurrentPath)
            {
                OnValueChanged?.Invoke(CurrentPath);
            }
        }

        /// <summary>
        /// Set the path for this field without invoking <see cref="OnValueChangeed"/> event.
        /// If the provided path is invalid, no change will be made if this field is required,
        /// or the field will be cleared if this field is not required.
        /// </summary>
        /// <param name="path">The file path.</param>
        public void SetPathWithoutNotify(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                if (!required)
                {
                    ClearContentWithoutNotify();
                }

                return;
            }

            bool found = false;
            if (isLocalFileSelector)
            {
                if (File.Exists(path))
                {
                    FilePath newFilePath = FilePath.Local(CurrentProjectFolder, path);
                    if (newFilePath.ShouldCopy)
                    {
                        newFilePath.RenameUntilNoOverwrite();
                        File.Copy(path, newFilePath.FullPath, true);
                    }

                    CurrentPath = newFilePath;
                    found = true;
                }
                else
                {
                    string full = Path.Combine(CurrentProjectFolder, path);
                    if (File.Exists(full))
                    {
                        CurrentPath = FilePath.Local(CurrentProjectFolder, full);
                        found = true;
                    }
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    FilePath newFilePath = FilePath.Global(path);
                    CurrentPath = newFilePath;
                    found = true;
                }
            }

            if (found)
            {
                contentText.text = CurrentPath.ShortenedPath;
                contentText.gameObject.SetActive(true);
                placeholderText.gameObject.SetActive(false);
                invalidIndicator.SetActive(false);
                clearButton.gameObject.SetActive(true);
                OnValidFilePath(CurrentPath);
            }
        }

        protected virtual void OnValidFilePath(FilePath path)
        {
        }

        protected virtual void OnInvalidFilePath()
        {
        }

        protected void Awake()
        {
            contentText.text = string.Empty;
            invalidIndicator.SetActive(required);

            openButton.onClick.AddListener(OnOpenBrowserClick);
            clearButton.onClick.AddListener(ClearContent);
        }

        protected void OnDestroy()
        {
            openButton.onClick.RemoveAllListeners();
            clearButton.onClick.RemoveAllListeners();
        }

        private void OnOpenBrowserClick()
        {
            string path =
                isSaveFile ?
                Shell.SaveFileDialog(extensionFilterName, acceptedExtensions, title, PlayerPrefs.GetString(initPathPrefKey, ""), defaultSaveFileName) :
                Shell.OpenFileDialog(extensionFilterName, acceptedExtensions, title, PlayerPrefs.GetString(initPathPrefKey, ""));
            if (modifyPref)
            {
                PlayerPrefs.SetString(initPathPrefKey, Path.GetDirectoryName(path));
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (isLocalFileSelector)
            {
                CurrentPath = FilePath.Local(CurrentProjectFolder, path);
                if (CurrentPath.ShouldCopy)
                {
                    CurrentPath.RenameUntilNoOverwrite();
                    File.Copy(path, CurrentPath.FullPath, true);
                }
            }
            else
            {
                CurrentPath = FilePath.Global(path);
            }

            OnValidFilePath(CurrentPath);
            OnValueChanged?.Invoke(CurrentPath);

            contentText.text = CurrentPath.ShortenedPath;
            contentText.gameObject.SetActive(true);
            placeholderText.gameObject.SetActive(false);
            invalidIndicator.SetActive(false);
            clearButton.gameObject.SetActive(true);
        }
    }
}