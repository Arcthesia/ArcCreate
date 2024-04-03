using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    public class CameraViews : MonoBehaviour
    {
        [SerializeField] private Button togglePickerButton;
        [SerializeField] private GameObject picker;
        [SerializeField] private Button defaultViewButton;
        [SerializeField] private CameraViewProperty[] views;
        [SerializeField] private TMP_Text currentViewText;
        [SerializeField] private GameObject viewRowPrefab;
        [SerializeField] private Transform viewRowParent;

        internal void Select(CameraViewProperty data)
        {
            Services.Gameplay.Camera.IsEditorCamera = true;
            Services.Gameplay.Camera.EditorCameraPosition = data.Position;
            Services.Gameplay.Camera.EditorCameraRotation = data.Rotation;
            Services.Gameplay.Camera.IsOrthographic = data.Orthographic;
            Services.Gameplay.Camera.OrthographicSize = data.OrthographicSize;
            currentViewText.text = I18n.S(data.Name);
        }

        private void Awake()
        {
            defaultViewButton.onClick.AddListener(SetDefaultView);
            togglePickerButton.onClick.AddListener(TogglePicker);

            foreach (var view in views)
            {
                GameObject go = Instantiate(viewRowPrefab, viewRowParent);
                CameraViewRow newRow = go.GetComponent<CameraViewRow>();
                newRow.SetData(this, view);
            }
        }

        private void OnDestroy()
        {
            defaultViewButton.onClick.RemoveListener(SetDefaultView);
            togglePickerButton.onClick.RemoveListener(TogglePicker);
        }

        private void TogglePicker()
        {
            picker.SetActive(!picker.activeSelf);
        }

        private void SetDefaultView()
        {
            Services.Gameplay.Camera.IsEditorCamera = false;
            Services.Gameplay.Camera.IsOrthographic = false;
            currentViewText.text = I18n.S("Compose.Dialog.CameraViews.Default");
        }
    }
}