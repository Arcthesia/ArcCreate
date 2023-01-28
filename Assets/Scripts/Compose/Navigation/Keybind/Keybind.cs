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
        private int index = 0;

        public Keybind(Keystroke[] keystrokes, IAction action)
        {
            InputActions = new InputAction[keystrokes.Length];
            Action = action;

            for (int i = 0; i < keystrokes.Length; i++)
            {
                Keystroke keystroke = keystrokes[i];
                InputAction inputAction = keystroke.ToAction(action.Id);
                InputActions[i] = inputAction;

                if (keystroke.ActuateOnRelease)
                {
                    inputAction.canceled += OnKeystroke;
                }
                else
                {
                    inputAction.performed += OnKeystroke;
                }

                inputAction.Enable();
            }
        }

        public IAction Action { get; set; }

        public InputAction[] InputActions { get; private set; }

        public void Destroy()
        {
            foreach (InputAction inputAction in InputActions)
            {
                inputAction.Disable();
            }
        }

        public void Reset()
        {
            index = 0;
        }

        private void OnKeystroke(InputAction.CallbackContext obj)
        {
            if (ReferenceEquals(obj.action, InputActions[index]))
            {
                index++;
                if (index >= InputActions.Length)
                {
                    index = 0;
                    if (Services.Navigation.ShouldExecute(Action))
                    {
                        Action.Execute();
                    }
                }
            }
            else
            {
                index = 0;
            }
        }
    }
}