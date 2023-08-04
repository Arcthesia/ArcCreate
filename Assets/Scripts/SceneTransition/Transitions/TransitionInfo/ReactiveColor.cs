using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.SceneTransition
{
    [RequireComponent(typeof(Graphic))]
    public class ReactiveColor : MonoBehaviour
    {
        [SerializeField] private ColorSO colorSO;
        private Graphic cachedGraphic;

        private void Awake()
        {
            cachedGraphic = GetComponent<Graphic>();
            if (colorSO != null)
            {
                colorSO.OnValueChange.AddListener(OnColorChange);
                OnColorChange(colorSO.Value);
            }
        }

        private void OnDestroy()
        {
            if (colorSO != null)
            {
                colorSO.OnValueChange.RemoveListener(OnColorChange);
            }
        }

        private void OnColorChange(Color color)
        {
            cachedGraphic.color = color;
        }
    }
}