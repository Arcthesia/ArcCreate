using System.Linq;
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
        private string[] options;

        private void Awake()
        {
            dropdown.onValueChanged.AddListener(OnDropdown);
            Settings.Locale.OnValueChanged.AddListener(OnSettings);

            if (reloadButton != null)
            {
                reloadButton.onClick.AddListener(Reload);
            }

            if (openFolderButton != null)
            {
                openFolderButton.onClick.AddListener(OpenFolder);
            }

            Reload();
        }

        private void OnDestroy()
        {
            dropdown.onValueChanged.RemoveListener(OnDropdown);
            Settings.Locale.OnValueChanged.RemoveListener(OnSettings);

            if (reloadButton != null)
            {
                reloadButton.onClick.RemoveListener(Reload);
            }

            if (openFolderButton != null)
            {
                openFolderButton.onClick.RemoveListener(OpenFolder);
            }
        }

        private void OnDropdown(int opt)
        {
            Settings.Locale.Value = options[Mathf.Clamp(opt, 0, options.Length - 1)];
        }

        private void OnSettings(string locale)
        {
            I18n.SetLocale(locale);
        }

        private void Reload()
        {
            options = I18n.ListAllLocales();
            dropdown.options = options.Select(s => new TMP_Dropdown.OptionData(s)).ToList();

            bool found = false;
            for (int i = 0; i < options.Length; i++)
            {
                string s = options[i];
                if (s == Settings.Locale.Value)
                {
                    dropdown.SetValueWithoutNotify(i);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                dropdown.SetValueWithoutNotify(0);
                Settings.Locale.Value = options[0];
            }
            else
            {
                I18n.SetLocale(Settings.Locale.Value);
            }
        }

        private void OpenFolder()
        {
            Shell.OpenExplorer(I18n.LocaleDirectory);
        }
    }
}