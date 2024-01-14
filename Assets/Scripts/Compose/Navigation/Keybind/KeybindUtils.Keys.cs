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

        public static readonly Dictionary<KeyCode, string> KeyToString = new Dictionary<KeyCode, string>()
        {
            // Alphanumeric
            { KeyCode.A, "A" },
            { KeyCode.B, "B" },
            { KeyCode.C, "C" },
            { KeyCode.D, "D" },
            { KeyCode.E, "E" },
            { KeyCode.F, "F" },
            { KeyCode.G, "G" },
            { KeyCode.H, "H" },
            { KeyCode.I, "I" },
            { KeyCode.J, "J" },
            { KeyCode.K, "K" },
            { KeyCode.L, "L" },
            { KeyCode.M, "M" },
            { KeyCode.N, "N" },
            { KeyCode.O, "O" },
            { KeyCode.P, "P" },
            { KeyCode.Q, "Q" },
            { KeyCode.R, "R" },
            { KeyCode.S, "S" },
            { KeyCode.T, "T" },
            { KeyCode.U, "U" },
            { KeyCode.V, "V" },
            { KeyCode.W, "W" },
            { KeyCode.X, "X" },
            { KeyCode.Y, "Y" },
            { KeyCode.Z, "Z" },

            // Arrows
            { KeyCode.LeftArrow, "←" },
            { KeyCode.RightArrow, "→" },
            { KeyCode.UpArrow, "↑" },
            { KeyCode.DownArrow, "↓" },

            // Symbols
            { KeyCode.BackQuote, "`" },
            { KeyCode.Quote, "'" },
            { KeyCode.Semicolon, "\"" },
            { KeyCode.Comma, "," },
            { KeyCode.Period, "." },
            { KeyCode.Slash, "/" },
            { KeyCode.Backslash, "\\" },
            { KeyCode.LeftBracket, "[" },
            { KeyCode.RightBracket, "]" },
            { KeyCode.Minus, "-" },
            { KeyCode.Equals, "=" },

            // Special keys
            { KeyCode.Backspace, "Backspace" },
            { KeyCode.Menu, "Menu" },
            { KeyCode.Delete, "Del" },
            { KeyCode.Return, "Enter" },
            { KeyCode.Escape, "Esc" },
            { KeyCode.PageUp, "PgUp" },
            { KeyCode.PageDown, "PgDn" },
            { KeyCode.Space, "␣" },
            { KeyCode.Tab, "Tab" },
            { KeyCode.LeftShift, "LShift" },
            { KeyCode.RightShift, "RShift" },
            { KeyCode.LeftAlt, "LAlt" },
            { KeyCode.RightAlt, "RAlt" },
            { KeyCode.LeftControl, "LCtrl" },
            { KeyCode.RightControl, "RCtrl" },
            { KeyCode.Home, "Home" },
            { KeyCode.End, "End" },
            { KeyCode.Insert, "Ins" },
            { KeyCode.CapsLock, "CapsLk" },
            { KeyCode.Numlock, "NumLk" },
            { KeyCode.Print, "PrtSc" },
            { KeyCode.ScrollLock, "ScrLk" },
            { KeyCode.Pause, "Pause" },
            { KeyCode.LeftWindows, "LWin" },
            { KeyCode.LeftApple, "LWin" },
            { KeyCode.RightWindows, "RWin" },
            { KeyCode.RightApple, "RWin" },

            // Number
            { KeyCode.Alpha1, "1" },
            { KeyCode.Alpha2, "2" },
            { KeyCode.Alpha3, "3" },
            { KeyCode.Alpha4, "4" },
            { KeyCode.Alpha5, "5" },
            { KeyCode.Alpha6, "6" },
            { KeyCode.Alpha7, "7" },
            { KeyCode.Alpha8, "8" },
            { KeyCode.Alpha9, "9" },
            { KeyCode.Alpha0, "0" },

            // Numpad
            { KeyCode.Keypad0, "Numpad 0" },
            { KeyCode.Keypad1, "Numpad 1" },
            { KeyCode.Keypad2, "Numpad 2" },
            { KeyCode.Keypad3, "Numpad 3" },
            { KeyCode.Keypad4, "Numpad 4" },
            { KeyCode.Keypad5, "Numpad 5" },
            { KeyCode.Keypad6, "Numpad 6" },
            { KeyCode.Keypad7, "Numpad 7" },
            { KeyCode.Keypad8, "Numpad 8" },
            { KeyCode.Keypad9, "Numpad 9" },
            { KeyCode.KeypadEnter, "Numpad Enter" },
            { KeyCode.KeypadDivide, "Numpad /" },
            { KeyCode.KeypadMultiply, "Numpad *" },
            { KeyCode.KeypadPlus, "Numpad +" },
            { KeyCode.KeypadMinus, "Numpad -" },
            { KeyCode.KeypadPeriod, "Numpad ." },
            { KeyCode.KeypadEquals, "Numpad =" },

            // F-keys
            { KeyCode.F1, "F1" },
            { KeyCode.F2, "F2" },
            { KeyCode.F3, "F3" },
            { KeyCode.F4, "F4" },
            { KeyCode.F5, "F5" },
            { KeyCode.F6, "F6" },
            { KeyCode.F7, "F7" },
            { KeyCode.F8, "F8" },
            { KeyCode.F9, "F9" },
            { KeyCode.F10, "F10" },
            { KeyCode.F11, "F11" },
            { KeyCode.F12, "F12" },

            // Mouse keys
            { KeyCode.Mouse0, "LMouse" },
            { KeyCode.Mouse1, "RMouse" },
            { KeyCode.Mouse2, "MMouse" },
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

        public static string GetString(KeyCode key)
        {
            if (KeyToString.TryGetValue(key, out string str))
            {
                return str;
            }

            return string.Empty;
        }
    }
}