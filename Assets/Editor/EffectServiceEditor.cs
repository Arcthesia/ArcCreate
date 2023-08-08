using ArcCreate.Gameplay.Judgement;
using ArcCreate.Gameplay.Particle;
using UnityEditor;
using UnityEngine;

namespace ArcCreate.EditorScripts
{
    [CustomEditor(typeof(ParticleService))]
    public class EffectServiceEditor : Editor
    {
        private float x;
        private float y;
        private JudgementResult result;

        public override void OnInspectorGUI()
        {
            ParticleService effect = (ParticleService)target;
            DrawDefaultInspector();

            /////
            GUILayout.BeginHorizontal();
            GUILayout.Label("X");
            x = EditorGUILayout.FloatField(x);
            GUILayout.Label("Y");
            y = EditorGUILayout.FloatField(y);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            result = (JudgementResult)EditorGUILayout.Popup("Judge result", (int)result, new string[]
                {
                    "MissEarly",
                    "GoodEarly",
                    "PerfectEarly",
                    "Max",
                    "PerfectLate",
                    "GoodLate",
                    "MissLate",
                });
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Play tap effect"))
            {
                effect.PlayTapParticle(new Vector3(x, y), result);
            }

            if (GUILayout.Button("Play text effect"))
            {
                effect.PlayTextParticle(new Vector3(x, y), result, Option<int>.None());
            }

            GUILayout.EndHorizontal();
        }
    }
}