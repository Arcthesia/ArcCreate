using ArcCreate.Compose.Timeline;
using UnityEditor;
using UnityEngine;

namespace ArcCreate.EditorScripts
{
    [CustomEditor(typeof(WaveformDisplay))]
    public class WaveformDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            WaveformDisplay disp = (WaveformDisplay)target;
            DrawDefaultInspector();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate"))
            {
                disp.GenerateWaveform();
            }

            GUILayout.EndHorizontal();
        }
    }
}