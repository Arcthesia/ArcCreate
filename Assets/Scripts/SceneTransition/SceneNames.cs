namespace ArcCreate.SceneTransition
{
    public static class SceneNames
    {
        public const string BootScene = "Boot";
        public const string ComposeScene = "Compose";
        public const string GameplayScene = "Gameplay";
        public const string RemoteScene = "Remote";
        public const string SelectScene = "Select";

#if UNITY_EDITOR || UNITY_STANDALONE
        public const string DefaultScene = ComposeScene;
#else
        public const string DefaultScene = RemoteScene;
#endif
    }
}