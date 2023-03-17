using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class DifficultyCell : MonoBehaviour
    {
        [SerializeField] private Graphic baseGraphic;
        [SerializeField] private Graphic gradietntGraphic;

        public Color Color
        {
            get => baseGraphic.color;
            set
            {
                baseGraphic.color = value;
                gradietntGraphic.color = InterfaceUtility.DarkenDiffColor(value);
            }
        }
    }
}