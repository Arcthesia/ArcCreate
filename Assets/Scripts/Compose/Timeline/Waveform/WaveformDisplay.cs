using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ArcCreate.Compose.Timeline
{
    public class WaveformDisplay : MonoBehaviour
    {
        [SerializeField] private RawImage image;
        [SerializeField] private RectTransform container;
        [SerializeField] private float minViewLength = 0.5f;
        [SerializeField] private float minScrollDist = 0.20f;
        [SerializeField] private AudioClipSO audioClipSO;

        private float scrollPivot;
        private float viewFromSecond;
        private float viewToSecond;
        private float minViewLengthOfClip;
        [SerializeField] private AudioClip clip;
        private Keyboard keyboard;

        private readonly int fromSampleShaderId = Shader.PropertyToID("_FromSample");
        private readonly int toSampleShaderId = Shader.PropertyToID("_ToSample");

        public event Action<float> OnWaveformDrag;

        public event Action OnWaveformScroll;

        public event Action OnWaveformZoom;

        public int ViewFromTiming => Mathf.RoundToInt(viewFromSecond * 1000);

        public int ViewToTiming => Mathf.RoundToInt(viewToSecond * 1000);

        public void OnDrag(BaseEventData eventData)
        {
            var ev = eventData as PointerEventData;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, ev.position, null, out Vector2 local);
            OnWaveformDrag?.Invoke(local.x);
        }

        public void OnPointerClick(BaseEventData eventData)
        {
            var ev = eventData as PointerEventData;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, ev.position, null, out Vector2 local);
            OnWaveformDrag?.Invoke(local.x);
        }

        public void OnScroll(BaseEventData eventData)
        {
            var ev = eventData as PointerEventData;
            Vector2 scrollDelta = ev.scrollDelta;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, ev.position, null, out Vector2 local);
            scrollPivot = (local.x / container.rect.width) + 0.5f;
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

                OnWaveformZoom?.Invoke();
            }
            else
            {
                float viewSize = viewToSecond - viewFromSecond;
                viewFromSecond += scrollDir * Mathf.Max(viewSize / 10, minScrollDist);
                viewToSecond += scrollDir * Mathf.Max(viewSize / 10, minScrollDist);

                viewToSecond = Mathf.Clamp(viewToSecond, 0, clip.length);
                viewFromSecond = Mathf.Clamp(viewFromSecond, 0, viewToSecond - viewSize);

                viewFromSecond = Mathf.Clamp(viewFromSecond, 0, clip.length);
                viewToSecond = Mathf.Clamp(viewToSecond, viewFromSecond + viewSize, clip.length);
                OnWaveformScroll?.Invoke();
            }

            ApplyViewRangeToWaveform();
        }

        public void FocusOnTiming(float seconds)
        {
            float viewDistance = viewToSecond - viewFromSecond;
            float newFrom = seconds - (viewDistance / 2);
            newFrom = Mathf.Clamp(newFrom, 0, clip.length - viewDistance);
            viewFromSecond = newFrom;
            viewToSecond = viewFromSecond + viewDistance;

            ApplyViewRangeToWaveform();
        }

        public void GenerateWaveform()
        {
            viewFromSecond = 0;
            viewToSecond = clip.length;
            Texture2D texture = WaveformGenerator.EncodeTexture(clip);
            image.texture = texture;
            image.material.mainTexture = texture;
            image.enabled = true;

            ApplyViewRangeToWaveform();
        }

        private void ApplyViewRangeToWaveform()
        {
            image.material.SetInt(fromSampleShaderId, WaveformGenerator.SecondToSample(viewFromSecond, clip));
            image.material.SetInt(toSampleShaderId, WaveformGenerator.SecondToSample(viewToSecond, clip));
        }

        private void Awake()
        {
            viewFromSecond = 0;
            viewToSecond = 0;
            image.enabled = false;
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
            minViewLengthOfClip = Mathf.Min(clip.length, minViewLength);
            GenerateWaveform();
        }
    }
}