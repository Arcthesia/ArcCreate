using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class StarRatingDialog : MonoBehaviour
    {
        [SerializeField] private StorageData storageData;
        [SerializeField] private Button[] buttons;
        [SerializeField] private StarRating display;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private ScriptedAnimator animator;
        private PlayHistory playHistory;

        public void Show()
        {
            if (!animator.IsShown)
            {
                animator.Show();
            }
        }

        public void Hide()
        {
            animator.Hide();
        }

        public void Attach(PlayHistory playHistory)
        {
            this.playHistory = playHistory;
            display.Value = playHistory.Rating;
        }

        private void Awake()
        {
            if (buttons == null)
            {
                return;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                int j = i;
                buttons[j].onClick.AddListener(() => display.Value = j + 1);
            }

            confirmButton.onClick.AddListener(OnConfirm);
            cancelButton.onClick.AddListener(OnCancel);
        }

        private void OnConfirm()
        {
            playHistory.Rating = display.Value;
            playHistory.Save();
            storageData.NotifyStorageChange();
            Hide();
        }

        private void OnCancel()
        {
            display.Value = playHistory.Rating;
            Hide();
        }

        private void OnDestroy()
        {
            if (buttons == null)
            {
                return;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].onClick.RemoveAllListeners();
            }

            confirmButton.onClick.RemoveListener(OnConfirm);
            cancelButton.onClick.RemoveListener(OnCancel);
        }
    }
}