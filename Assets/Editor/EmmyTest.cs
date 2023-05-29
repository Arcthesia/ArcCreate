using System.IO;
using System.Reflection;
using ArcCreate.Compose.EventsEditor;
using ArcCreate.Gameplay.Scenecontrol;
using ArcCreate.Utility.Lua;
using UnityEditor;
using UnityEngine;

namespace ArcCreate.EditorScripts
{
    public class EmmyTest : Editor
    {
        [MenuItem("ArcCreate/TestEmmy/Scenecontrol")]
        public static void GenerateTestScenecontrolEmmy()
        {
            ScenecontrolLuaEnvironment env = new ScenecontrolLuaEnvironment();
            env.GenerateEmmyLua(Path.Combine(Application.dataPath, ".."));
        }
    }
}