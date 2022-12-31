using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.SceneTransition
{
    [RequireComponent(typeof(Image))]
    public class ReactiveImage : MonoBehaviour
    {
        [SerializeField] private SpriteSO spriteSO;
        [SerializeField] private ColorSO colorSO;
        private Image cachedImage;

        private void Awake()
        {
            cachedImage = GetComponent<Image>();
            if (spriteSO != null)
            {
                spriteSO.OnValueChange.AddListener(OnSpriteChange);
            }

            if (colorSO != null)
            {
                colorSO.OnValueChange.AddListener(OnColorChange);
            }
        }

        private void OnDestroy()
        {
            if (spriteSO != null)
            {
                spriteSO.OnValueChange.RemoveListener(OnSpriteChange);
            }

            if (colorSO != null)
            {
                colorSO.OnValueChange.RemoveListener(OnColorChange);
            }
        }

        private void OnSpriteChange(Sprite sprite)
        {
            cachedImage.sprite = sprite;
        }

        private void OnColorChange(Color color)
        {
            cachedImage.color = color;
        }
    }
}