using System;
using System.IO;
using ArcCreate.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class FileSelectField : MonoBehaviour
    {
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private TMP_Text placeholderText;
        [SerializeField] private Button openButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private GameObject invalidIndicator;
        [SerializeField] private bool required;
        [SerializeField] private string title;
        [SerializeField] private string initPathPrefKey;
        [SerializeField] private string extensionFilterName;
        [SerializeField] private string[] acceptedExtensions;

        public event Action<string> OnValueChanged;

        public string CurrentPath { get; private set; } = null;

        public bool IsValidPath => File.Exists(CurrentPath);

        protected virtual void OnValidFilePath(string path)
        {
        }

        protected virtual void OnInvalidFilePath()
        {
        }

        protected void Awake()
        {
            if (!required)
            {
                invalidIndicator.SetActive(false);
            }
            else
            {
                invalidIndicator.SetActive(string.IsNullOrEmpty(contentText.text));
            }

            if (!string.IsNullOrEmpty(contentText.text))
            {
                if (File.Exists(contentText.text))
                {
                    CurrentPath = contentText.text;
                    invalidIndicator.SetActive(false);
                }
                else
                {
                    contentText.text = string.Empty;
                    invalidIndicator.SetActive(required);
                }
            }

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
            string path = Shell.OpenFileDialog(extensionFilterName, acceptedExtensions, title, PlayerPrefs.GetString(initPathPrefKey, ""));
            PlayerPrefs.SetString(initPathPrefKey, Path.GetDirectoryName(path));

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            CurrentPath = path;
            OnValidFilePath(CurrentPath);
            OnValueChanged?.Invoke(CurrentPath);

            contentText.text = path;
            contentText.gameObject.SetActive(true);
            placeholderText.gameObject.SetActive(false);
            invalidIndicator.SetActive(false);
            clearButton.gameObject.SetActive(true);
        }

        private void ClearContent()
        {
            CurrentPath = null;
            OnInvalidFilePath();
            OnValueChanged?.Invoke(null);

            contentText.text = string.Empty;
            contentText.gameObject.SetActive(false);
            placeholderText.gameObject.SetActive(true);
            if (required)
            {
                invalidIndicator.SetActive(true);
            }

            clearButton.gameObject.SetActive(false);
        }
    }
}