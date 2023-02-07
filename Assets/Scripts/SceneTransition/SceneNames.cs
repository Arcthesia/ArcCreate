namespace ArcCreate.SceneTransition
{
    public static class SceneNames
    {
        public const string BootScene = "Boot";
        public const string ComposeScene = "Compose";
        public const string GameplayScene = "Gameplay";
        public const string RemoteScene = "Remote";

#if UNITY_EDITOR || UNITY_STANDALONE
        public const string DefaultScene = RemoteScene;
#else
        public const string DefaultScene = RemoteScene;
#endif
    }
}