using System.IO;
using System.Reflection;
using ArcCreate.Compose.EventsEditor;
using ArcCreate.Gameplay.Scenecontrol;
using ArcCreate.Utilities.Lua;
using UnityEditor;
using UnityEngine;

namespace ArcCreate.EditorScripts
{
    public class EmmyTest : Editor
    {
        [MenuItem("ArcCreate/TestEmmy/Scenecontrol")]
        public static void GenerateTestScenecontrolEmmy()
        {
            Assembly scAssembly = Assembly.GetAssembly(typeof(ScenecontrolService));
            var emmy = LuaRunner.GetCommonEmmySharp();
            emmy.AppendAssembly(scAssembly);
            emmy.AppendFunction(typeof(ScenecontrolLuaEnvironment).GetMethod("AddScenecontrol"));
            emmy.AppendFunction(typeof(ScenecontrolLuaEnvironment).GetMethod("Notify"));
            emmy.AppendFunction(typeof(ScenecontrolLuaEnvironment).GetMethod("NotifyWarn"));
            emmy.AppendFunction(typeof(ScenecontrolLuaEnvironment).GetMethod("NotifyError"));
            emmy.Build(Path.GetDirectoryName(Application.dataPath));
        }
    }
}