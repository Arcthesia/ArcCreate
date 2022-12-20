using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class GameplayViewport : MonoBehaviour
    {
        [SerializeField] private float debounceSeconds = 1f;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RawImage viewportImage;

        private float previousWidth;
        private float previousHeight;
        private float applyNewSizeAfter = float.MaxValue;

        private void Update()
        {
            float width = viewport.rect.width;
            float height = viewport.rect.height;
            if (width != previousWidth || height != previousHeight)
            {
                applyNewSizeAfter = Time.realtimeSinceStartup + debounceSeconds;
            }

            previousWidth = width;
            previousHeight = height;

            if (Time.realtimeSinceStartup >= applyNewSizeAfter)
            {
                var texture = viewportImage.texture as RenderTexture;
                if (texture == null)
                {
                    return;
                }

                texture.Release();
                texture.width = (int)width;
                texture.height = (int)height;
                Services.Gameplay.ApplyAspect(width / height);
                applyNewSizeAfter = float.MaxValue;
            }
        }
    }
}