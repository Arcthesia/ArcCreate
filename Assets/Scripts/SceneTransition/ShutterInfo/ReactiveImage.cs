using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.SceneTransition
{
    [RequireComponent(typeof(Image))]
    public class ReactiveImage : MonoBehaviour
    {
        [SerializeField] private SpriteSO spriteSO;
        [SerializeField] private Image cachedImage;

        private void Awake()
        {
            cachedImage = GetComponent<Image>();
            spriteSO.OnValueChange.AddListener(OnSpriteChange);
        }

        private void OnDestroy()
        {
            spriteSO.OnValueChange.RemoveListener(OnSpriteChange);
        }

        private void OnSpriteChange(Sprite sprite)
        {
            cachedImage.sprite = sprite;
        }
    }
}