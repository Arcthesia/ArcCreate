using ArcCreate.Utility.Animation;
using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace ArcCreate.EditorScripts
{
    [CustomEditor(typeof(ScriptedAnimator), true)]
    public class AnimatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var animator = (ScriptedAnimator)target;
            DrawDefaultInspector();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Show animation"))
            {
                animator.SetupComponents();
                RunTween(animator.GetShowTween(out float duration));
            }

            if (GUILayout.Button("Hide animation"))
            {
                animator.SetupComponents();
                RunTween(animator.GetHideTween(out float duration));
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Register default values"))
            {
                animator.SetupComponents();
                animator.RegisterDefaultValues();
            }

            if (GUILayout.Button("Reset to default"))
            {
                animator.SetupComponents();
                animator.RegisterDefaultValues();
            }

            GUILayout.EndHorizontal();
        }

        private void RunTween(Tween tween)
        {
            DOTweenEditorPreview.Stop();
            DOTweenEditorPreview.PrepareTweenForPreview(tween);
            DOTweenEditorPreview.Start();
        }
    }
}