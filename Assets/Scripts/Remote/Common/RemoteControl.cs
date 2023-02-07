namespace ArcCreate.Remote.Common
{
    public enum RemoteControl
    {
        // Control bytes
        Invalid = -3,
        ShowLog = -2,
        Abort = -1,
        CheckConnection = 0,

        // Playback
        CurrentTiming = 1,
        StartTiming = 2,
        EndTiming = 3,
        Loop = 4,
        Play = 5,
        Pause = 6,

        // Files
        Chart = 16,
        Audio = 17,
        JacketArt = 18,
        Background = 19,
        VideoBackground = 20,

        // Info
        Title = 32,
        Composer = 33,
        DifficultyName = 34,
        DifficultyColor = 35,
        BaseBpm = 36,
        Speed = 37,
        GlobalOffset = 38,

        // Skin
        AlignmentSkin = 48,
        AccentSkin = 49,
        NoteSkin = 50,
        ParticleSkin = 51,
        SingleLineSkin = 52,
        TrackSkin = 53,

        // TODO: Arc colors
    }
}