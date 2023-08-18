using ArcCreate.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class ModifierDisplay : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Toggle autoToggle;
        [SerializeField] private Toggle practiceToggle;
        [SerializeField] private Toggle mirrorToggle;
        [SerializeField] private GameObject autoPreview;
        [SerializeField] private GameObject practicePreview;
        [SerializeField] private GameObject mirrorPreview;
        [SerializeField] private GameObject modifierListParent;

        private void Awake()
        {
            gameplayData.EnablePracticeMode.Value = false;
            gameplayData.EnableAutoplayMode.Value = false;

            autoToggle.onValueChanged.AddListener(OnAutoToggle);
            practiceToggle.onValueChanged.AddListener(OnPracticeToggle);
            mirrorToggle.onValueChanged.AddListener(OnMirrorToggle);

            autoToggle.SetIsOnWithoutNotify(false);
            practiceToggle.SetIsOnWithoutNotify(false);
            mirrorToggle.SetIsOnWithoutNotify(Settings.MirrorNotes.Value);

            autoPreview.SetActive(false);
            practicePreview.SetActive(false);
            mirrorPreview.SetActive(Settings.MirrorNotes.Value);
            UpdateModifierParent();
        }

        private void OnDestroy()
        {
            autoToggle.onValueChanged.RemoveListener(OnAutoToggle);
            practiceToggle.onValueChanged.RemoveListener(OnPracticeToggle);
            mirrorToggle.onValueChanged.RemoveListener(OnMirrorToggle);
        }

        private void OnAutoToggle(bool val)
        {
            gameplayData.EnableAutoplayMode.Value = val;
            autoPreview.SetActive(val);
            UpdateModifierParent();
        }

        private void OnPracticeToggle(bool val)
        {
            gameplayData.EnablePracticeMode.Value = val;
            practicePreview.SetActive(val);
            UpdateModifierParent();
        }

        private void OnMirrorToggle(bool val)
        {
            Settings.MirrorNotes.Value = val;
            mirrorPreview.SetActive(val);
            UpdateModifierParent();
        }

        private void UpdateModifierParent()
        {
            modifierListParent.SetActive(autoPreview.activeSelf || practicePreview.activeSelf || mirrorPreview.activeSelf);
        }
    }
}