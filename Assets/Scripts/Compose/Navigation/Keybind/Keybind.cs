using UnityEngine.InputSystem;

namespace ArcCreate.Compose.Navigation
{
    public enum KeybindResponse
    {
        /// <summary>
        /// Input keystroke has invalidated this keybind.
        /// </summary>
        Invalid,

        /// <summary>
        /// Input keystroke is part of the keybind.
        /// </summary>
        Incomplete,

        /// <summary>
        /// Input keystroke completes the keybind.
        /// </summary>
        Complete,
    }

    /// <summary>
    /// Data class representing keybind, which is a series of <see cref="Keystroke"/>.
    /// </summary>
    public class Keybind
    {
        private int currentKeystrokeIndex = 0;

        public Keybind(Keystroke[] keystrokes)
        {
            Keystrokes = keystrokes;
        }

        public IAction Action { get; set; }

        public Keystroke[] Keystrokes { get; private set; }

        public KeybindResponse CheckKeystroke(Keyboard keyboard)
        {
            Keystroke keystroke = Keystrokes[currentKeystrokeIndex];
            bool keystrokeHit = keystroke.Check(keyboard);

            if (keystrokeHit)
            {
                currentKeystrokeIndex += 1;
                if (currentKeystrokeIndex > Keystrokes.Length)
                {
                    ResetState();
                    return KeybindResponse.Complete;
                }
                else
                {
                    return KeybindResponse.Incomplete;
                }
            }
            else
            {
                ResetState();
                return KeybindResponse.Invalid;
            }
        }

        public void ResetState()
        {
            currentKeystrokeIndex = 0;
        }
    }
}