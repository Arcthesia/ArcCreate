using ArcCreate.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    [RequireComponent(typeof(Image))]
    public class GradeDisplay : MonoBehaviour
    {
        [SerializeField] private GradeSprite[] gradeSprites;

        public void Display(Grade grade)
        {
            Image image = GetComponent<Image>();
            foreach (var spr in gradeSprites)
            {
                if (spr.Grade == grade)
                {
                    image.sprite = spr.Sprite;
                    return;
                }
            }
        }

        [System.Serializable]
        private struct GradeSprite
        {
            public Grade Grade;
            public Sprite Sprite;
        }
    }
}