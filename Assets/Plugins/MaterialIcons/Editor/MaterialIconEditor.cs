
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Google.MaterialDesign.Icons
{

[CustomEditor(typeof(MaterialIcon), true), CanEditMultipleObjects]
public class MaterialIconEditor : UnityEditor.UI.TextEditor
{
	private static readonly Color darkColor = new Color(0.196f, 0.196f, 0.196f);
	private static readonly Color lightColor = new Color(0.804f, 0.804f, 0.804f);

	private SerializedProperty spText;
	private SerializedProperty spColor;
	private SerializedProperty spRaycastTarget;
	private SerializedProperty spAlignment;

	private MaterialIcon icon;
	private Font MaterialIconsRegular;
	private GUIStyle iconStyle;
	private GUIContent iconTooltip = new GUIContent();
	private GUIContent mixedContent = new GUIContent("\u2014", "Mixed Values");

	protected override void OnEnable()
	{
		base.OnEnable();
		icon = target as MaterialIcon;

		if(string.IsNullOrEmpty(icon.text))
		{
			icon.Init();
		}

		if(icon.font == null)
		{
			icon.LoadFont();
		}

		MaterialIconsRegular = icon.font;

		iconStyle = new GUIStyle();
		iconStyle.font = MaterialIconsRegular;
		iconStyle.fontSize = 42;
		iconStyle.alignment = TextAnchor.MiddleCenter;
		iconStyle.normal.textColor = iconStyle.active.textColor = iconStyle.focused.textColor = iconStyle.hover.textColor = EditorGUIUtility.isProSkin ? lightColor : darkColor;

		iconTooltip.tooltip = icon.iconUnicode;

		spText = serializedObject.FindProperty("m_Text");
		spColor = serializedObject.FindProperty("m_Color");
		spRaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
		spAlignment = serializedObject.FindProperty("m_FontData.m_Alignment");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		if(MaterialIconsRegular == null)
		{
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("Could not find \"MaterialIcons-Regular\" font data.", MessageType.Error);
		}

		EditorGUILayout.Space();

		EditorGUI.BeginDisabledGroup(MaterialIconsRegular == null);

		Rect iconRect = GUILayoutUtility.GetRect(EditorGUIUtility.singleLineHeight * 3f, EditorGUIUtility.singleLineHeight * 3f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
		DoIconControl(iconRect, spText, () => {
			MaterialIconSelectionWindow.Init(MaterialIconsRegular, spText.stringValue, (selected) => {
				spText.stringValue = selected;
				serializedObject.ApplyModifiedProperties();
				iconTooltip.tooltip = icon.iconUnicode;
				Repaint();
			});
		});

		EditorGUI.EndDisabledGroup();

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(spColor);
		EditorGUILayout.PropertyField(spRaycastTarget);

		EditorGUILayout.Space();

		Rect alignmentRect = GUILayoutUtility.GetRect(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
		DoTextAlignmentControl(alignmentRect, spAlignment);

		serializedObject.ApplyModifiedProperties();
	}

	private void DoIconControl(Rect position, SerializedProperty text, System.Action callback)
	{
		bool mixedValues = (text.hasMultipleDifferentValues || ((text.stringValue != null) && (text.stringValue.Length > 1)));
		GUIContent guiContent = new GUIContent("Icon");
		EditorGUI.BeginProperty(position, guiContent, text);
		Rect rect = EditorGUI.PrefixLabel(position, guiContent);
		rect.width = rect.height;
		if(GUI.Button(rect, mixedValues ? mixedContent : iconTooltip))
			callback.Invoke();
		GUI.Label(rect, mixedValues ? string.Empty : text.stringValue, iconStyle);
		EditorGUI.EndProperty();
	}

	/// <summary> Reflection for the private synonymous method from the FontDataDrawer class. </summary>
	private static readonly MethodInfo DoHorizontalAlignmentControl = typeof(UnityEditor.UI.FontDataDrawer).GetMethod("DoHorizontalAligmentControl", BindingFlags.NonPublic | BindingFlags.Static);

	/// <summary> Reflection for the private synonymous method from the FontDataDrawer class. </summary>
	private static readonly MethodInfo DoVerticalAlignmentControl = typeof(UnityEditor.UI.FontDataDrawer).GetMethod("DoVerticalAligmentControl", BindingFlags.NonPublic | BindingFlags.Static);

	/// <summary> Workaround for the non-static private synonymous method from the FontDataDrawer class. </summary>
	private static void DoTextAlignmentControl(Rect position, SerializedProperty alignment)
	{
		try
		{
			GUIContent guiContent = new GUIContent("Alignment");
			EditorGUIUtility.SetIconSize(new Vector2(15f, 15f));
			EditorGUI.BeginProperty(position, guiContent, alignment);
			Rect rect = EditorGUI.PrefixLabel(position, guiContent);
			float size1 = 60f;
			float size2 = Mathf.Clamp(rect.width - size1 * 2f, 2f, 10f);
			Rect position2 = new Rect(rect.x, rect.y, size1, rect.height);
			Rect position3 = new Rect(position2.xMax + size2, rect.y, size1, rect.height);
			DoHorizontalAlignmentControl.Invoke(null, new object[] { position2, alignment });
			DoVerticalAlignmentControl.Invoke(null, new object[] { position3, alignment });
			EditorGUI.EndProperty();
			EditorGUIUtility.SetIconSize(Vector2.zero);
		}
		catch(System.Exception e)
		{
			Debug.LogException(e);
		}
	}

}

}
