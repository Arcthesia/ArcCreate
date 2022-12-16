
using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Google.MaterialDesign.Icons
{

public class MaterialIconSelectionWindow : EditorWindow
{
	private static readonly Color darkColor = new Color(0.196f, 0.196f, 0.196f);
	private static readonly Color lightColor = new Color(0.804f, 0.804f, 0.804f);

	private int iconSize = 58;
	private int labelHeight = 24;
	private int spacing = 10;
	private bool showNames = true;
	private Vector2 scrollPos = Vector2.zero;
	private string filterText = string.Empty;
	private bool filterGotFocus = false;

	private string selected;
	private string selectedName;
	private bool selectionKeep;
	private System.Action<string> onSelectionChanged;

	private Font MaterialIconsRegular;
	private CodepointData[] codepointsCollection;
	private CodepointData[] filteredCollection;

	private GUIStyle toolbarSeachTextFieldStyle;
	private GUIStyle toolbarSeachCancelButtonStyle;
	private GUIStyle toolbarSeachCancelButtonEmptyStyle;
	private GUIStyle toolbarLabelStyle;
	private GUIStyle iconImageStyle;
	private GUIStyle iconLabelStyle;
	private GUIStyle iconSelectionStyle;

	public void LoadDependencies(Font MaterialIconsRegular)
	{
		showNames = EditorPrefs.GetBool(typeof(MaterialIconSelectionWindow) + ".showNames", true);

		if(MaterialIconsRegular == null)
			return;

		this.MaterialIconsRegular = MaterialIconsRegular;

		string fontPath = AssetDatabase.GetAssetPath(MaterialIconsRegular);
		string codepointsPath = Path.GetDirectoryName(fontPath) + "/codepoints";

		List<CodepointData> tempList = new List<CodepointData>();

		foreach(string codepoint in File.ReadAllLines(codepointsPath))
		{
			string[] data = codepoint.Split(' ');
			tempList.Add(new CodepointData(data[0], data[1]));
		}

		codepointsCollection = tempList.ToArray();
		filteredCollection = codepointsCollection;

		var temp = filteredCollection.FirstOrDefault(data => data.codeGUIContent.text == selected);
		if(temp != null)
			selectedName = temp.name;
	}

	public static void Init(Font MaterialIconsRegular, string preSelect, System.Action<string> callback)
	{
		MaterialIconSelectionWindow window = EditorWindow.GetWindow<MaterialIconSelectionWindow>(true);
		window.selected = preSelect;
		window.onSelectionChanged = callback;
		window.LoadDependencies(MaterialIconsRegular);
	}

	private void OnEnable()
	{
		base.titleContent = new GUIContent("Material Icon Selection");
		base.minSize = new Vector2((iconSize + labelHeight + spacing) * 5f + GUI.skin.verticalScrollbar.fixedWidth + 1f, (iconSize + labelHeight + spacing) * 6f + EditorStyles.toolbar.fixedHeight);
		selectionKeep = true;
	}

	private void OnGUI()
	{
		if((toolbarSeachTextFieldStyle == null) || (iconImageStyle == null))
		{
			toolbarSeachTextFieldStyle = new GUIStyle("ToolbarSeachTextField");
			toolbarSeachCancelButtonStyle = new GUIStyle("ToolbarSeachCancelButton");
			toolbarSeachCancelButtonEmptyStyle = new GUIStyle("ToolbarSeachCancelButtonEmpty");
			toolbarLabelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
			iconSelectionStyle = new GUIStyle("selectionrect");
			iconImageStyle = new GUIStyle() { font = MaterialIconsRegular, fontSize = iconSize - spacing - 10, alignment = TextAnchor.MiddleCenter };
			iconLabelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperCenter, wordWrap = true };
			iconImageStyle.padding = iconLabelStyle.padding = new RectOffset();
			iconImageStyle.normal.textColor = iconLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? lightColor : darkColor;
		}

		if(MaterialIconsRegular == null)
		{
			EditorGUILayout.HelpBox("Could not find \"MaterialIcons-Regular\" font data.", MessageType.Error);
			return;
		}

		if((codepointsCollection == null) || (codepointsCollection.Length == 0))
		{
			EditorGUILayout.HelpBox("Could not find \"codepoints\" font data.", MessageType.Error);
			return;
		}

		OnHeaderGUI();
		OnBodyGUI();
	}

	private void OnHeaderGUI()
	{
		Rect groupRect = new Rect(0f, 0f, base.position.width, EditorStyles.toolbar.fixedHeight);
		GUI.BeginGroup(groupRect);

		if(Event.current.type == EventType.Repaint)
		{
			EditorStyles.toolbar.Draw(groupRect, false, false, false, false);
		}

		Rect filterRect = new Rect(6f, 2f, groupRect.width - 6f - 20f - 64f - 6f, groupRect.height - 2f);
		Rect clearRect = new Rect(filterRect.x + filterRect.width, filterRect.y, 20f, filterRect.height);

		EditorGUI.BeginChangeCheck();

		GUI.SetNextControlName(typeof(MaterialIconSelectionWindow) + ".filterText");
		filterText = EditorGUI.TextField(filterRect, filterText, toolbarSeachTextFieldStyle);

		if(GUI.Button(clearRect, GUIContent.none, string.IsNullOrEmpty(filterText) ? toolbarSeachCancelButtonEmptyStyle : toolbarSeachCancelButtonStyle))
		{
			filterText = string.Empty;
			GUI.FocusControl(null);
		}

		if(!filterGotFocus)
		{
			EditorGUI.FocusTextInControl(typeof(MaterialIconSelectionWindow) + ".filterText");
			filterGotFocus = true;
		}

		if(EditorGUI.EndChangeCheck())
		{
			filteredCollection = codepointsCollection.Where(data => string.IsNullOrEmpty(filterText) || data.nameGUIContent.text.IndexOf(filterText, System.StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
			selectionKeep = true;
		}

		Rect nameRect = new Rect(clearRect.x + clearRect.width, groupRect.y, 64f, groupRect.height);
		EditorGUI.BeginChangeCheck();
		showNames = EditorGUI.Toggle(nameRect, showNames, EditorStyles.toolbarButton);
		if(EditorGUI.EndChangeCheck())
		{
			GUI.FocusControl(null);
			EditorPrefs.SetBool(typeof(MaterialIconSelectionWindow) + ".showNames", showNames);
		}
		EditorGUI.LabelField(nameRect, "Names", toolbarLabelStyle);

		GUI.EndGroup();
	}

	private void OnBodyGUI()
	{
		Rect iconRect = new Rect(0f, 0f, iconSize + labelHeight, iconSize);
		Rect labelRect = new Rect(0f, 0f, iconRect.width, labelHeight);
		if(!showNames)
		{
			iconRect.width -= labelHeight;
			labelRect.height = 0f;
		}
		Rect buttonRect = new Rect(0f, 0f, iconRect.width + spacing, iconRect.height + labelRect.height + spacing);

		Rect groupRect = new Rect(0f, EditorStyles.toolbar.fixedHeight, base.position.width, base.position.height - EditorStyles.toolbar.fixedHeight);
		GUI.BeginGroup(groupRect);

		Rect scrollRect = new Rect(0f, 0f, groupRect.width, groupRect.height);
		int columns = Mathf.FloorToInt((scrollRect.width - GUI.skin.verticalScrollbar.fixedWidth) / (iconRect.width + spacing));
		Rect viewRect = new Rect(0f, 0f, scrollRect.width - GUI.skin.verticalScrollbar.fixedWidth, Mathf.Ceil(filteredCollection.Length / (float) columns) * (iconRect.height + labelRect.height + spacing));

		scrollPos = GUI.BeginScrollView(scrollRect, scrollPos, viewRect);

		for(int i = 0; i < filteredCollection.Length; i += columns)
		{
			for(int j = 0; j < columns; j++)
			{
				if((i + j) >= filteredCollection.Length)
					break;

				var data = filteredCollection[i + j];

				iconRect.x = (j * (iconRect.width + spacing)) + (spacing / 2f);
				iconRect.y = ((i / (float) columns) * (iconRect.height + labelRect.height + spacing)) + (spacing / 2f);

				labelRect.x = iconRect.x;
				labelRect.y = iconRect.y + iconRect.height;

				buttonRect.x = iconRect.x - (spacing / 2f);
				buttonRect.y = iconRect.y - (spacing / 2f);

				if(data.name == selectedName)
				{
					if(Event.current.type == EventType.Repaint)
					{
						iconSelectionStyle.Draw(buttonRect, false, true, true, true);
					}

					if(selectionKeep)
					{
						if(buttonRect.y + buttonRect.height > scrollPos.y + scrollRect.height)
							scrollPos.y = buttonRect.y + buttonRect.height - scrollRect.height;
						else if(buttonRect.y < scrollPos.y)
							scrollPos.y = buttonRect.y;

						selectionKeep = false;
						base.Repaint();
					}
				}

				GUI.Label(iconRect, data.codeGUIContent, iconImageStyle);
				if(showNames)
					GUI.Label(labelRect, data.nameGUIContent, iconLabelStyle);

				if(GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
				{
					GUI.FocusControl(null);
					bool shouldClose = data.codeGUIContent.text == selected;
					selected = data.codeGUIContent.text;
					selectedName = data.name;
					onSelectionChanged.Invoke(selected);
					if(shouldClose)
						base.Close();
				}
			}
		}

		GUI.EndScrollView();
		GUI.EndGroup();

		if(Event.current.type == EventType.KeyDown)
		{
			if(Event.current.keyCode == KeyCode.LeftArrow)
			{
				SelectRelative(-1);
				Event.current.Use();
			}

			if(Event.current.keyCode == KeyCode.RightArrow)
			{
				SelectRelative(+1);
				Event.current.Use();
			}

			if(Event.current.keyCode == KeyCode.UpArrow)
			{
				SelectRelative(-columns);
				Event.current.Use();
			}

			if(Event.current.keyCode == KeyCode.DownArrow)
			{
				SelectRelative(+columns);
				Event.current.Use();
			}

			if(Event.current.keyCode == KeyCode.PageUp)
			{
				SelectRelative(-(columns * 6));
				Event.current.Use();
			}

			if(Event.current.keyCode == KeyCode.PageDown)
			{
				SelectRelative(+(columns * 6));
				Event.current.Use();
			}

			if(Event.current.keyCode == KeyCode.Home)
			{
				SelectAbsolute(0);
				Event.current.Use();
			}

			if(Event.current.keyCode == KeyCode.End)
			{
				SelectAbsolute(filteredCollection.Length - 1);
				Event.current.Use();
			}
		}
	}

	private void SelectRelative(int delta)
	{
		SelectAbsolute(System.Array.FindIndex(filteredCollection, (data) => data.name == selectedName) + delta);
	}

	private void SelectAbsolute(int index)
	{
		index = Mathf.Clamp(index, 0, filteredCollection.Length - 1);

		selected = filteredCollection[index].codeGUIContent.text;
		selectedName = filteredCollection[index].name;
		onSelectionChanged.Invoke(selected);
		selectionKeep = true;
		base.Repaint();
	}

	[System.Serializable]
	public class CodepointData
	{
		public string name { get; private set; }
		public string code { get; private set; }
		public GUIContent nameGUIContent { get; private set; }
		public GUIContent codeGUIContent { get; private set; }

		public CodepointData(string name, string code)
		{
			this.name = name;
			this.code = code;
			this.nameGUIContent = new GUIContent(string.Format("{0} ({1})", name.ToLowerInvariant().Replace('_', ' '), code));
			this.codeGUIContent = new GUIContent(char.ConvertFromUtf32(System.Convert.ToInt32(this.code, 16)), this.nameGUIContent.text);
		}

	}

}

}
