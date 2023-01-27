using System;

namespace ArcCreate.Compose.Popups
{
    public enum ButtonColor
    {
        Default,
        Highlight,
        Warning,
        Danger,
    }

    public struct ButtonSetting
    {
        public string Text;
        public Action Callback;
        public ButtonColor ButtonColor;
    }
}