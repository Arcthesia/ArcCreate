using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Utilities.ExternalAssets
{
    public class ExternalSpriteLoaderComponent : MonoBehaviour
    {
        [SerializeField] private string[] subdirectories;
        [SerializeField] private bool loadFullRect = false;

        private SpriteRenderer spriteRenderer;
        private Image image;

        private ExternalSprite externalSprite;

        private void Awake()
        {
            bool spriteRendererAvailable = TryGetComponent(out spriteRenderer);
            bool imageAvailable = TryGetComponent(out image);

            if (!spriteRendererAvailable && !imageAvailable)
            {
                throw new System.Exception("Must attach either an image component or a sprite renderer component to this object");
            }

            string subdirectory = System.IO.Path.Combine(subdirectories);
            externalSprite = spriteRendererAvailable ?
                new ExternalSprite(spriteRenderer.sprite, subdirectory, loadFullRect) :
                new ExternalSprite(image.sprite, subdirectory, loadFullRect);
            StartLoading().Forget();
        }

        private async UniTask StartLoading()
        {
            await externalSprite.Load();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = externalSprite.Value;
            }

            if (image != null)
            {
                image.sprite = externalSprite.Value;
            }
        }

        private void OnDestroy()
        {
            externalSprite.Unload();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = externalSprite.Value;
            }

            if (image != null)
            {
                image.sprite = externalSprite.Value;
            }
        }
    }
}