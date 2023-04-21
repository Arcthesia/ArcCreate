using ArcCreate.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class GradeDisplay : MonoBehaviour
    {
        [SerializeField] private GradeSprite[] gradeSprites;
        private Image image;

        public void Display(Grade grade)
        {
            foreach (var spr in gradeSprites)
            {
                if (spr.Grade == grade)
                {
                    image.sprite = spr.Sprite;
                    return;
                }
            }
        }

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        [System.Serializable]
        private struct GradeSprite
        {
            public Grade Grade;
            public Sprite Sprite;
        }
    }
}