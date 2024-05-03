using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    public class CameraViewRow : MonoBehaviour
    {
        [SerializeField] private Button mainButton;
        [SerializeField] private TMP_Text label;
        private CameraViewProperty data;
        private CameraViews manager;

        public void SetData(CameraViews manager, CameraViewProperty data)
        {
            this.manager = manager;
            this.data = data;
            OnLocale();
        }

        private void OnLocale()
        {
            this.label.text = I18n.S(data.Name);
        }

        private void Awake()
        {
            mainButton.onClick.AddListener(Select);
            I18n.OnLocaleChanged += OnLocale;
        }

        private void OnDestroy()
        {
            mainButton.onClick.RemoveListener(Select);
            I18n.OnLocaleChanged -= OnLocale;
        }

        private void Select()
        {
            manager.Select(data);
        }
    }
}