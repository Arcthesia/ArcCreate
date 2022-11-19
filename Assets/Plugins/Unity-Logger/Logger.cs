#define UNITY_DIALOGS // Comment out to disable dialogs for fatal errors
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR && UNITY_DIALOGS
using UnityEditor;
#endif

///////////////////////////
// Types
///////////////////////////

[System.Flags]
public enum Channel : uint
{
/// <summary>
    /// Logs from C# to do with our Lua api
    /// </summary>
    Lua             = 1 << 0,
    /// <summary>
    /// Logs directly from the Lua VM
    /// </summary>
    LuaNative       = 1 << 1,
    /// <summary>
    /// Logs to do with AI/GOAP/
    /// </summary>
    AI              = 1 << 2,
    /// <summary>
    /// Logs to do with graphics/rendering
    /// </summary>
    Rendering       = 1 << 3,
    /// <summary>
    /// Logs to do with the physics system
    /// </summary>
    Physics         = 1 << 4,
    /// <summary>
    /// Logs to do with our UI system
    /// </summary>
    UI              = 1 << 5,
    /// <summary>
    /// Logs about NetDevices and networks
    /// </summary>
    NetDevice       = 1 << 6,
    /// <summary>
    /// Logs to do with sound and Wwise
    /// </summary>
    Audio           = 1 << 7,
    /// <summary>
    /// Logs to do with level loading
    /// </summary>
    Loading         = 1 << 8,
    /// <summary>
    /// Logs to do with localisation
    /// </summary>
    Localisation    = 1 << 9,
    /// <summary>
    /// Logs to do with platform services
    /// </summary>
    Platform        = 1 << 10,
    /// <summary>
    /// Logs asserts
    /// </summary>
    Assert          = 1 << 11,
    /// <summary>
    /// Build/Content generation logs
    /// </summary>
    Build           = 1 << 12,
    /// <summary>
    /// Analytics logs
    /// </summary>
    Analytics       = 1 << 13,
    /// <summary>
    /// Animation logs
    /// </summary>
    Animation       = 1 << 14,
}

/// <summary>
/// The priority of the log
/// </summary>
public enum Priority
{
    // Default, simple output about game
    Info,
    // Warnings that things might not be as expected
    Warning,
    // Things have already failed, alert the dev
    Error,
    // Things will not recover, bring up pop up dialog
    FatalError,
}

public class Logger
{
    public const Channel kAllChannels = (Channel) ~0u;

    ///////////////////////////
    // Singleton set up 
    ///////////////////////////

    private static Logger instance;
    private static Logger Instance
    {
        get
        {
            return instance ?? (instance = new Logger());
        }

    }
    
    private Logger()
    {
        m_Channels 	= kAllChannels;
    }
    
    ///////////////////////////
    // Members
    ///////////////////////////
    private Channel m_Channels;

    public delegate void OnLogFunc(Channel channel, Priority priority, string message);
    public static event OnLogFunc OnLog;

    ///////////////////////////
    // Channel Control
    ///////////////////////////

    public static void ResetChannels()
    {
        Instance.m_Channels = kAllChannels;
    }

    public static void AddChannel(Channel channelToAdd)
    {
        Instance.m_Channels |= channelToAdd;
    }

    public static void RemoveChannel(Channel channelToRemove)
    {
        Instance.m_Channels &= ~channelToRemove;
    }

    public static void ToggleChannel(Channel channelToToggle)
    {
        Instance.m_Channels ^= channelToToggle;
    }

    public static bool IsChannelActive(Channel channelToCheck)
    {
        return (Instance.m_Channels & channelToCheck) == channelToCheck;
    }

    public static void SetChannels(Channel channelsToSet)
    {
        Instance.m_Channels = channelsToSet;
    }

    ///////////////////////////

    ///////////////////////////
    // Logging functions
    ///////////////////////////

    /// <summary>
    /// Standard logging function, priority will default to info level
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="message"></param>
    public static void Log(Channel logChannel, string message)
    {
        FinalLog(logChannel, Priority.Info, message);
    }

    /// <summary>
    /// Standard logging function with specified priority
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="priority"></param>
    /// <param name="message"></param>
    public static void Log(Channel logChannel, Priority priority, string message)
    {
        FinalLog(logChannel, priority, message);
    }

    /// <summary>
    /// Log with format args, priority will default to info level
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Log(Channel logChannel, string message, params object[] args)
    {
        FinalLog(logChannel, Priority.Info, string.Format(message, args));
    }

    /// <summary>
    /// Log with format args and specified priority
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="priority"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Log(Channel logChannel, Priority priority, string message, params object[] args)
    {
        FinalLog(logChannel, priority, string.Format(message, args));
    }

    /// <summary>
    /// Assert that the passed in condition is true, otherwise log a fatal error
    /// </summary>
    /// <param name="condition">The condition to test</param>
    /// <param name="message">A user provided message that will be logged</param>
    public static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            FinalLog(Channel.Assert, Priority.FatalError, string.Format("Assert Failed: {0}", message));
        }
    }

    /// <summary>
    /// This function controls where the final string goes
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="priority"></param>
    /// <param name="message"></param>
    private static void FinalLog(Channel logChannel, Priority priority, string message)
    {
        if (IsChannelActive(logChannel))
        {
            // Dialog boxes can't support rich text mark up, do we won't colour the final string 
			string finalMessage = ContructFinalString(logChannel, priority, message, (priority != Priority.FatalError));

#if UNITY_EDITOR && UNITY_DIALOGS
            // Fatal errors will create a pop up when in the editor
            if (priority == Priority.FatalError)
			{
			    bool ignore = EditorUtility.DisplayDialog("Fatal error", finalMessage, "Ignore", "Break");
			    if (!ignore)
			    {
			        Debug.Break();
			    }
            }
#endif 
            // Call the correct unity logging function depending on the type of error 
            switch (priority)
            {
                case Priority.FatalError:
                case Priority.Error:
                    Debug.LogError(finalMessage);
                    break;

                case Priority.Warning:
                    Debug.LogWarning(finalMessage);
                    break;

                case Priority.Info:
                    Debug.Log(finalMessage);
                    break;
            }

            if(OnLog != null)
            {
                OnLog.Invoke(logChannel, priority, finalMessage);
            }
        }
    }

    /// <summary>
    /// Creates the final string with colouration based on channel and priority 
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="priority"></param>
    /// <param name="message"></param>
    /// <param name="shouldColour"></param>
    /// <returns></returns>
    private static string ContructFinalString(Channel logChannel, Priority priority, string message, bool shouldColour)
    {
        string channelColour    = null;
        string priortiyColour   = priorityToColour[priority];

        if(!channelToColour.TryGetValue(logChannel, out channelColour))
        {
            channelColour = "black";
            Debug.LogErrorFormat("Please add colour for channel {0}", logChannel);
        }

        if(shouldColour)
        {
            return string.Format("<b><color={0}>[{1}] </color></b> <color={2}>{3}</color>", channelColour, logChannel, priortiyColour, message);
        }

        return string.Format("[{0}] {1}", logChannel, message);
    }

    /// <summary>
    /// Map a channel to a colour, using Unity's rich text system
    /// </summary>
    private static readonly Dictionary<Channel, string> channelToColour = new Dictionary<Channel, string>
    {
        { Channel.Lua,          "cyan" },
        { Channel.LuaNative,    "lightblue" },
        { Channel.AI,           "blue" },
        { Channel.Rendering,    "green" },
        { Channel.Physics,      "yellow" },
        { Channel.UI,           "purple" },
        { Channel.NetDevice,    "orange" },
        { Channel.Audio,        "teal" },
        { Channel.Loading,      "olive" },
        { Channel.Localisation, "brown" },
        { Channel.Platform,     "red" },
        { Channel.Assert,       "red" },
        { Channel.Build,        "navy" },
        { Channel.Analytics,    "maroon" },
    };

    /// <summary>
    /// Map a priority to a colour, using Unity's rich text system
    /// </summary>
    private static readonly Dictionary<Priority, string> priorityToColour = new Dictionary<Priority, string>
    {
		#if UNITY_PRO_LICENSE
        { Priority.Info,        "white" },
		#else
		{ Priority.Info,        "black" },
		#endif
        { Priority.Warning,     "orange" },
        { Priority.Error,       "red" },
        { Priority.FatalError,  "red" },
    };
}
