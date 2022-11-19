using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles persistence of FCP colors across scenes or gaming sessions
/// </summary>
[RequireComponent(typeof(FlexibleColorPicker))]
public class FCP_Persistence : MonoBehaviour {

    public string saveName = GenerateID();
    public SaveStrategy saveStrategy;

    public enum SaveStrategy {
        SessionOnly, //Do not permanently save, but only for scene loading and some special cases
        File, //save data to a single textfile in persistent data
        PlayerPrefs, //save html strings to individual playerpref slots
    }

    private FlexibleColorPicker fcp;

    private static Dictionary<string, Color> savedColors;
    private static string saveFilePath;
    private static bool saveFileLoaded;
    private static bool saveFileOutdated;

    private void Awake() {
        fcp = GetComponent<FlexibleColorPicker>();
        InitStatic();
    }

    private void InitStatic() {
        if(saveFilePath == null)
            saveFilePath = Path.Combine(Application.persistentDataPath, "FCP_SavedColors.txt");

        if(savedColors == null)
            savedColors = new Dictionary<string, Color>(); 

        if(!saveFileLoaded & saveStrategy == SaveStrategy.File) {
            LoadDataFile();
            saveFileLoaded = true;
        }
    }

    private void OnDestroy() {
        if(saveFileOutdated & saveStrategy == SaveStrategy.File) {
            SaveDataFile();
            saveFileOutdated = false;
        }

    }

    private void OnEnable() {
        if(savedColors == null)
            InitStatic();
        if(LoadColor(out Color c))
            fcp.color = c;
    }

    private void OnDisable() {
        SaveColor(fcp.color);
    }

    private void LoadDataFile() {
        string[] data = File.ReadAllLines(saveFilePath);
        Color c;
        foreach(string d in data) {
            int split = d.LastIndexOf('#');
            if(split >= 0)
            { 
                if(ColorUtility.TryParseHtmlString(d.Substring(split, d.Length - split), out c))
                    savedColors.Add(d.Substring(0, split), c);
            }
        }
    }

    private void SaveDataFile() {
        string[] data = new string[savedColors.Count];
        int i = 0;
        foreach(KeyValuePair<string, Color> pair in savedColors)
            data[i++] = pair.Key + "#" + ColorUtility.ToHtmlStringRGBA(pair.Value);

        File.WriteAllText(saveFilePath, string.Join("\r\n", data));
    }

    public void SaveColor(Color c) {
        if(saveStrategy == SaveStrategy.PlayerPrefs) {
            string pref = "FCP_sc_" + saveName;
            PlayerPrefs.SetString(pref, '#' + ColorUtility.ToHtmlStringRGBA(c));
        }
        else {
            if(savedColors.ContainsKey(saveName))
            {
                saveFileOutdated |= savedColors[saveName] != c;
                savedColors[saveName] = c;
            }
            else
            {
                savedColors.Add(saveName, c);
                saveFileOutdated = true;
            }
        }
    }

    public bool LoadColor(out Color c) {
        c = Color.black;

        if(saveStrategy == SaveStrategy.PlayerPrefs) {
            string pref = "FCP_sc_" + saveName;
            if(!PlayerPrefs.HasKey(pref))
                return false;
            if(!ColorUtility.TryParseHtmlString(PlayerPrefs.GetString(pref), out c))
                return false;
        }
        else {
            if(savedColors.ContainsKey(saveName))
                c = savedColors[saveName];
            else
                return false;
        }
        return true;
    }

    private static string GenerateID() {
        return Convert.ToBase64String(BitConverter.GetBytes(DateTime.Now.Ticks));
    }
}
