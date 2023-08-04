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

        public Theme Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChange.Invoke(value);
                PlayerPrefs.SetInt(playerPrefKey, (int)value);
            }
        }

        public Theme LastSelectedTheme
        {
            get => (Theme)PlayerPrefs.GetInt(playerPrefKey, (int)defaultTheme);
        }

        public OnChangeEvent OnValueChange { get; set; } = new OnChangeEvent();

        public class OnChangeEvent : UnityEvent<Theme>
        {
        }
    }
}