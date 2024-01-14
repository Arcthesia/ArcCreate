using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Compose.Navigation
{
    public class KeybindHintList : MonoBehaviour
    {
        private const float PaddingLeft = 5;
        private readonly List<KeybindDisplay> keybindDisplays = new List<KeybindDisplay>();

        [SerializeField] private Pool<KeybindDisplay> keybindPool;
        [SerializeField] private GameObject keybindPrefab;
        [SerializeField] private GameObject keystrokePrefab;
        private RectTransform rect;
        private bool queueRebuild = false;

        private bool enableDisplay = true;

        public bool EnableDisplay
        {
            get => enableDisplay;
            set
            {
                gameObject.SetActive(Settings.EnableKeybindHintDisplay.Value && value);
                enableDisplay = value;
            }
        }

        public void RebuildList()
        {
            if (!gameObject.activeInHierarchy)
            {
                queueRebuild = true;
                return;
            }

            List<Keybind> keybinds = Services.Navigation.GetKeybindsToDisplay();
            keybindPool.ReturnAll();
            keybindDisplays.Clear();

            float totalLength = 0;
            float maxLength = rect.rect.width;

            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            for (int i = 0; i < keybinds.Count; i++)
            {
                Keybind keybind = keybinds[i];
                if ((shift && !IsModifier(keybind, KeyCode.LeftShift, KeyCode.RightShift))
                 || (alt && !IsModifier(keybind, KeyCode.LeftAlt, KeyCode.RightAlt))
                 || (ctrl && !IsModifier(keybind, KeyCode.LeftControl, KeyCode.RightControl)))
                {
                    continue;
                }

                KeybindDisplay display = keybindPool.Get();
                display.SetKeybind(keybind);
                display.SetPosition(totalLength + PaddingLeft);
                keybindDisplays.Add(display);
                totalLength += display.Size;
                if (totalLength > maxLength)
                {
                    return;
                }
            }
        }

        private void OnEnable()
        {
            if (queueRebuild)
            {
                RebuildList();
            }
        }

        private void Awake()
        {
            Pools.New<KeystrokeDisplay>(Values.KeystrokeDisplayPool, keystrokePrefab, transform, 32);
            keybindPool = Pools.New<KeybindDisplay>(Values.KeybindDisplayPool, keybindPrefab, transform, 32);
            Settings.EnableKeybindHintDisplay.OnValueChanged.AddListener(OnDisplaySettings);
            OnDisplaySettings(Settings.EnableKeybindHintDisplay.Value);
            rect = GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            Settings.EnableKeybindHintDisplay.OnValueChanged.RemoveListener(OnDisplaySettings);
        }

        private void OnDisplaySettings(bool shouldDisplay)
        {
            gameObject.SetActive(shouldDisplay);
        }

        private bool IsModifier(Keybind keybind, KeyCode leftMod, KeyCode rightMod)
        {
            if (keybind.Keystrokes.Length == 0)
            {
                return false;
            }

            foreach (var keyCode in keybind.Keystrokes[0].Modifiers)
            {
                if (keyCode == leftMod || keyCode == rightMod)
                {
                    return true;
                }
            }

            return false;
        }
    }
}