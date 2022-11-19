using UnityEngine;
using UnityEditor;

public class LoggerEditor : EditorWindow
{
    [MenuItem("Logging/Logger Window")]
    public static void ShowWindow()
    {
        GetWindow(typeof(LoggerEditor));
    }

    [SerializeField]
    private Channel loggerChannels = Logger.kAllChannels;

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
 
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear all"))
        {
            loggerChannels = 0;
        }
        if (GUILayout.Button("Select all"))
        {
            loggerChannels = Logger.kAllChannels;
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Label("Click to toggle logging channels", EditorStyles.boldLabel);
        
        foreach (Channel channel in System.Enum.GetValues(typeof(Channel)))
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Toggle((loggerChannels & channel) == channel, "", GUILayout.ExpandWidth(false));
            if (GUILayout.Button(channel.ToString()))
            {
                loggerChannels ^= channel;
            }
            EditorGUILayout.EndHorizontal();
        }

        // If the game is playing then update it live when changes are made
        if (EditorApplication.isPlaying && EditorGUI.EndChangeCheck())
        {
            Logger.SetChannels(loggerChannels);
        }
    }
    
    // When the game starts update the logger instance with the users selections
    private void OnEnable()
    {
        Logger.SetChannels(loggerChannels);
    }
}
