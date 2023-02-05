using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Component for handling resizing the viewport.
    /// </summary>
    public class GameplayViewport : MonoBehaviour
    {
        [SerializeField] private Camera editorCamera;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;
        [SerializeField] private float[] aspectRatios;
        private readonly Vector3[] corners = new Vector3[4];

        private void Update()
        {
            if (Services.Gameplay == null)
            {
                return;
            }

            viewport.GetWorldCorners(corners);

            Vector3 bottomLeft = editorCamera.WorldToScreenPoint(corners[0]);
            Vector3 topRight = editorCamera.WorldToScreenPoint(corners[2]);

            float x = bottomLeft.x;
            float y = bottomLeft.y;
            float width = topRight.x - bottomLeft.x;
            float height = topRight.y - bottomLeft.y;

            Rect normalized = new Rect(
                x / Screen.width,
                y / Screen.height,
                width / Screen.width,
                height / Screen.height);

            Services.Gameplay.SetCameraViewportRect(normalized);
        }

        private void OnEnable()
        {
            if (Services.Gameplay == null)
            {
                return;
            }

            Camera cam = Services.Gameplay.Camera.GameplayCamera;
            if (cam != null)
            {
                cam.enabled = false;
            }
        }

        private void OnDisable()
        {
            if (Services.Gameplay == null)
            {
                return;
            }

            Camera cam = Services.Gameplay.Camera.GameplayCamera;
            if (cam != null)
            {
                cam.enabled = false;
            }
        }

        private void Awake()
        {
            Settings.ViewportAspectRatioSetting.OnValueChanged.AddListener(OnAspectRatioSetting);
            OnAspectRatioSetting(Settings.ViewportAspectRatioSetting.Value);
        }

        private void OnDestroy()
        {
            Settings.ViewportAspectRatioSetting.OnValueChanged.RemoveListener(OnAspectRatioSetting);
        }

        private void OnAspectRatioSetting(int option)
        {
            option = Mathf.Min(option, aspectRatios.Length - 1);
            float ratio = aspectRatios[option];
            aspectRatioFitter.aspectRatio = ratio;
        }
    }
}