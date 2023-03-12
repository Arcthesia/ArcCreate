using System;
using ArcCreate.Compose.Components;
using ArcCreate.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ArcCreate.Compose.Timeline
{
    public class WaveformDisplay : MonoBehaviour
    {
        [SerializeField] private Camera editorCamera;
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private RawImage image;
        [SerializeField] private RectTransform container;
        [SerializeField] private TicksDisplay ticksDisplay;
        [SerializeField] private SliderRange slider;
        [SerializeField] private float minViewLength = 0.5f;
        [SerializeField] private float minScrollDist = 0.20f;
        [SerializeField] private float scrollDelay = 0.1f;

        private float viewFromSecond;
        private float viewToSecond;
        private float targetViewFromSecond;
        private float targetViewToSecond;
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
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, ev.position, editorCamera, out Vector2 local);
            OnWaveformDrag?.Invoke(local.x);
        }

        public void OnPointerClick(BaseEventData eventData)
        {
            var ev = eventData as PointerEventData;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, ev.position, editorCamera, out Vector2 local);
            OnWaveformDrag?.Invoke(local.x);
        }

        public void OnScroll(BaseEventData eventData)
        {
            var ev = eventData as PointerEventData;
            Vector2 scrollDelta = ev.scrollDelta;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, ev.position, editorCamera, out Vector2 local);
            float scrollPivot = Mathf.Clamp(local.x / container.rect.width, -0.5f, 0.5f) + 0.5f;
            float scrollDir = Mathf.Sign(scrollDelta.y);
            float scrollSensitivity = Settings.ScrollSensitivityTimeline.Value;

            int interation = keyboard.shiftKey.isPressed ? 5 : 1;
            for (int i = 0; i < interation; i++)
            {
                if (keyboard.ctrlKey.isPressed)
                {
                    float pivotSecond = Mathf.Lerp(targetViewFromSecond, targetViewToSecond, scrollPivot);
                    float oldViewSize = targetViewToSecond - targetViewFromSecond;
                    float viewSize = oldViewSize * (1 - (scrollDir * scrollSensitivity));
                    viewSize = Mathf.Max(viewSize, minViewLengthOfClip);

                    // x -----scrollPivot ------ pivotSecond -------- 1 - scrollPivot --------- y
                    // [<--------------------------viewSize------------------------------------>]
                    targetViewFromSecond = pivotSecond - (viewSize * scrollPivot);
                    targetViewToSecond = pivotSecond + (viewSize * (1 - scrollPivot));

                    targetViewFromSecond = Mathf.Clamp(targetViewFromSecond, 0, clip.length);
                    targetViewToSecond = Mathf.Clamp(targetViewToSecond, 0, clip.length);

                    if (targetViewFromSecond > targetViewToSecond)
                    {
                        (targetViewFromSecond, targetViewToSecond) = (targetViewToSecond, targetViewFromSecond);
                    }

                    OnWaveformZoom?.Invoke();
                }
                else
                {
                    float viewSize = targetViewToSecond - targetViewFromSecond;
                    float offset = Mathf.Max(viewSize * Mathf.Abs(scrollSensitivity), minScrollDist);
                    offset *= Mathf.Sign(scrollSensitivity);

                    targetViewFromSecond -= scrollDir * offset;
                    targetViewToSecond -= scrollDir * offset;

                    targetViewToSecond = Mathf.Clamp(targetViewToSecond, 0, clip.length);
                    targetViewFromSecond = Mathf.Clamp(targetViewFromSecond, 0, targetViewToSecond - viewSize);

                    targetViewFromSecond = Mathf.Clamp(targetViewFromSecond, 0, clip.length);
                    targetViewToSecond = Mathf.Clamp(targetViewToSecond, targetViewFromSecond + viewSize, clip.length);
                    OnWaveformScroll?.Invoke();
                }
            }

            ApplyViewRangeToWaveform();
        }

        /// <summary>
        /// Centers the waveform display to the provided time.
        /// </summary>
        /// <param name="seconds">The time value in seconds.</param>
        public void FocusOnTiming(float seconds)
        {
            float viewDistance = targetViewToSecond - targetViewFromSecond;
            float newFrom = seconds - (viewDistance / 2);
            newFrom = Mathf.Clamp(newFrom, 0, clip.length - viewDistance);
            viewFromSecond = newFrom;
            viewToSecond = viewFromSecond + viewDistance;
            targetViewFromSecond = viewFromSecond;
            targetViewToSecond = viewToSecond;

            ApplyViewRangeToWaveform();
        }

        /// <summary>
        /// Reconstruct the waveform.
        /// </summary>
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
            if (clip != null)
            {
                slider.SetValueWithoutNotify(viewFromSecond / clip.length, viewToSecond / clip.length);
            }

            ticksDisplay.UpdateTicks();
        }

        private void OnSlider(float from, float to)
        {
            if (clip == null)
            {
                return;
            }

            viewFromSecond = from * clip.length;
            viewToSecond = to * clip.length;

            viewFromSecond = Mathf.Clamp(viewFromSecond, 0, viewToSecond - minViewLengthOfClip);
            viewToSecond = Mathf.Clamp(viewToSecond, viewFromSecond + minViewLengthOfClip, clip.length);

            targetViewFromSecond = viewFromSecond;
            targetViewToSecond = viewToSecond;

            ApplyViewRangeToWaveform();
        }

        private void Awake()
        {
            viewFromSecond = 0;
            viewToSecond = 0;
            image.enabled = false;
            gameplayData.AudioClip.OnValueChange += OnClipLoad;
            keyboard = InputSystem.GetDevice<Keyboard>();
            slider.OnValueChanged += OnSlider;
        }

        private void OnDestroy()
        {
            gameplayData.AudioClip.OnValueChange -= OnClipLoad;
            slider.OnValueChanged -= OnSlider;
        }

        private void Update()
        {
            viewFromSecond += (targetViewFromSecond - viewFromSecond) * scrollDelay;
            viewToSecond += (targetViewToSecond - viewToSecond) * scrollDelay;
            ApplyViewRangeToWaveform();
        }

        private void OnClipLoad(AudioClip clip)
        {
            this.clip = clip;
            viewFromSecond = 0;
            viewToSecond = clip.length;
            targetViewFromSecond = viewFromSecond;
            targetViewToSecond = viewToSecond;
            minViewLengthOfClip = Mathf.Min(clip.length, minViewLength);
            GenerateWaveform();
        }
    }
}