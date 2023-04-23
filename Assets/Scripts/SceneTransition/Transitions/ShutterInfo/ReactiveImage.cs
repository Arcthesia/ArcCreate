using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.SceneTransition
{
    [RequireComponent(typeof(Image))]
    public class ReactiveImage : MonoBehaviour
    {
        [SerializeField] private SpriteSO spriteSO;
        private Image cachedImage;

        private void Awake()
        {
            cachedImage = GetComponent<Image>();
            if (spriteSO != null)
            {
                spriteSO.OnValueChange.AddListener(OnSpriteChange);
                OnSpriteChange(spriteSO.Value);
            }
        }

        private void OnDestroy()
        {
            if (spriteSO != null)
            {
                spriteSO.OnValueChange.RemoveListener(OnSpriteChange);
            }
        }

        private void OnSpriteChange(Sprite sprite)
        {
            cachedImage.sprite = sprite;
        }
    }
}