using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ArcCreate.Compose.Navigation
{
    /// <summary>
    /// Utility class for converting between string and keybind.
    /// </summary>
    public static partial class KeybindUtils
    {
        /// <summary>
        /// Try generating a keybind from a string.
        /// See: "https://github.com/lydell/vim-like-key-notation" for in-depth explanation of the format.
        /// </summary>
        /// <param name="keybindString">The string to parse.</param>
        /// <param name="action">The action to bind to the keybind.</param>
        /// <param name="keybind">The output keybind.</param>
        /// <param name="reason">The reason for failing to parse the string. This argument will be null if the string is valid.</param>
        /// <returns>Whether or not parsing succeeded.</returns>
        public static bool TryParseKeybind(string keybindString, IAction action, out Keybind keybind, out string reason)
        {
            MatchCollection matches = new Regex(@"<[^<>\s]+>|[\s\S]|^$").Matches(keybindString);

            Keystroke[] keystrokes = new Keystroke[matches.Count];

            for (int i = 0; i < matches.Count; i++)
            {
                string keystrokeString = matches[i].Value;

                if (!TryParseKeystroke(keystrokeString, out Keystroke keystroke, out string actionReason))
                {
                    keybind = null;
                    reason = actionReason;
                    reason = I18n.S("Compose.Exception.Navigation.ParseKeybind", reason);
                    return false;
                }
                else
                {
                    keystrokes[i] = keystroke;
                }
            }

            keybind = new Keybind(keystrokes, action);
            reason = null;
            return true;
        }

        /// <summary>
        /// Try generating a keystroke from a string.
        /// See: "https://github.com/lydell/vim-like-key-notation" for in-depth explanation of the format.
        /// </summary>
        /// <param name="keystrokeString">The string to parse.</param>
        /// <param name="keystroke">The output keystroke.</param>
        /// <param name="reason">The reason for failing to parse the string. This argument will be null if the string is valid.</param>
        /// <returns>Whether or not parsing succeeded.</returns>
        public static bool TryParseKeystroke(string keystrokeString, out Keystroke keystroke, out string reason)
        {
            if (keystrokeString.Length == 1)
            {
                Regex whitespaceRegex = new Regex(@"\s");
                if (whitespaceRegex.IsMatch(keystrokeString))
                {
                    keystroke = default;
                    reason = I18n.S("Compose.Exception.Navigation.ParseWhitespace");
                    return false;
                }

                if (!TryGetKeyCode(keystrokeString, out KeyCode k))
                {
                    keystroke = default;
                    reason = I18n.S("Compose.Exception.Navigation.InvalidKey", keystrokeString);
                    return false;
                }

                bool isShift = IsShiftedKey.Contains(keystrokeString) || char.IsUpper(keystrokeString[0]);

                keystroke = isShift ? new Keystroke(new KeyCode[] { KeyCode.LeftShift }, k, false, false) : new Keystroke(k, false, false);
                reason = null;
                return true;
            }

            Regex capture = new Regex(@"^<((?:[a-z]-)*)([a-z\d]+|[^<>\s])>$", RegexOptions.IgnoreCase);
            MatchCollection matches = capture.Matches(keystrokeString);
            if (matches.Count != 1 || matches[0].Groups.Count != 3)
            {
                keystroke = default;
                reason = I18n.S("Compose.Exception.Navigation.InvalidKeystroke", keystrokeString);
                return false;
            }

            string modifiersString = matches[0].Groups[1].Value.ToLower();
            string key = matches[0].Groups[2].Value;
            string keyLower = key.ToLower();

            if (TryGetKeyCode(keyLower, out KeyCode keycode))
            {
                keystroke = new Keystroke(keycode, false, false);
            }
            else
            {
                keystroke = default;
                reason = I18n.S("Compose.Exception.Navigation.InvalidKey", key);
                return false;
            }

            HashSet<KeyCode> modifierKeyCodes = new HashSet<KeyCode>();

            if (IsShiftedKey.Contains(key) || (key.Length == 1 && char.IsUpper(key[0])))
            {
                modifierKeyCodes.Add(KeyCode.LeftShift);
            }

            string[] modifiers = modifiersString.Split('-');
            for (int i = 0; i < modifiers.Length - 1; i++)
            {
                string mod = modifiers[i].ToLower();

                bool duplicateException = false;
                if (mod == "u")
                {
                    if (keystroke.ActuateOnRelease)
                    {
                        duplicateException = true;
                    }

                    keystroke.ActuateOnRelease = true;
                }
                else if (mod == "h")
                {
                    if (keystroke.ActuateOnHold)
                    {
                        duplicateException = true;
                    }

                    keystroke.ActuateOnHold = true;
                }
                else
                {
                    if (!ModMap.ContainsKey(mod))
                    {
                        keystroke = default;
                        reason = I18n.S("Compose.Exception.Navigation.InvalidModifier", mod);
                        return false;
                    }

                    KeyCode mappedMod = ModMap[mod];
                    if (modifierKeyCodes.Contains(mappedMod))
                    {
                        duplicateException = true;
                    }

                    modifierKeyCodes.Add(mappedMod);
                }

                if (duplicateException)
                {
                    keystroke = default;
                    reason = I18n.S("Compose.Exception.Navigation.DuplicateModifier", mod);
                    return false;
                }
            }

            keystroke.Modifiers = modifierKeyCodes.ToArray();
            reason = null;
            return true;
        }
    }
}