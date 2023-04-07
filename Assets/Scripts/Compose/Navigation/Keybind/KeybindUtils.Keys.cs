using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Compose.Navigation
{
    /// <summary>
    /// Utility class for converting between string and keybind.
    /// </summary>
    public static partial class KeybindUtils
    {
        public static readonly Dictionary<string, KeyCode> KeyboardAlias = new Dictionary<string, KeyCode>()
        {
            // Alphanumeric
            { "a", KeyCode.A },
            { "b", KeyCode.B },
            { "c", KeyCode.C },
            { "d", KeyCode.D },
            { "e", KeyCode.E },
            { "f", KeyCode.F },
            { "g", KeyCode.G },
            { "h", KeyCode.H },
            { "i", KeyCode.I },
            { "j", KeyCode.J },
            { "k", KeyCode.K },
            { "l", KeyCode.L },
            { "m", KeyCode.M },
            { "n", KeyCode.N },
            { "o", KeyCode.O },
            { "p", KeyCode.P },
            { "q", KeyCode.Q },
            { "r", KeyCode.R },
            { "s", KeyCode.S },
            { "t", KeyCode.T },
            { "u", KeyCode.U },
            { "v", KeyCode.V },
            { "w", KeyCode.W },
            { "x", KeyCode.X },
            { "y", KeyCode.Y },
            { "z", KeyCode.Z },

            // Arrows
            { "left", KeyCode.LeftArrow },
            { "leftarrow", KeyCode.LeftArrow },
            { "arrowleft", KeyCode.LeftArrow },
            { "right", KeyCode.RightArrow },
            { "rightarrow", KeyCode.RightArrow },
            { "arrowright", KeyCode.RightArrow },
            { "up", KeyCode.UpArrow },
            { "uparrow", KeyCode.UpArrow },
            { "arrowup", KeyCode.UpArrow },
            { "down", KeyCode.DownArrow },
            { "downarrow", KeyCode.DownArrow },
            { "arrowdown", KeyCode.DownArrow },

            // Symbols
            { "backquote", KeyCode.BackQuote },
            { "quote", KeyCode.Quote },
            { "semicolon", KeyCode.Semicolon },
            { "comma", KeyCode.Comma },
            { "period", KeyCode.Period },
            { "slash", KeyCode.Slash },
            { "backslash", KeyCode.Backslash },
            { "leftbracket", KeyCode.LeftBracket },
            { "rightbracket", KeyCode.RightBracket },
            { "minus", KeyCode.Minus },
            { "equals", KeyCode.Equals },
            { "`", KeyCode.BackQuote },
            { "'", KeyCode.Quote },
            { ";", KeyCode.Semicolon },
            { ",", KeyCode.Comma },
            { ".", KeyCode.Period },
            { "/", KeyCode.Slash },
            { "\\", KeyCode.Backslash },
            { "[", KeyCode.LeftBracket },
            { "]", KeyCode.RightBracket },
            { "-", KeyCode.Minus },
            { "=", KeyCode.Equals },

            // Special keys
            { "bs", KeyCode.Backspace },
            { "backspace", KeyCode.Backspace },
            { "menu", KeyCode.Menu },
            { "apps", KeyCode.Menu },
            { "contextmenu", KeyCode.Menu },
            { "del", KeyCode.Delete },
            { "delete", KeyCode.Delete },
            { "return", KeyCode.Return },
            { "cr", KeyCode.Return },
            { "enter", KeyCode.Return },
            { "esc", KeyCode.Escape },
            { "escape", KeyCode.Escape },
            { "pgup", KeyCode.PageUp },
            { "pageup", KeyCode.PageUp },
            { "pgdn", KeyCode.PageDown },
            { "pagedown", KeyCode.PageDown },
            { "space", KeyCode.Space },
            { "tab", KeyCode.Tab },
            { "leftshift", KeyCode.LeftShift },
            { "rightshift", KeyCode.RightShift },
            { "shift", KeyCode.LeftShift },
            { "leftalt", KeyCode.LeftAlt },
            { "rightalt", KeyCode.RightAlt },
            { "alt", KeyCode.LeftAlt },
            { "altgr", KeyCode.AltGr },
            { "leftctrl", KeyCode.LeftControl },
            { "rightctrl", KeyCode.RightControl },
            { "ctrl", KeyCode.LeftControl },
            { "leftmeta", KeyCode.LeftWindows },
            { "leftwindows", KeyCode.LeftWindows },
            { "leftapple", KeyCode.LeftApple },
            { "leftcommand", KeyCode.LeftCommand },
            { "rightmeta", KeyCode.RightWindows },
            { "rightwindows", KeyCode.RightWindows },
            { "rightapple", KeyCode.RightApple },
            { "rightcommand", KeyCode.RightCommand },
            { "meta", KeyCode.LeftWindows },
            { "windows", KeyCode.LeftWindows },
            { "apple", KeyCode.LeftApple },
            { "command", KeyCode.LeftCommand },
            { "home", KeyCode.Home },
            { "end", KeyCode.End },
            { "insert", KeyCode.Insert },
            { "capslock", KeyCode.CapsLock },
            { "numlock", KeyCode.Numlock },
            { "printscreen", KeyCode.Print },
            { "prtscn", KeyCode.Print },
            { "scrolllock", KeyCode.ScrollLock },
            { "pause", KeyCode.Pause },

            // Number
            { "digit1", KeyCode.Alpha1 },
            { "digit2", KeyCode.Alpha2 },
            { "digit3", KeyCode.Alpha3 },
            { "digit4", KeyCode.Alpha4 },
            { "digit5", KeyCode.Alpha5 },
            { "digit6", KeyCode.Alpha6 },
            { "digit7", KeyCode.Alpha7 },
            { "digit8", KeyCode.Alpha8 },
            { "digit9", KeyCode.Alpha9 },
            { "digit0", KeyCode.Alpha0 },
            { "1", KeyCode.Alpha1 },
            { "2", KeyCode.Alpha2 },
            { "3", KeyCode.Alpha3 },
            { "4", KeyCode.Alpha4 },
            { "5", KeyCode.Alpha5 },
            { "6", KeyCode.Alpha6 },
            { "7", KeyCode.Alpha7 },
            { "8", KeyCode.Alpha8 },
            { "9", KeyCode.Alpha9 },
            { "0", KeyCode.Alpha0 },

            // Numpad
            { "numpad0", KeyCode.Keypad0 },
            { "numpad1", KeyCode.Keypad1 },
            { "numpad2", KeyCode.Keypad2 },
            { "numpad3", KeyCode.Keypad3 },
            { "numpad4", KeyCode.Keypad4 },
            { "numpad5", KeyCode.Keypad5 },
            { "numpad6", KeyCode.Keypad6 },
            { "numpad7", KeyCode.Keypad7 },
            { "numpad8", KeyCode.Keypad8 },
            { "numpad9", KeyCode.Keypad9 },
            { "numpadenter", KeyCode.KeypadEnter },
            { "numpaddivide", KeyCode.KeypadDivide },
            { "numpadmultiply", KeyCode.KeypadMultiply },
            { "numpadplus", KeyCode.KeypadPlus },
            { "numpadminus", KeyCode.KeypadMinus },
            { "numpadperiod", KeyCode.KeypadPeriod },
            { "numpadequals", KeyCode.KeypadEquals },

            // F-keys
            { "f1", KeyCode.F1 },
            { "f2", KeyCode.F2 },
            { "f3", KeyCode.F3 },
            { "f4", KeyCode.F4 },
            { "f5", KeyCode.F5 },
            { "f6", KeyCode.F6 },
            { "f7", KeyCode.F7 },
            { "f8", KeyCode.F8 },
            { "f9", KeyCode.F9 },
            { "f10", KeyCode.F10 },
            { "f11", KeyCode.F11 },
            { "f12", KeyCode.F12 },

            // Shift modified
            { "~", KeyCode.BackQuote },
            { "_", KeyCode.Minus },
            { "+", KeyCode.Equals },
            { "{", KeyCode.LeftBracket },
            { "}", KeyCode.RightBracket },
            { "|", KeyCode.Backslash },
            { ":", KeyCode.Semicolon },
            { "\"", KeyCode.Quote },
            { "<", KeyCode.Comma },
            { ">", KeyCode.Period },
            { "?", KeyCode.Slash },
            { "!", KeyCode.Alpha1 },
            { "@", KeyCode.Alpha2 },
            { "#", KeyCode.Alpha3 },
            { "$", KeyCode.Alpha4 },
            { "%", KeyCode.Alpha5 },
            { "^", KeyCode.Alpha6 },
            { "&", KeyCode.Alpha7 },
            { "*", KeyCode.Alpha8 },
            { "(", KeyCode.Alpha9 },
            { ")", KeyCode.Alpha0 },

            // Mouse keys
            { "mouse1", KeyCode.Mouse0 },
            { "btn1", KeyCode.Mouse0 },
            { "button1", KeyCode.Mouse0 },
            { "mouse2", KeyCode.Mouse1 },
            { "btn2", KeyCode.Mouse1 },
            { "button2", KeyCode.Mouse1 },
            { "mouse3", KeyCode.Mouse2 },
            { "btn3", KeyCode.Mouse2 },
            { "button3", KeyCode.Mouse2 },
        };

        public static readonly HashSet<string> IsShiftedKey = new HashSet<string>()
        {
            "~", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+",
            "{", "}", "|",
            ":", "\"",
            "<", ">", "?",
        };

        public static readonly Dictionary<KeyCode, string> ShiftKeys = new Dictionary<KeyCode, string>()
        {
            { KeyCode.BackQuote, "~" },
            { KeyCode.Alpha1, "!" },
            { KeyCode.Alpha2, "@" },
            { KeyCode.Alpha3, "#" },
            { KeyCode.Alpha4, "$" },
            { KeyCode.Alpha5, "%" },
            { KeyCode.Alpha6, "^" },
            { KeyCode.Alpha7, "&" },
            { KeyCode.Alpha8, "*" },
            { KeyCode.Alpha9, "(" },
            { KeyCode.Alpha0, ")" },
            { KeyCode.Minus, "_" },
            { KeyCode.Equals, "+" },
            { KeyCode.LeftBracket, "{" },
            { KeyCode.RightBracket, "}" },
            { KeyCode.Backslash, "|" },
            { KeyCode.Semicolon, ":" },
            { KeyCode.Quote, "\"" },
            { KeyCode.Comma, "<" },
            { KeyCode.Period, ">" },
            { KeyCode.Slash, "?" },
        };

        public static readonly Dictionary<string, KeyCode> ModMap = new Dictionary<string, KeyCode>()
        {
            { "c", KeyCode.LeftControl },
            { "s", KeyCode.LeftShift },
            { "a", KeyCode.LeftAlt },
        };

        public static bool TryGetKeyCode(string alias, out KeyCode key)
        {
            alias = alias.ToLower();
            if (KeyboardAlias.TryGetValue(alias, out key))
            {
                return true;
            }
            else
            {
                key = default;
                return false;
            }
        }
    }
}