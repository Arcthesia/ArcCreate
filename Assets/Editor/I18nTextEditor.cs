using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(I18nText), true)]
public class I18nTextEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var text = target as I18nText;

        DrawDefaultInspector();
        if (GUILayout.Button("Apply Locale"))
        {
            I18n.SetLocale(I18n.CurrentLocale);
            text.LoadComponent();
            text.ApplyLocale();
        }
    }
}