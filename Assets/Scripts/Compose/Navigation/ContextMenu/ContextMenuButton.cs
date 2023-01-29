using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Navigation
{
    public class ContextMenuButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Button button;
        private IAction action;
        private ContextMenu menu;

        public float Width { get; private set; }

        public RectTransform Rect { get; private set; }

        public void Setup(IAction action, ContextMenu menu)
        {
            this.action = action;
            this.menu = menu;
            string content = I18n.S(action.I18nName);
            text.text = content;
            Width = text.GetPreferredValues(content).x;
        }

        private void OnClick()
        {
            if (Services.Navigation.ShouldExecute(action))
            {
                action.Execute();
                menu.CloseContextMenu();
            }
        }

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
            Rect = GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }
    }
}