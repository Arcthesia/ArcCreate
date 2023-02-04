using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.Compose.Popups
{
    public class PopupsService : MonoBehaviour, IPopupsService
    {
        [SerializeField] private GameObject textDialogPrefab;
        [SerializeField] private Transform textDialogParent;
        [SerializeField] private Notification notification;
        [SerializeField] private ColorPickerWindow colorPickerWindow;
        [SerializeField] private AudioClip vineboom;

        public void CreateTextDialog(string title, string content, params ButtonSetting[] buttonSettings)
        {
            GameObject go = Instantiate(textDialogPrefab, textDialogParent);
            TextDialog dialog = go.GetComponent<TextDialog>();
            dialog.Setup(title, content, buttonSettings);
        }

        public void Notify(Severity severity, string content)
        {
            notification.SetContent(severity, content);
        }

        public ColorPickerWindow OpenColorPicker(Vector2 screenPosition, Color defaultColor)
        {
            colorPickerWindow.OpenAt(screenPosition, defaultColor);
            return colorPickerWindow;
        }

        public void PlayVineBoom()
        {
            AudioSource.PlayClipAtPoint(vineboom, Vector3.zero);
        }
    }
}