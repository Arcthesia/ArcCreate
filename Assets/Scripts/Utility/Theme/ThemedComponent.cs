using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Utility
{
    [RequireComponent(typeof(Graphic))]
    public class ThemedComponent : MonoBehaviour
    {
        [SerializeField] private ThemeGroup themeGroup;
        [SerializeField] private ThemeColor themeColor;

        private Graphic graphic;

        private void Awake()
        {
            graphic = GetComponent<Graphic>();
            themeGroup.OnValueChange.AddListener(OnThemeChange);
            OnThemeChange(themeGroup.Value);
        }

        private void OnDestroy()
        {
            themeGroup.OnValueChange.RemoveListener(OnThemeChange);
        }

        private void OnThemeChange(Theme theme)
        {
            graphic.color = themeColor.GetColor(theme);
        }
    }
}