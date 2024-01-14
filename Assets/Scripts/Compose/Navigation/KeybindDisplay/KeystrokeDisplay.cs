using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Navigation
{
    public class KeystrokeDisplay : MonoBehaviour
    {
        private const float TotalPadding = 10;
        private const float MouseIconWidth = 10;
        private readonly char[] chars = new char[32];
        [SerializeField] private Image mouseHint;
        [SerializeField] private TMP_Text keyText;
        [SerializeField] private KeystrokeSprites sprites;
        [SerializeField] private RectTransform rectTransform;

        public float Size { get; private set; }

        public void SetPosition(float x)
        {
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
        }

        public void SetKeystroke(Keystroke keystroke)
        {
            bool ctrl = false;
            bool shift = false;
            bool alt = false;
            for (int i = 0; i < keystroke.Modifiers.Length; i++)
            {
                KeyCode modifier = keystroke.Modifiers[i];
                ctrl |= modifier == KeyCode.LeftControl || modifier == KeyCode.RightControl;
                shift |= modifier == KeyCode.LeftShift || modifier == KeyCode.RightShift;
                alt |= modifier == KeyCode.LeftAlt || modifier == KeyCode.RightAlt;
            }

            int c = 0;
            bool addPlus = false;
            if (ctrl)
            {
                c = AddChars("Ctrl", c);
                addPlus = true;
            }

            if (shift)
            {
                if (addPlus)
                {
                    chars[c++] = '+';
                }

                c = AddChars("Shift", c);
                addPlus = true;
            }

            if (alt)
            {
                if (addPlus)
                {
                    chars[c++] = '+';
                }

                c = AddChars("Alt", c);
                addPlus = true;
            }

            if (addPlus)
            {
                chars[c++] = '+';
            }

            switch (keystroke.Key)
            {
                case KeyCode.Mouse0:

                    mouseHint.sprite = sprites.LeftMouseSprite;
                    break;
                case KeyCode.Mouse1:
                    mouseHint.sprite = sprites.RightMouseSprite;
                    break;
                case KeyCode.Mouse2:
                    mouseHint.sprite = sprites.MidMouseSprite;
                    break;
                default:
                    c = AddChars(KeyToString(keystroke.Key), c);
                    break;
            }

            bool isMouse = keystroke.Key == KeyCode.Mouse0
                        || keystroke.Key == KeyCode.Mouse1
                        || keystroke.Key == KeyCode.Mouse2;

            keyText.SetCharArray(chars, 0, c);
            mouseHint.gameObject.SetActive(isMouse);
            Size = keyText.preferredWidth + TotalPadding + (isMouse ? MouseIconWidth : 0);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Size);
        }

        private int AddChars(string s, int start)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (start + i >= chars.Length)
                {
                    return start + i;
                }

                chars[start + i] = s[i];
            }

            return start + s.Length;
        }

        private string KeyToString(KeyCode key)
        {
            return KeybindUtils.GetString(key);
        }
    }
}