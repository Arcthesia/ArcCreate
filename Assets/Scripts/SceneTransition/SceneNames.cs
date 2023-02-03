namespace ArcCreate.SceneTransition
{
    public static class SceneNames
    {
        public const string BootScene = "Boot";
        public const string ComposeScene = "Compose";
        public const string GameplayScene = "Gameplay";

#if UNITY_EDITOR || UNITY_STANDALONE
        public const string DefaultScene = ComposeScene;
#else
        public const string DefaultScene = GameplayScene;
#endif
    }
}