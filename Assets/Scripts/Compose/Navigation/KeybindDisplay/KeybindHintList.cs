using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Navigation
{
    public class KeybindHintList : MonoBehaviour, IScrollHandler
    {
        private const float PaddingLeft = 5;
        private readonly List<KeybindDisplay> keybindDisplays = new List<KeybindDisplay>();

        [SerializeField] private Pool<KeybindDisplay> keybindPool;
        [SerializeField] private GameObject keybindPrefab;
        [SerializeField] private GameObject keystrokePrefab;
        [SerializeField] private GameObject hintDisplay;
        [SerializeField] private RectTransform mainDisplayRect;
        [SerializeField] private float mainDisplayOffset;
        [SerializeField] private float baseScrollSensitivity = 1;
        private RectTransform rect;
        private bool queueRebuild = false;
        private float offset = 0;

        private bool enableDisplay = true;

        public bool EnableDisplay
        {
            get => enableDisplay;
            set
            {
                hintDisplay.SetActive(Settings.EnableKeybindHintDisplay.Value && value);
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

            offset = 0;
            RebuildListCore();
        }

        public void OnScroll(PointerEventData eventData)
        {
            Vector2 scroll = eventData.scrollDelta;
            float delta = Math.Abs(scroll.x) > Math.Abs(scroll.y) ? scroll.x : scroll.y;
            offset += delta * Settings.ScrollSensitivityHorizontal.Value * baseScrollSensitivity;
            RebuildListCore();
        }

        private void RebuildListCore()
        {
            List<Keybind> keybinds = Services.Navigation.GetKeybindsToDisplay();
            keybindPool.ReturnAll();
            keybindDisplays.Clear();

            float maxLength = rect.rect.width;

            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            offset = Mathf.Min(0, offset);
            float leftOffset = offset + PaddingLeft;
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
                leftOffset += display.Size;
                if (leftOffset < 0)
                {
                    keybindPool.Return(display);
                    continue;
                }

                display.SetPosition(leftOffset - display.Size);
                keybindDisplays.Add(display);
                if (leftOffset > maxLength)
                {
                    return;
                }
            }

            if (offset < 0 && leftOffset < maxLength)
            {
                float lengthLeft = maxLength - leftOffset;
                offset += lengthLeft;
                foreach (var keybind in keybindDisplays)
                {
                    keybind.SetPosition(keybind.Position + lengthLeft);
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
            rect = GetComponent<RectTransform>();
            Pools.New<KeystrokeDisplay>(Values.KeystrokeDisplayPool, keystrokePrefab, transform, 32);
            keybindPool = Pools.New<KeybindDisplay>(Values.KeybindDisplayPool, keybindPrefab, transform, 32);
            Settings.EnableKeybindHintDisplay.OnValueChanged.AddListener(OnDisplaySettings);
            OnDisplaySettings(Settings.EnableKeybindHintDisplay.Value);
        }

        private void OnDestroy()
        {
            Settings.EnableKeybindHintDisplay.OnValueChanged.RemoveListener(OnDisplaySettings);
        }

        private void OnDisplaySettings(bool shouldDisplay)
        {
            gameObject.SetActive(shouldDisplay);
            mainDisplayRect.offsetMin = new Vector2(0, mainDisplayOffset + (shouldDisplay ? rect.rect.height : 0));
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