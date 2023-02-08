namespace ArcCreate.Remote.Common
{
    public enum RemoteControl
    {
        // Control bytes
        Invalid = short.MinValue,
        ShowLog = -2,
        Abort = -1,
        StartConnection = 0,

        // Playback
        CurrentTiming = 1,
        Play = 2,
        Pause = 3,

        // Files
        ReloadAllFiles = 16,
        Chart = 17,
        Audio = 18,
        JacketArt = 19,
        Background = 20,
        Metadata = 21,

        // Settings
        Speed = 32,
        GlobalOffset = 33,
    }
}