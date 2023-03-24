using UnityEngine;

namespace ArcCreate.Utility
{
    public class FpsCounterMenu : MonoBehaviour
    {
        private void Awake()
        {
            Settings.ShowFPSCounter.OnValueChanged.AddListener(OnSettings);
            OnSettings(Settings.ShowFPSCounter.Value);
        }

        private void OnDestroy()
        {
            Settings.ShowFPSCounter.OnValueChanged.RemoveListener(OnSettings);
        }

        private void OnSettings(bool on)
        {
            gameObject.SetActive(on);
            Debug.Log($"Fps counter: {on}");
        }
    }
}