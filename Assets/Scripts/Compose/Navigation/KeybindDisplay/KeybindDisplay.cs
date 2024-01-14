using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ArcCreate.Compose.Navigation
{
    public class KeybindDisplay : MonoBehaviour
    {
        private const float TotalPadding = 22;
        private readonly List<KeystrokeDisplay> keystrokes = new List<KeystrokeDisplay>();

        [SerializeField] private Transform keyHint;
        [SerializeField] private Pool<KeystrokeDisplay> keystrokePool;
        [SerializeField] private RectTransform actionTextRect;
        [SerializeField] private TMP_Text actionText;
        [SerializeField] private RectTransform rectTransform;

        public float Size { get; set; }

        public void SetPosition(float x)
        {
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
        }

        public void SetKeybind(Keybind keybind)
        {
            for (int i = 0; i < keystrokes.Count; i++)
            {
                keystrokePool.Return(keystrokes[i]);
            }

            keystrokes.Clear();
            for (int i = 0; i < keybind.Keystrokes.Length; i++)
            {
            }

            float totalLength = 0;
            for (int i = 0; i < keybind.Keystrokes.Length; i++)
            {
                Keystroke keystroke = keybind.Keystrokes[i];
                KeystrokeDisplay display = keystrokePool.Get(keyHint);
                display.SetKeystroke(keystroke);
                display.SetPosition(totalLength);
                totalLength += display.Size;
                keystrokes.Add(display);
            }

            actionText.text = keybind.Action.I18nName;
            Size = totalLength + actionText.preferredWidth + TotalPadding;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Size);
        }

        private void Awake()
        {
            keystrokePool = Pools.Get<KeystrokeDisplay>(Values.KeystrokeDisplayPool);
        }
    }
}