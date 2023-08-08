using UnityEngine;
using UnityEngine.Events;

namespace ArcCreate.Utility
{
    [CreateAssetMenu(fileName = "Theme", menuName = "ScriptableObject/Theme")]
    public class ThemeGroup : ScriptableObject
    {
        [SerializeField] private string playerPrefKey;
        [SerializeField] private Theme defaultTheme;

        private Theme value;
        private Option<Theme> overrideValue;

        public Theme Value
        {
            get => value;
            set
            {
                this.value = value;
                Update();
            }
        }

        public Option<Theme> OverrideValue
        {
            get => overrideValue;
            set
            {
                overrideValue = value;
                Update();
            }
        }

        public Theme LastSelectedTheme
        {
            get => (Theme)PlayerPrefs.GetInt(playerPrefKey, (int)defaultTheme);
        }

        public OnChangeEvent OnValueChange { get; set; } = new OnChangeEvent();

        private void Update()
        {
            Theme theme = value;
            if (overrideValue.HasValue)
            {
                theme = overrideValue.Value;
            }

            OnValueChange.Invoke(theme);
            PlayerPrefs.SetInt(playerPrefKey, (int)theme);
        }

        public class OnChangeEvent : UnityEvent<Theme>
        {
        }
    }
}