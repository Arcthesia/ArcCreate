using ArcCreate.Gameplay.Audio;
using UnityEditor;
using UnityEngine;

namespace ArcCreate.EditorScripts
{
    [CustomEditor(typeof(AudioService))]
    public class AudioServiceEditor : Editor
    {
        private int targetTiming = 0;
        private int delay = 2000;

        public override void OnInspectorGUI()
        {
            AudioService audio = (AudioService)target;
            DrawDefaultInspector();

            /////
            GUILayout.BeginHorizontal();
            GUILayout.Label("Timing");
            targetTiming = EditorGUILayout.IntField(targetTiming);
            GUILayout.Label("Delay");
            delay = EditorGUILayout.IntField(delay);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pause"))
            {
                audio.Pause();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("PlayImmediate"))
            {
                audio.PlayImmediately(targetTiming);
            }

            if (GUILayout.Button("PlayDelay"))
            {
                audio.PlayWithDelay(targetTiming, delay);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ResumeImmediate"))
            {
                audio.ResumeImmediately();
            }

            if (GUILayout.Button("ResumeDelay"))
            {
                audio.ResumeWithDelay(delay);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ResumeReturnImmediate"))
            {
                audio.ResumeReturnableImmediately();
            }

            if (GUILayout.Button("ResumeReturnDelay"))
            {
                audio.ResumeReturnableWithDelay(delay);
            }

            GUILayout.EndHorizontal();
        }
    }
}