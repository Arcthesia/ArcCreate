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
            Keystrokes = keystrokes;
            Action = action;
        }

        public IAction Action { get; set; }

        public Keystroke[] Keystrokes { get; private set; }

        public void Reset()
        {
            index = 0;
        }

        public KeybindState CheckKeybind()
        {
            Keystroke current = Keystrokes[index];
            if (current.WasExecuted)
            {
                if (index == Keystrokes.Length - 1)
                {
                    Reset();
                    return KeybindState.Complete;
                }
                else
                {
                    index += 1;
                    return KeybindState.InProgress;
                }
            }

            Reset();
            return KeybindState.Invalid;
        }

        public void Execute()
        {
            if (Services.Navigation.ShouldExecute(Action))
            {
                Action.Execute();
            }
        }
    }
}