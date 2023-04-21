using ArcCreate.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    [RequireComponent(typeof(Image))]
    public class ClearResultDisplay : MonoBehaviour
    {
        [SerializeField] private ClearResultSprite[] resultSprites;
        private Image image;

        public void Display(ClearResult result)
        {
            foreach (var spr in resultSprites)
            {
                if (spr.Result == result)
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
        private struct ClearResultSprite
        {
            public ClearResult Result;
            public Sprite Sprite;
        }
    }
}