using System.Collections.Generic;
using System.Text.RegularExpressions;

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

                if (!KeyAlias.TryGetValue(keystrokeString.ToLower(), out var k))
                {
                    keystroke = default;
                    reason = I18n.S("Compose.Exception.Navigation.InvalidKey", keystrokeString);
                    return false;
                }

                keystroke = new Keystroke()
                {
                    Modifier1 = null,
                    Modifier2 = null,
                    ActuateOnRelease = false,
                    Key = "<Keyboard>/" + k.ToString(),
                };

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

            if (!KeyAlias.ContainsKey(keyLower))
            {
                keystroke = default;
                reason = I18n.S("Compose.Exception.Navigation.InvalidKey", key);
                return false;
            }

            keystroke = new Keystroke()
            {
                Key = "<Keyboard>/" + KeyAlias[keyLower].ToString(),
            };

            HashSet<string> mappedMods = new HashSet<string>();

            if (IsShiftedKey.Contains(key) || (key.Length == 1 && char.IsUpper(key[0])))
            {
                mappedMods.Add("Shift");
            }

            string[] modifiers = modifiersString.Split('-');
            for (int i = 0; i < modifiers.Length - 1; i++)
            {
                string mod = modifiers[i].ToLower();
                if (!ModMap.ContainsKey(mod))
                {
                    keystroke = default;
                    reason = I18n.S("Compose.Exception.Navigation.InvalidModifier", mod);
                    return false;
                }

                string mappedMod = ModMap[mod];
                if (mappedMods.Contains(mappedMod))
                {
                    keystroke = default;
                    reason = I18n.S("Compose.Exception.Navigation.DuplicateModifier", mod);
                    return false;
                }

                mappedMods.Add(mappedMod);
            }

            if (mappedMods.Contains("u"))
            {
                keystroke.ActuateOnRelease = true;
                mappedMods.Remove("u");
            }

            if (mappedMods.Count > 2)
            {
                keystroke = default;
                reason = I18n.S("Compose.Exception.Navigation.TooManyModifiers", keystrokeString);
                return false;
            }

            foreach (var modifier in mappedMods)
            {
                if (keystroke.Modifier1 == null)
                {
                    keystroke.Modifier1 = modifier;
                }
                else
                {
                    keystroke.Modifier2 = modifier;
                }
            }

            reason = null;
            return true;
        }
    }
}