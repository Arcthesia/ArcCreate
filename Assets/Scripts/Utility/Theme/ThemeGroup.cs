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

        public Option<Theme> OverrideValue
        {
            get => overrideValue;
            set
            {
                overrideValue = value;
                Update();
            }
        }

        public Theme Value
        {
            get
            {
                Theme theme = value;
                if (overrideValue.HasValue)
                {
                    theme = overrideValue.Value;
                }

                return theme;
            }

            set
            {
                this.value = value;
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
            Theme theme = Value;
            OnValueChange.Invoke(theme);
            PlayerPrefs.SetInt(playerPrefKey, (int)theme);
        }

        public class OnChangeEvent : UnityEvent<Theme>
        {
        }
    }
}