using TMPro;
using UnityEngine;

namespace ArcCreate.Compose.Popups
{
    public class TextDialog : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private Transform buttonParent;

        public void Setup(string title, string content, ButtonSetting[] buttonSettings)
        {
            titleText.text = title;
            contentText.text = content;

            foreach (ButtonSetting setting in buttonSettings)
            {
                GameObject go = Instantiate(buttonPrefab, buttonParent);
                TextDialogButton button = go.GetComponent<TextDialogButton>();
                button.Setup(setting.Text, setting.Callback, this);
            }
        }

        public void CloseSelf()
        {
            Destroy(gameObject);
        }
    }
}