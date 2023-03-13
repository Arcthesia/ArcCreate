using System;
using System.IO;
using ArcCreate.Data;
using ArcCreate.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class ExportUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField publisherField;
        [SerializeField] private TMP_InputField packageNameField;
        [SerializeField] private GameObject requiredIndicator;
        [SerializeField] private TMP_Text identifierPreview;
        [SerializeField] private Button confirmButton;

        private void Awake()
        {
            confirmButton.onClick.AddListener(StartExport);
            publisherField.onValueChanged.AddListener(OnInfochange);
            packageNameField.onValueChanged.AddListener(OnInfochange);
        }

        private void OnDestroy()
        {
            confirmButton.onClick.RemoveListener(StartExport);
            publisherField.onValueChanged.RemoveListener(OnInfochange);
            packageNameField.onValueChanged.RemoveListener(OnInfochange);
        }

        private void OnInfochange(string arg)
        {
            identifierPreview.text = I18n.S("Compose.UI.Export.Package.Identifier", $"{publisherField.text}.{packageNameField.text}");
            bool valid = !string.IsNullOrEmpty(publisherField.text) && !string.IsNullOrEmpty(packageNameField.text);
            requiredIndicator.SetActive(!valid);
            identifierPreview.gameObject.SetActive(valid);
        }

        private void StartExport()
        {
            string publisher = publisherField.text;
            string package = packageNameField.text;
            DateTime builtAt = DateTime.UtcNow;

            if (string.IsNullOrEmpty(publisher) || string.IsNullOrEmpty(package))
            {
                requiredIndicator.SetActive(true);
                return;
            }

            requiredIndicator.SetActive(false);

            ProjectSettings proj = Services.Project.CurrentProject;
            string outputPath = Shell.SaveFileDialog(
                "ArcCreate package",
                new string[] { "arcpkg" },
                "Export package",
                Path.GetDirectoryName(proj.Path),
                $"{publisher}.{package}");

            if (string.IsNullOrEmpty(outputPath))
            {
                return;
            }

            if (Values.ProjectModified)
            {
                Services.Project.SaveProject();
            }

            new Exporter(proj, publisher, package, builtAt).Export(outputPath);
        }
    }
}