using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyDoc("Class for requesting input from user through interacting with the track.")]
    [EmmyGroup("Macros")]
    public class TrackInput
    {
        [EmmyDoc("Request a timing selection. Returned value is accessed through the key \"timing\"")]
        public static MacroRequest RequestTiming(bool showVertical = false, string notification = null)
        {
            MacroRequest request = new MacroRequest();
            if (notification != null)
            {
                Services.Popups.Notify(Popups.Severity.Info, notification);
            }
            else
            {
                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.Macros.Select.Timing"));
            }

            Services.Macros.RequestTrackTiming(request, showVertical);
            return request;
        }

        [EmmyDoc("Request a vertical position selection. Returned value is accessed through the key \"x\", \"y\" or \"xy\"")]
        public static MacroRequest RequestPosition(int timing, string notification = null)
        {
            MacroRequest request = new MacroRequest();
            if (notification != null)
            {
                Services.Popups.Notify(Popups.Severity.Info, notification);
            }
            else
            {
                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.Macros.Select.Position"));
            }

            Services.Macros.RequestTrackPosition(request, timing);
            return request;
        }

        [EmmyDoc("Request a lane selection. Returned value is accessed through the key \"lane\"")]
        public static MacroRequest RequestLane(string notification = null)
        {
            MacroRequest request = new MacroRequest();
            if (notification != null)
            {
                Services.Popups.Notify(Popups.Severity.Info, notification);
            }
            else
            {
                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.Macros.Select.Lane"));
            }

            Services.Macros.RequestTrackLane(request);
            return request;
        }
    }
}