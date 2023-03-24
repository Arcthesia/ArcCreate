using ArcCreate.Storage;
using ArcCreate.Utility;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ArcCreate.EditorScripts
{
    [CustomEditor(typeof(FileImportManager), true)]
    public class StorageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var storage = (FileImportManager)target;
            DrawDefaultInspector();

            if (GUILayout.Button("Import test package"))
            {
                string importFrom = Shell.OpenFileDialog("ArcCreate Packaage", new string[] { "arcpkg" }, "Import test package", "");
                storage.ImportArchive(importFrom).Forget();
            }
        }
    }
}