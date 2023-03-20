namespace ArcCreate.SceneTransition
{
    public static class SceneNames
    {
        public const string BootScene = "Boot";
        public const string ComposeScene = "Compose";
        public const string GameplayScene = "Gameplay";
        public const string RemoteScene = "Remote";
        public const string SelectScene = "Select";
        public const string StorageScene = "Storage";

#if UNITY_EDITOR || UNITY_STANDALONE
        public static readonly string[] DefaultScenes = new string[] { ComposeScene };
#else
        public static readonly string[] DefaultScenes = new string[] { StorageScene, SelectScene };
#endif
    }
}