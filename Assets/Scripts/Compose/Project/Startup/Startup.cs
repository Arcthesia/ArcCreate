using System.Collections.Generic;
using System.IO;
using ArcCreate.Data;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class Startup : MonoBehaviour
    {
        private const string PlayerPrefKey = "Startup.RecentProjects";
        private const int MaxProjectHistoryCount = 10;
        [SerializeField] private Text test;
        [SerializeField] private Button[] closeButtons;
        [SerializeField] private RecentProjectButton[] recentProjectButtons;

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            foreach (var button in closeButtons)
            {
                button.onClick.AddListener(Close);
            }

            Services.Project.OnProjectLoad += OnProjectLoad;
            AssignRecentProjects();
        }

        private void OnDestroy()
        {
            foreach (var button in closeButtons)
            {
                button.onClick.RemoveListener(Close);
            }

            Services.Project.OnProjectLoad -= OnProjectLoad;
        }

        private void OnProjectLoad(ProjectSettings settings)
        {
            Close();

            string recentProjectsPrefs = PlayerPrefs.GetString(PlayerPrefKey);
            List<string> split = new List<string>();
            if (!string.IsNullOrEmpty(recentProjectsPrefs))
            {
                split = JsonConvert.DeserializeObject<List<string>>(recentProjectsPrefs);
            }

            split.RemoveAll(path => !File.Exists(path) || path == settings.Path);
            split.Insert(0, settings.Path);
            while (split.Count > MaxProjectHistoryCount)
            {
                split.RemoveAt(split.Count - 1);
            }

            PlayerPrefs.SetString(PlayerPrefKey, JsonConvert.SerializeObject(split));
        }

        private void AssignRecentProjects()
        {
            string recentProjectsPrefs = PlayerPrefs.GetString(PlayerPrefKey);
            if (string.IsNullOrEmpty(recentProjectsPrefs))
            {
                foreach (var b in recentProjectButtons)
                {
                    b.SetProjectPath(null);
                }

                return;
            }

            try
            {
                List<string> split = JsonConvert.DeserializeObject<List<string>>(recentProjectsPrefs);
                split.RemoveAll(path => !File.Exists(path));
                int validPathCount = Mathf.Min(split.Count, recentProjectButtons.Length);
                for (int i = 0; i < validPathCount; i++)
                {
                    string path = split[i];
                    recentProjectButtons[i].SetProjectPath(path);
                }

                for (int i = validPathCount; i < recentProjectButtons.Length; i++)
                {
                    recentProjectButtons[i].SetProjectPath(null);
                }

                PlayerPrefs.SetString(PlayerPrefKey, JsonConvert.SerializeObject(split));
            }
            catch
            {
            }
        }
    }
}