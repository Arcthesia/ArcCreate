using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class DifficultyCell : MonoBehaviour
    {
        [SerializeField] private Graphic baseGraphic;
        [SerializeField] private Graphic gradietntGraphic;

        [Header("Animation")]
        [SerializeField] private float tweenDuration;
        [SerializeField] private Ease tweenEase;

        public Color Color
        {
            get => baseGraphic.color;
            set
            {
                baseGraphic.DOColor(value, tweenDuration).SetEase(tweenEase);
                gradietntGraphic.DOColor(InterfaceUtility.DarkenDiffColor(value), tweenDuration).SetEase(tweenEase);
            }
        }
    }
}