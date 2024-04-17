using UnityEditor;

namespace ArcCreate.EditorScripts
{
    public class ScriptBatch 
    {
        private static readonly string[] Levels = new string[] {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/Compose.unity",
            "Assets/Scenes/Gameplay.unity",
            "Assets/Scenes/Greeting.unity",
            "Assets/Scenes/Remote.unity",
            "Assets/Scenes/Result.unity",
            "Assets/Scenes/Select.unity",
            "Assets/Scenes/Storage.unity",
        };

        [MenuItem("ArcCreate/Custom build because linux is stupid")]
        public static void BuildGame ()
        {
            string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
            BuildPipeline.BuildPlayer(Levels, path + "/ArcCreate", BuildTarget.StandaloneLinux64, BuildOptions.None);
        }
    }
}