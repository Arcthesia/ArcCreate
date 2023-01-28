using System.Text;
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
        /// <param name="keybind">The output keybind.</param>
        /// <param name="reason">The reason for failing to parse the string. This argument will be null if the string is valid.</param>
        /// <returns>Whether or not parsing succeeded.</returns>
        public static bool TryParseKeybind(string keybindString, out Keybind keybind, out string reason)
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

            keybind = new Keybind(keystrokes);
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
            }

            Regex capture = new Regex(@"^<((?:[a-z]-)*)([a-z\d]+|[^<>\s])>$", RegexOptions.IgnoreCase);
            MatchCollection matches = capture.Matches(keystrokeString);
            if (matches.Count != 1 || matches[0].Groups.Count != 2)
            {
                keystroke = default;
                reason = I18n.S("Compose.Exception.Navigation.InvalidKeystroke", keystrokeString);
                return false;
            }

            string modifiersString = matches[0].Groups[1].Value.ToLower();
            string key = matches[0].Groups[2].Value;

            if (!KeyMap.ContainsKey(key))
            {
                keystroke = default;
                reason = I18n.S("Compose.Exception.Navigation.InvalidKey", key);
                return false;
            }

            keystroke = new Keystroke()
            {
                Key = KeyMap[key],
            };

            if (IsShiftedKey.Contains(key) || (key.Length == 1 && char.IsUpper(key[0])))
            {
                keystroke.ShiftKey = true;
            }

            string[] modifiers = modifiersString.Split('-');
            for (int i = 0; i < modifiers.Length - 1; i++)
            {
                string mod = modifiers[i].ToLower();
                if (mod != "c" && mod != "a" && mod != "m" && mod != "s")
                {
                    keystroke = default;
                    reason = I18n.S("Compose.Exception.Navigation.InvalidModifier", mod);
                    return false;
                }

                if ((keystroke.CtrlKey && mod == "c")
                 || (keystroke.AltKey && mod == "a")
                 || (keystroke.MetaKey && mod == "m")
                 || (keystroke.ShiftKey && mod == "s"))
                {
                    keystroke = default;
                    reason = I18n.S("Compose.Exception.Navigation.DuplicateModifier", mod);
                    return false;
                }

                switch (mod)
                {
                    case "c":
                        keystroke.CtrlKey = true;
                        break;
                    case "a":
                        keystroke.AltKey = true;
                        break;
                    case "m":
                        keystroke.MetaKey = true;
                        break;
                    case "s":
                        keystroke.ShiftKey = true;
                        break;
                }
            }

            reason = null;
            return true;
        }

        /// <summary>
        /// Converting a keybind into string.
        /// </summary>
        /// <param name="keybind">The keybind to convert.</param>
        /// <returns>The converted string.</returns>
        public static string ToKeybindString(this Keybind keybind)
        {
            StringBuilder s = new StringBuilder();
            foreach (Keystroke keystroke in keybind.Keystrokes)
            {
                s.Append(ToKeystrokeString(keystroke));
            }

            return s.ToString();
        }

        /// <summary>
        /// Converting a keystroke into string.
        /// </summary>
        /// <param name="keystroke">The keystroke to convert.</param>
        /// <returns>The converted string.</returns>
        public static string ToKeystrokeString(this Keystroke keystroke)
        {
            bool printShift = keystroke.ShiftKey;
            string keyString = keystroke.Key.ToString();

            if (keystroke.ShiftKey)
            {
                if (ShiftKeys.ContainsKey(keystroke.Key))
                {
                    keyString = ShiftKeys[keystroke.Key];
                    printShift = false;
                }
                else if (keyString.Length == 1 && char.IsLetter(keyString[0]))
                {
                    keyString = keyString.ToUpper();
                    printShift = false;
                }
            }

            if (keystroke.CtrlKey || keystroke.AltKey || keystroke.MetaKey || printShift)
            {
                StringBuilder s = new StringBuilder();
                s.Append("<");
                if (keystroke.CtrlKey)
                {
                    s.Append("c-");
                }

                if (keystroke.AltKey)
                {
                    s.Append("a-");
                }

                if (keystroke.MetaKey)
                {
                    s.Append("m-");
                }

                if (printShift)
                {
                    s.Append("s-");
                }

                s.Append(keyString);
                s.Append(">");
                return s.ToString();
            }

            return keyString;
        }
    }
}