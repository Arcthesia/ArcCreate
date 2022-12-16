using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class Option : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private string value;
        [SerializeField] private Graphic highlight;
        private bool highlighted = false;

        public OptionsPanel Panel { get; set; }

        public bool Highlighted
        {
            get => highlighted;
            set
            {
                highlighted = value;
                highlight.enabled = value;
            }
        }

        public string Value => value;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Panel == null)
            {
                return;
            }

            Panel.OnOptionSelected(this);
        }
    }
}