using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class PanelLayoutRow : MonoBehaviour
    {
        [SerializeField] private Button mainButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private TMP_Text label;
        private byte[] data;
        private PanelLayoutManager manager;

        public string Label => label.text;

        public void SetData(PanelLayoutManager manager, byte[] data, string label)
        {
            this.manager = manager;
            this.data = data;
            this.label.text = label;
        }

        private void Awake()
        {
            mainButton.onClick.AddListener(Select);
            removeButton.onClick.AddListener(Remove);
        }

        private void OnDestroy()
        {
            mainButton.onClick.RemoveListener(Select);
            removeButton.onClick.RemoveListener(Remove);
        }

        private void Remove()
        {
            manager.Remove(label.text);
        }

        private void Select()
        {
            manager.Select(data);
        }
    }
}