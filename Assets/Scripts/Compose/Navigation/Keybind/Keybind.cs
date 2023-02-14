using Cysharp.Threading.Tasks;
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
        private bool isFinalKeyHeld = false;

        public Keybind(Keystroke[] keystrokes, IAction action)
        {
            InputActions = new InputAction[keystrokes.Length];
            Keystrokes = keystrokes;

            Action = action;

            for (int i = 0; i < keystrokes.Length; i++)
            {
                Keystroke keystroke = keystrokes[i];
                InputAction inputAction = keystroke.ToAction(action.Id);
                InputActions[i] = inputAction;

                if (i == keystrokes.Length - 1 && keystroke.ActuateOnHold)
                {
                    inputAction.performed += OnFinalKeystrokeDown;
                    inputAction.canceled += OnFinalKeystrokeUp;
                }
                else if (keystroke.ActuateOnRelease)
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

        public Keystroke[] Keystrokes { get; private set; }

        public void Destroy()
        {
            for (int i = 0; i < Keystrokes.Length; i++)
            {
                Keystroke keystroke = Keystrokes[i];
                InputAction inputAction = InputActions[i];

                if (i == Keystrokes.Length - 1 && keystroke.ActuateOnHold)
                {
                    inputAction.performed -= OnFinalKeystrokeDown;
                    inputAction.canceled -= OnFinalKeystrokeUp;
                }
                else if (keystroke.ActuateOnRelease)
                {
                    inputAction.canceled -= OnKeystroke;
                }
                else
                {
                    inputAction.performed -= OnKeystroke;
                }

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
                if (BlockUnwantedModifier(index))
                {
                    return;
                }

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

        private void OnFinalKeystrokeDown(InputAction.CallbackContext obj)
        {
            if (index == InputActions.Length - 1)
            {
                if (BlockUnwantedModifier(index))
                {
                    return;
                }

                index = 0;
                isFinalKeyHeld = true;
                if (Services.Navigation.ShouldExecute(Action))
                {
                    StartRepeatedExecution().Forget();
                }
            }
            else
            {
                index = 0;
            }
        }

        private void OnFinalKeystrokeUp(InputAction.CallbackContext obj)
        {
            isFinalKeyHeld = false;
        }

        private bool BlockUnwantedModifier(int index)
        {
            Keyboard keyboard = Keyboard.current;
            Keystroke keystroke = Keystrokes[index];
            return
                string.IsNullOrEmpty(keystroke.Modifier1) && string.IsNullOrEmpty(keystroke.Modifier2) &&
                (keyboard.ctrlKey.isPressed || keyboard.shiftKey.isPressed || keyboard.altKey.isPressed);
        }

        private async UniTask StartRepeatedExecution()
        {
            while (isFinalKeyHeld)
            {
                Action.Execute();
                await UniTask.NextFrame();
            }
        }
    }
}