using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ArcCreate.Compose.Timeline
{
    [RequireComponent(typeof(RawImage))]
    public class WaveformDisplay : MonoBehaviour, IScrollHandler
    {
        [SerializeField] private Camera mainCamera;
        private RawImage image;
        private RectTransform imageTransform;
        [SerializeField] private float minViewLength = 0.5f;
        [SerializeField] private double debounceSeconds = 0.2;
        [SerializeField] private AudioClipSO audioClipSO;
        [SerializeField] private Color waveformBg;
        [SerializeField] private Color waveformColor;

        private float scrollPivot;
        [SerializeField] private float viewFromSecond;
        [SerializeField] private float viewToSecond;
        private float textureViewFromSecond;
        private float textureViewToSecond;
        private float minViewLengthOfClip;
        private Vector2 baseAnchoredPosition;
        [SerializeField] private double scheduledReset = double.MaxValue;
        private AudioClip clip;
        private Vector2 previousRect;
        private Keyboard keyboard;

        public void OnScroll(PointerEventData eventData)
        {
            Vector2 scrollDelta = eventData.scrollDelta;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(imageTransform, eventData.position, null, out Vector2 local);
            scrollPivot = (local.x / imageTransform.rect.width) + 0.5f;
            float scrollDir = Mathf.Sign(scrollDelta.y);

            if (keyboard.shiftKey.isPressed)
            {
                scrollDir *= 4;
            }

            if (keyboard.ctrlKey.isPressed)
            {
                float pivotSecond = Mathf.Lerp(viewFromSecond, viewToSecond, scrollPivot);
                float oldViewSize = viewToSecond - viewFromSecond;
                float viewSize = oldViewSize * (1 - (scrollDir / 10));
                viewSize = Mathf.Max(viewSize, minViewLengthOfClip);

                // x -----scrollPivot ------ pivotSecond -------- 1 - scrollPivot --------- y
                // [<--------------------------viewSize------------------------------------>]
                viewFromSecond = pivotSecond - (viewSize * scrollPivot);
                viewToSecond = pivotSecond + (viewSize * (1 - scrollPivot));

                viewFromSecond = Mathf.Clamp(viewFromSecond, 0, clip.length);
                viewToSecond = Mathf.Clamp(viewToSecond, 0, clip.length);

                if (viewFromSecond > viewToSecond)
                {
                    (viewFromSecond, viewToSecond) = (viewToSecond, viewFromSecond);
                }
            }
            else
            {
                float viewSize = viewToSecond - viewFromSecond;
                viewFromSecond += scrollDir / 10;
                viewToSecond += scrollDir / 10;

                viewToSecond = Mathf.Clamp(viewToSecond, 0, clip.length);
                viewFromSecond = Mathf.Clamp(viewFromSecond, 0, viewToSecond - viewSize);

                viewFromSecond = Mathf.Clamp(viewFromSecond, 0, clip.length);
                viewToSecond = Mathf.Clamp(viewToSecond, viewFromSecond + viewSize, clip.length);
            }

            // .        viewFrom-----------------------------viewTo
            // .        [<--------------------------------------->]
            // textureViewFrom-----------------------------textureViewTo
            // [<----------------------------------------------------->]
            AlignView();
        }

        private void AlignView()
        {
            float tempScaleX = Mathf.Abs((textureViewToSecond - textureViewFromSecond) / (viewToSecond - viewFromSecond));
            imageTransform.localScale = new Vector2(tempScaleX, 1);

            float pivotX = imageTransform.pivot.x;
            float pivotXSecond = Mathf.Lerp(viewFromSecond, viewToSecond, pivotX);
            float pivotXTextureSecond = Mathf.Lerp(textureViewFromSecond, textureViewToSecond, pivotX);
            float diffSecondToMove = pivotXSecond - pivotXTextureSecond;
            float diffXToMove = diffSecondToMove / (viewToSecond - viewFromSecond) * imageTransform.rect.width;

            imageTransform.anchoredPosition = baseAnchoredPosition - new Vector2(diffXToMove, 0);

            scheduledReset = Time.realtimeSinceStartup + debounceSeconds;
        }

        private void SetWave()
        {
            float midPoint = (viewFromSecond + viewToSecond) / 2;
            float length = Mathf.Abs(viewToSecond - viewFromSecond);
            minViewLengthOfClip = Mathf.Min(minViewLength, clip.length);

            textureViewFromSecond = midPoint - length;
            textureViewToSecond = midPoint + length;
            textureViewFromSecond = Mathf.Clamp(textureViewFromSecond, 0, clip.length);
            textureViewToSecond = Mathf.Clamp(textureViewToSecond, 0, clip.length);

            float width = imageTransform.rect.width * (textureViewToSecond - textureViewFromSecond) / length;
            Vector2 size = new Vector2(width, imageTransform.rect.height);
            if (size.x > 0 && size.y > 0)
            {
                image.texture = WaveformGenerator.GetWaveformTexture(clip, size, textureViewFromSecond, textureViewToSecond, waveformBg, waveformColor);
            }

            AlignView();
        }

        private void Update()
        {
            if (clip != null && (imageTransform.rect.width != previousRect.x || imageTransform.rect.height != previousRect.y))
            {
                scheduledReset = Time.realtimeSinceStartup + debounceSeconds;
            }

            previousRect = new Vector2(imageTransform.rect.width, imageTransform.rect.height);

            if (Time.realtimeSinceStartup > scheduledReset)
            {
                SetWave();
                scheduledReset = double.MaxValue;
            }
        }

        private void Awake()
        {
            image = GetComponent<RawImage>();
            imageTransform = GetComponent<RectTransform>();

            baseAnchoredPosition = imageTransform.anchoredPosition;

            viewFromSecond = 0;
            textureViewFromSecond = 0;
            viewToSecond = 0;
            textureViewToSecond = 0;

            audioClipSO.OnValueChange.AddListener(OnClipLoad);
            keyboard = InputSystem.GetDevice<Keyboard>();
        }

        private void OnDestroy()
        {
            audioClipSO.OnValueChange.RemoveListener(OnClipLoad);
        }

        private void OnClipLoad(AudioClip clip)
        {
            this.clip = clip;
            viewFromSecond = 0;
            viewToSecond = clip.length;
            SetWave();
        }
    }
}