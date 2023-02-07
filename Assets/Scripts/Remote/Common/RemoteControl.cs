namespace ArcCreate.Remote.Common
{
    public enum RemoteControl
    {
        // Control bytes
        Invalid = -2,
        Abort = -1,
        CheckConnection = 0,

        // Playback
        Timing = 1,
        LoopStart = 2,
        LoopEnd = 3,
        Play = 4,
        Pause = 5,

        // Files
        Chart = 16,
        Audio = 17,
        JacketArt = 18,
        Background = 19,

        // Info
        Title = 32,
        Composer = 33,
        Difficulty = 34,
        BaseBpm = 35,
        Speed = 36,
        GlobalOffset = 37,

        // Skin
        AlignmentSkin = 48,
        AccentSkin = 52,
        NoteSkin = 49,
        ParticleSkin = 50,
        SingleLineSkin = 51,
        TrackSkin = 52,

        // Settings
        ShowLog = 64,
    }
}