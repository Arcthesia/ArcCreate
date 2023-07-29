using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class RecentProjectButton : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;
        private RectTransform rect;

        public string ProjectPath { get; private set; }

        public void SetProjectPath(string projectPath)
        {
            ProjectPath = projectPath;
            if (projectPath == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            label.text = ExtractLabelFor(projectPath);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Services.Popups.ShowHint(eventData.position, Popups.Severity.Info, ProjectPath, rect, eventData.enterEventCamera);
        }

        private string ExtractLabelFor(string projectPath)
        {
            return new DirectoryInfo(Path.GetDirectoryName(projectPath)).Name;
        }

        private void Awake()
        {
            button.onClick.AddListener(OpenProject);
            rect = GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OpenProject);
        }

        private void OpenProject()
        {
            Services.Project.OpenProject(ProjectPath);
        }
    }
}