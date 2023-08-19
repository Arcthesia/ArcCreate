using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Utility
{
    public class I18nSelector : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private Button reloadButton;
        [SerializeField] private Button openFolderButton;
        [SerializeField] private Button reportMissingButton;
        private List<I18n.LocaleEntry> options;

        private void Awake()
        {
            dropdown.onValueChanged.AddListener(OnDropdown);
            I18n.OnLocaleChanged += Reload;

            if (reloadButton != null)
            {
                reloadButton.onClick.AddListener(Reload);
            }

            if (openFolderButton != null)
            {
                openFolderButton.onClick.AddListener(OpenFolder);
            }

            if (reportMissingButton != null)
            {
                reportMissingButton.onClick.AddListener(ReportMissing);
            }

            Reload();
        }

        private void OnDestroy()
        {
            dropdown.onValueChanged.RemoveListener(OnDropdown);
            I18n.OnLocaleChanged -= Reload;

            if (reloadButton != null)
            {
                reloadButton.onClick.RemoveListener(Reload);
            }

            if (openFolderButton != null)
            {
                openFolderButton.onClick.RemoveListener(OpenFolder);
            }

            if (reportMissingButton != null)
            {
                reportMissingButton.onClick.RemoveListener(ReportMissing);
            }
        }

        private void ReportMissing()
        {
            I18n.ReportMissingEntries().Forget();
        }

        private void OnDropdown(int opt)
        {
            Settings.Locale.Value = options[Mathf.Clamp(opt, 0, options.Count - 1)].Id;
            I18n.SetLocale(Settings.Locale.Value);
        }

        private void Reload()
        {
            options = I18n.LocaleList;
            dropdown.options = options.Select(s => new TMP_Dropdown.OptionData(s.LocalName)).ToList();

            bool found = false;
            for (int i = 0; i < options.Count; i++)
            {
                string s = options[i].Id;
                if (s == I18n.CurrentLocale)
                {
                    dropdown.SetValueWithoutNotify(i);
                    found = true;
                    break;
                }
            }

            if (found)
            {
                if (reportMissingButton != null)
                {
                    reportMissingButton.interactable = I18n.CurrentLocale != I18n.DefaultLocale;
                }
            }
        }

        private void OpenFolder()
        {
            Shell.OpenExplorer(I18n.LocaleDirectory);
        }
    }
}