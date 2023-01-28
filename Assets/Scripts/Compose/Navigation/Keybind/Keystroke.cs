using UnityEngine.InputSystem;

namespace ArcCreate.Compose.Navigation
{
    /// <summary>
    /// Represent a keystroke, which is a combination of modifier keys (i.e ctrl, alt, shift, meta), and a main key.
    /// </summary>
    public struct Keystroke
    {
        public bool CtrlKey;

        public bool AltKey;

        public bool MetaKey;

        public bool ShiftKey;

        public Key Key;

        // TODO: add more activation method like scroll, click etc.
        public bool Check(Keyboard keyboard)
        {
            if (CtrlKey && !keyboard.ctrlKey.isPressed)
            {
                return false;
            }

            if (AltKey && !keyboard.altKey.isPressed)
            {
                return false;
            }

            if (MetaKey && !(keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed))
            {
                return false;
            }

            if (ShiftKey && !keyboard.shiftKey.isPressed)
            {
                return false;
            }

            return keyboard[Key].isPressed;
        }
    }
}