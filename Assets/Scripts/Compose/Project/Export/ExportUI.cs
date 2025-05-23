using System;
using System.IO;
using ArcCreate.Data;
using ArcCreate.Utility;
using ArcCreate.Utility.Parser;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class ExportUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField publisherField;
        [SerializeField] private TMP_InputField packageNameField;
        [SerializeField] private TMP_InputField versionField;
        [SerializeField] private GameObject requiredIndicator;
        [SerializeField] private TMP_Text identifierPreview;
        [SerializeField] private Button confirmButton;

        private void Awake()
        {
            confirmButton.onClick.AddListener(OnStartExport);
            publisherField.onValueChanged.AddListener(OnInfoChange);
            packageNameField.onValueChanged.AddListener(OnInfoChange);
            Services.Project.OnProjectLoad += OnProjectChange;
        }

        private void OnDestroy()
        {
            confirmButton.onClick.RemoveListener(OnStartExport);
            publisherField.onValueChanged.RemoveListener(OnInfoChange);
            packageNameField.onValueChanged.RemoveListener(OnInfoChange);
            Services.Project.OnProjectLoad -= OnProjectChange;
        }

        private void OnProjectChange(ProjectSettings settings)
        {
            if (settings.EditorSettings != null)
            {
                if (!string.IsNullOrWhiteSpace(settings.EditorSettings.LastUsedPublisher))
                {
                    publisherField.text = settings.EditorSettings.LastUsedPublisher;
                }
                else
                {
                    publisherField.text = "";
                }

                if (!string.IsNullOrWhiteSpace(settings.EditorSettings.LastUsedPackageName))
                {
                    packageNameField.text = settings.EditorSettings.LastUsedPackageName;
                }
                else
                {
                    packageNameField.text = "";
                }

                versionField.text = settings.EditorSettings.LastUsedVersionNumber.ToString();
            }
            else
            {
                publisherField.text = "";
                packageNameField.text = "";
                versionField.text = "0";
            }

            if (string.IsNullOrWhiteSpace(publisherField.text) && !string.IsNullOrWhiteSpace(Settings.LastUsedPublisherName.Value))
            {
                publisherField.text = Settings.LastUsedPublisherName.Value;
            }
        }

        private void OnInfoChange(string arg)
        {
            bool valid = !string.IsNullOrWhiteSpace(publisherField.text) && !string.IsNullOrWhiteSpace(packageNameField.text);
            requiredIndicator.SetActive(!valid);
            identifierPreview.gameObject.SetActive(valid);
            identifierPreview.text = I18n.S("Compose.UI.Export.Package.Identifier", $"{publisherField.text}.{packageNameField.text}");
        }

        private void OnStartExport()
        {
            StartExport().Forget();
        }

        private async UniTask StartExport()
        {
            string publisher = publisherField.text;
            string package = packageNameField.text;
            Evaluator.TryInt(versionField.text, out int version);
            DateTime builtAt = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(publisher) || string.IsNullOrWhiteSpace(package))
            {
                requiredIndicator.SetActive(true);
                return;
            }

            requiredIndicator.SetActive(false);

            ProjectSettings proj = Services.Project.CurrentProject;
            string outputPath = null;
            var cancelled = false;
            if (Settings.UseNativeFileBrowser.Value)
            {
                outputPath = Shell.SaveFileDialog(
                    "ArcCreate package",
                    new string[] { "arcpkg" },
                    "Export package",
                    Path.GetDirectoryName(proj.Path),
                    $"{publisher}.{package}");
            }
            else
            {
                var filter = new SimpleFileBrowser.FileBrowser.Filter("ArcCreate package", new [] { "arcpkg" });
                SimpleFileBrowser.FileBrowser.SetFilters(false, filter);
                SimpleFileBrowser.FileBrowser.ShowSaveDialog(
                    onSuccess: (string[] paths) => {
                        if (paths.Length >= 1)
                        {
                            outputPath = paths[0];
                        }
                    },
                    onCancel: () => {
                        cancelled = true;
                    },
                    pickMode: SimpleFileBrowser.FileBrowser.PickMode.Files,
                    allowMultiSelection: false,
                    initialPath: Path.GetDirectoryName(proj.Path),
                    initialFilename: $"{publisher}.{package}.arcpkg",
                    title: "Export package");
                while (outputPath == null && !cancelled)
                {
                    await UniTask.NextFrame();
                }
            }

            if (string.IsNullOrWhiteSpace(outputPath) || cancelled)
            {
                return;
            }

            if (proj.EditorSettings == null)
            {
                proj.EditorSettings = new EditorProjectSettings
                {
                    LastUsedPublisher = publisher,
                    LastUsedPackageName = package,
                    LastUsedVersionNumber = version,
                };
            }
            else
            {
                proj.EditorSettings.LastUsedPackageName = publisher;
                proj.EditorSettings.LastUsedPackageName = package;
                proj.EditorSettings.LastUsedVersionNumber = version;
            }

            Settings.LastUsedPublisherName.Value = publisher;
            Services.Project.SaveProject();

            new Exporter(proj, publisher, package, version).Export(outputPath);
        }
    }
}