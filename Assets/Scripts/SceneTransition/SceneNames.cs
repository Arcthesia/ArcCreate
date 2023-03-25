namespace ArcCreate.SceneTransition
{
    public static class SceneNames
    {
        public const string TitleScene = "Title";
        public const string SongSelectScene = "Select";
        public const string BootScene = "Boot";
        public const string ComposeScene = "Compose";
        public const string GameplayScene = "Gameplay";
        public const string RemoteScene = "Remote";
        public const string SelectScene = "Select";
        public const string StorageScene = "Storage";

#if UNITY_EDITOR || UNITY_STANDALONE
        public const string DefaultScene = SelectScene;
#else
        public const string DefaultScene = SelectScene;
#endif

#if UNITY_EDITOR || !UNITY_STANDALONE
        public static readonly string[] RequiredScenes = new string[] { StorageScene };
#else
        public static readonly string[] RequiredScenes = new string[0];
#endif
    }
}