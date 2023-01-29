using UnityEngine.InputSystem;

namespace ArcCreate.Compose.Navigation
{
    /// <summary>
    /// Represent a keystroke, which is a combination of modifier keys (i.e ctrl, alt, shift, meta), and a main key.
    /// </summary>
    public struct Keystroke
    {
        public string Modifier1;
        public string Modifier2;
        public string Key;
        public bool ActuateOnRelease;
        public bool ActuateOnHold;

        public InputAction ToAction(string name)
        {
            InputAction action = new InputAction(name: name);

            if (Modifier1 != null && Modifier2 != null)
            {
                action.AddCompositeBinding("ButtonWithTwoModifiers")
                    .With("Modifier1", "<Keyboard>/left" + Modifier1)
                    .With("Modifier1", "<Keyboard>/right" + Modifier1)
                    .With("Modifier2", "<Keyboard>/left" + Modifier2)
                    .With("Modifier2", "<Keyboard>/right" + Modifier2)
                    .With("Button", Key);
            }
            else if (Modifier1 != null)
            {
                action.AddCompositeBinding("ButtonWithOneModifier")
                    .With("Modifier", "<Keyboard>/left" + Modifier1)
                    .With("Modifier", "<Keyboard>/right" + Modifier1)
                    .With("Button", Key);
            }
            else if (Modifier2 != null)
            {
                action.AddCompositeBinding("ButtonWithOneModifier")
                    .With("Modifier", "<Keyboard>/left" + Modifier2)
                    .With("Modifier", "<Keyboard>/right" + Modifier2)
                    .With("Button", Key);
            }
            else
            {
                action.AddBinding(Key);
            }

            return action;
        }
    }
}