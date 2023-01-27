using ArcCreate.Compose.Components;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    // Default: false / 0 / 0,0,0 / 0,0,0
    // Stationary: false / 0 / 0,9,9 / 26.565,180,0
    // Top: true / 15 / 0,9,-14 / 90,180,0
    // Top: true / 17 / -3,9,-28 / 90,270,0
    public class CameraViews : MonoBehaviour
    {
        [SerializeField] private CameraViewProperty[] views;
        [SerializeField] private Button switchViewButton;
        [SerializeField] private IconText currentViewText;

        private int index = 0;

        public void SwitchCameraStatus()
        {
            index = index + 1;
            if (index >= views.Length)
            {
                index = 0;
            }

            if (index != 0)
            {
                Services.Gameplay.Camera.IsEditorCamera = true;
                Services.Gameplay.Camera.EditorCameraPosition = views[index].Position;
                Services.Gameplay.Camera.EditorCameraRotation = views[index].Rotation;
                Services.Gameplay.Camera.GameplayCamera.orthographic = views[index].Orthographic;
                Services.Gameplay.Camera.GameplayCamera.orthographicSize = views[index].OrthographicSize;
            }
            else
            {
                Services.Gameplay.Camera.IsEditorCamera = false;
                Services.Gameplay.Camera.GameplayCamera.orthographic = false;
            }

            currentViewText.Text.text = I18n.S(views[index].Name);
            currentViewText.UpdateSize();
        }

        private void Awake()
        {
            switchViewButton.onClick.AddListener(SwitchCameraStatus);
        }

        private void OnDestroy()
        {
            switchViewButton.onClick.RemoveListener(SwitchCameraStatus);
        }
    }
}