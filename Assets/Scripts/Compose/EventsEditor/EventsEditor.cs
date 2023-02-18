using ArcCreate.Gameplay.Chart;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    public class EventsEditor : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Color highlightColor;
        [SerializeField] private Color normalColor;
        [SerializeField] private Button showTimingButton;
        [SerializeField] private Image showTimingImage;
        [SerializeField] private GameObject timingSection;
        [SerializeField] private Button showCameraButton;
        [SerializeField] private Image showCameraImage;
        [SerializeField] private GameObject cameraSection;
        [SerializeField] private Button showScenecontrolButton;
        [SerializeField] private Image showScenecontrolImage;
        [SerializeField] private GameObject scenecontrolSection;

        private void Awake()
        {
            showTimingButton.onClick.AddListener(OnShowTiming);
            showCameraButton.onClick.AddListener(OnShowCamera);
            showScenecontrolButton.onClick.AddListener(OnShowScenecontrol);
            Values.EditingTimingGroup.OnValueChange += OnTimingGroup;

            OnShowTiming();
        }

        private void OnTimingGroup(int tg)
        {
            TimingGroup group = Services.Gameplay.Chart.GetTimingGroup(tg);
            canvasGroup.interactable = group.GroupProperties.Editable;
        }

        private void OnDestroy()
        {
            showTimingButton.onClick.RemoveListener(OnShowTiming);
            showCameraButton.onClick.RemoveListener(OnShowCamera);
            showScenecontrolButton.onClick.RemoveListener(OnShowScenecontrol);

            Values.EditingTimingGroup.OnValueChange -= OnTimingGroup;
        }

        private void OnShowTiming()
        {
            timingSection.SetActive(true);
            cameraSection.SetActive(false);
            scenecontrolSection.SetActive(false);

            showTimingImage.color = highlightColor;
            showCameraImage.color = normalColor;
            showScenecontrolImage.color = normalColor;
        }

        private void OnShowCamera()
        {
            timingSection.SetActive(false);
            cameraSection.SetActive(true);
            scenecontrolSection.SetActive(false);

            showTimingImage.color = normalColor;
            showCameraImage.color = highlightColor;
            showScenecontrolImage.color = normalColor;
        }

        private void OnShowScenecontrol()
        {
            timingSection.SetActive(false);
            cameraSection.SetActive(false);
            scenecontrolSection.SetActive(true);

            showTimingImage.color = normalColor;
            showCameraImage.color = normalColor;
            showScenecontrolImage.color = highlightColor;
        }
    }
}