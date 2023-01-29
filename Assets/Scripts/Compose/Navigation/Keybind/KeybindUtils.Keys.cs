using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace ArcCreate.Compose.Navigation
{
    /// <summary>
    /// Utility class for converting between string and keybind.
    /// </summary>
    public static partial class KeybindUtils
    {
        public static readonly Dictionary<string, Key> KeyboardAlias = new Dictionary<string, Key>()
        {
            // Alphanumeric
            { "a", Key.A },
            { "b", Key.B },
            { "c", Key.C },
            { "d", Key.D },
            { "e", Key.E },
            { "f", Key.F },
            { "g", Key.G },
            { "h", Key.H },
            { "i", Key.I },
            { "j", Key.J },
            { "k", Key.K },
            { "l", Key.L },
            { "m", Key.M },
            { "n", Key.N },
            { "o", Key.O },
            { "p", Key.P },
            { "q", Key.Q },
            { "r", Key.R },
            { "s", Key.S },
            { "t", Key.T },
            { "u", Key.U },
            { "v", Key.V },
            { "w", Key.W },
            { "x", Key.X },
            { "y", Key.Y },
            { "z", Key.Z },

            // Arrows
            { "left", Key.LeftArrow },
            { "leftarrow", Key.LeftArrow },
            { "arrowleft", Key.LeftArrow },
            { "right", Key.RightArrow },
            { "rightarrow", Key.RightArrow },
            { "arrowright", Key.RightArrow },
            { "up", Key.UpArrow },
            { "uparrow", Key.UpArrow },
            { "arrowup", Key.UpArrow },
            { "down", Key.DownArrow },
            { "downarrow", Key.DownArrow },
            { "arrowdown", Key.DownArrow },

            // Symbols
            { "backquote", Key.Backquote },
            { "quote", Key.Quote },
            { "semicolon", Key.Semicolon },
            { "comma", Key.Comma },
            { "period", Key.Period },
            { "slash", Key.Slash },
            { "backslash", Key.Backslash },
            { "leftbracket", Key.LeftBracket },
            { "rightbracket", Key.RightBracket },
            { "minus", Key.Minus },
            { "equals", Key.Equals },
            { "`", Key.Backquote },
            { "'", Key.Quote },
            { ";", Key.Semicolon },
            { ",", Key.Comma },
            { ".", Key.Period },
            { "/", Key.Slash },
            { "\\", Key.Backslash },
            { "[", Key.LeftBracket },
            { "]", Key.RightBracket },
            { "-", Key.Minus },
            { "=", Key.Equals },

            // Special keys
            { "bs", Key.Backspace },
            { "backspace", Key.Backspace },
            { "menu", Key.ContextMenu },
            { "apps", Key.ContextMenu },
            { "contextmenu", Key.ContextMenu },
            { "del", Key.Delete },
            { "delete", Key.Delete },
            { "return", Key.Enter },
            { "cr", Key.Enter },
            { "enter", Key.Enter },
            { "esc", Key.Escape },
            { "escape", Key.Escape },
            { "pgup", Key.PageUp },
            { "pageup", Key.PageUp },
            { "pgdn", Key.PageDown },
            { "pagedown", Key.PageDown },
            { "space", Key.Space },
            { "tab", Key.Tab },
            { "leftshift", Key.LeftShift },
            { "rightshift", Key.RightShift },
            { "shift", Key.LeftShift },
            { "leftalt", Key.LeftAlt },
            { "rightalt", Key.RightAlt },
            { "alt", Key.LeftAlt },
            { "altgr", Key.AltGr },
            { "leftctrl", Key.LeftCtrl },
            { "rightctrl", Key.RightCtrl },
            { "ctrl", Key.LeftCtrl },
            { "leftmeta", Key.LeftMeta },
            { "leftwindows", Key.LeftWindows },
            { "leftapple", Key.LeftApple },
            { "leftcommand", Key.LeftCommand },
            { "rightmeta", Key.RightMeta },
            { "rightwindows", Key.RightWindows },
            { "rightapple", Key.RightApple },
            { "rightcommand", Key.RightCommand },
            { "meta", Key.LeftMeta },
            { "windows", Key.LeftWindows },
            { "apple", Key.LeftApple },
            { "command", Key.LeftCommand },
            { "home", Key.Home },
            { "end", Key.End },
            { "insert", Key.Insert },
            { "capslock", Key.CapsLock },
            { "numlock", Key.NumLock },
            { "printscreen", Key.PrintScreen },
            { "prtscn", Key.PrintScreen },
            { "scrolllock", Key.ScrollLock },
            { "pause", Key.Pause },

            // Numpad
            { "numpad0", Key.Numpad0 },
            { "numpad1", Key.Numpad1 },
            { "numpad2", Key.Numpad2 },
            { "numpad3", Key.Numpad3 },
            { "numpad4", Key.Numpad4 },
            { "numpad5", Key.Numpad5 },
            { "numpad6", Key.Numpad6 },
            { "numpad7", Key.Numpad7 },
            { "numpad8", Key.Numpad8 },
            { "numpad9", Key.Numpad9 },
            { "numpadenter", Key.NumpadEnter },
            { "numpaddivide", Key.NumpadDivide },
            { "numpadmultiply", Key.NumpadMultiply },
            { "numpadplus", Key.NumpadPlus },
            { "numpadminus", Key.NumpadMinus },
            { "numpadperiod", Key.NumpadPeriod },
            { "numpadequals", Key.NumpadEquals },

            // F-keys
            { "f1", Key.F1 },
            { "f2", Key.F2 },
            { "f3", Key.F3 },
            { "f4", Key.F4 },
            { "f5", Key.F5 },
            { "f6", Key.F6 },
            { "f7", Key.F7 },
            { "f8", Key.F8 },
            { "f9", Key.F9 },
            { "f10", Key.F10 },
            { "f11", Key.F11 },
            { "f12", Key.F12 },

            // Shift modified
            { "~", Key.Backquote },
            { "_", Key.Minus },
            { "+", Key.Equals },
            { "{", Key.LeftBracket },
            { "}", Key.RightBracket },
            { "|", Key.Backslash },
            { ":", Key.Semicolon },
            { "\"", Key.Quote },
            { "<", Key.Comma },
            { ">", Key.Period },
            { "?", Key.Slash },
        };

        public static readonly Dictionary<string, string> MiscKeyAlias = new Dictionary<string, string>()
        {
            { "digit1", "<Keyboard>/1" },
            { "digit2", "<Keyboard>/2" },
            { "digit3", "<Keyboard>/3" },
            { "digit4", "<Keyboard>/4" },
            { "digit5", "<Keyboard>/5" },
            { "digit6", "<Keyboard>/6" },
            { "digit7", "<Keyboard>/7" },
            { "digit8", "<Keyboard>/8" },
            { "digit9", "<Keyboard>/9" },
            { "digit0", "<Keyboard>/0" },
            { "1", "<Keyboard>/1" },
            { "2", "<Keyboard>/2" },
            { "3", "<Keyboard>/3" },
            { "4", "<Keyboard>/4" },
            { "5", "<Keyboard>/5" },
            { "6", "<Keyboard>/6" },
            { "7", "<Keyboard>/7" },
            { "8", "<Keyboard>/8" },
            { "9", "<Keyboard>/9" },
            { "0", "<Keyboard>/0" },
            { "!", "<Keyboard>/1" },
            { "@", "<Keyboard>/2" },
            { "#", "<Keyboard>/3" },
            { "$", "<Keyboard>/4" },
            { "%", "<Keyboard>/5" },
            { "^", "<Keyboard>/6" },
            { "&", "<Keyboard>/7" },
            { "*", "<Keyboard>/8" },
            { "(", "<Keyboard>/9" },
            { ")", "<Keyboard>/0" },
            { "mouse1", "<Mouse>/leftButton" },
            { "btn1", "<Mouse>/leftButton" },
            { "button1", "<Mouse>/leftButton" },
            { "mouse2", "<Mouse>/rightButton" },
            { "btn2", "<Mouse>/rightButton" },
            { "button2", "<Mouse>/rightButton" },
            { "mouse3", "<Mouse>/middleButton" },
            { "btn3", "<Mouse>/middleButton" },
            { "button3", "<Mouse>/middleButton" },
        };

        public static readonly HashSet<string> IsShiftedKey = new HashSet<string>()
        {
            "`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=",
            "[", "]", "\\",
            ";", "'",
            ",", ".", "/",
        };

        public static readonly Dictionary<Key, string> ShiftKeys = new Dictionary<Key, string>()
        {
            { Key.Backquote, "~" },
            { Key.Digit1, "!" },
            { Key.Digit2, "@" },
            { Key.Digit3, "#" },
            { Key.Digit4, "$" },
            { Key.Digit5, "%" },
            { Key.Digit6, "^" },
            { Key.Digit7, "&" },
            { Key.Digit8, "*" },
            { Key.Digit9, "(" },
            { Key.Digit0, ")" },
            { Key.Minus, "_" },
            { Key.Equals, "+" },
            { Key.LeftBracket, "{" },
            { Key.RightBracket, "}" },
            { Key.Backslash, "|" },
            { Key.Semicolon, ":" },
            { Key.Quote, "\"" },
            { Key.Comma, "<" },
            { Key.Period, ">" },
            { Key.Slash, "?" },
        };

        public static readonly Dictionary<string, string> ModMap = new Dictionary<string, string>()
        {
            { "c", "Ctrl" },
            { "s", "Shift" },
            { "u", "u" },
            { "h", "h" },
            { "a", "Alt" },
        };

        public static bool TryGetKeyString(string alias, out string key)
        {
            alias = alias.ToLower();
            if (KeyboardAlias.TryGetValue(alias, out Key keyboardKey))
            {
                key = "<Keyboard>/" + keyboardKey.ToString();
                return true;
            }
            else if (MiscKeyAlias.TryGetValue(alias, out var miscKey))
            {
                key = miscKey;
                return true;
            }
            else
            {
                key = null;
                return false;
            }
        }
    }
}