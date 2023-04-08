using UnityEngine;

namespace ArcCreate.Compose.Navigation
{
    /// <summary>
    /// Represent a keystroke, which is a combination of modifier keys (i.e ctrl, alt, shift, meta), and a main key.
    /// </summary>
    public struct Keystroke
    {
        public KeyCode[] Modifiers;
        public KeyCode Key;
        public bool ActuateOnRelease;
        public bool ActuateOnHold;

        public Keystroke(KeyCode key, bool actuateOnHold, bool actuateOnRelease)
        {
            Modifiers = new KeyCode[0];
            Key = key;
            ActuateOnHold = actuateOnHold;
            ActuateOnRelease = actuateOnRelease;
        }

        public Keystroke(KeyCode[] modifiers, KeyCode key, bool actuateOnHold, bool actuateOnRelease)
        {
            Modifiers = modifiers;
            Key = key;
            ActuateOnHold = actuateOnHold;
            ActuateOnRelease = actuateOnRelease;
        }

        public bool WasExecuted
        {
            get
            {
                foreach (var mod in Modifiers)
                {
                    if (!Input.GetKey(mod))
                    {
                        return false;
                    }
                }

                if (ActuateOnHold)
                {
                    return Input.GetKey(Key);
                }
                else if (ActuateOnRelease)
                {
                    return Input.GetKeyUp(Key);
                }
                else
                {
                    return Input.GetKeyDown(Key);
                }
            }
        }
    }
}