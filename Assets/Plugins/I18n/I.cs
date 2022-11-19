using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum ILocale
{
    zh_Hans,
    en,
    jp,
    zh_TW
}

public class I : MonoBehaviour
{
    private static I Instance { get; set; }
    public const string MISSING_LOCALE = "<MISSING LOCALE>";

    public static ILocale currentLocale = ILocale.en; 
    private JObject langJson;
    private readonly Dictionary<string, string> strings = new Dictionary<string, string>();

    public static UnityEvent OnLocaleChanged = new UnityEvent();

    public void Awake()
    {
        Instance = this;
        currentLocale = (ILocale)PlayerPrefs.GetInt("Locale", 1);
        ReloadLocale();
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetInt("Locale", (int)currentLocale);
    }

    public static string S(string key)
    {
        if (Instance == null || !Instance.strings.ContainsKey(key))
        {
            return MISSING_LOCALE;
        }
        string[] split = key.Split('.');

        JToken obj = Instance.langJson;
        foreach(string s in split)
        {
            obj = obj[s];
        }

        return obj.ToObject<string>();
    }

    public static string S(string key, params object[] args)
    {
        return string.Format(S(key), args);
    }

    private void ReloadLocale()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Locales", currentLocale.ToString()) + ".json";
        if (!File.Exists(path))
            path = Path.Combine(Application.streamingAssetsPath, "Locales", (ILocale.en).ToString()) + ".json";
        try
        {
            langJson = JObject.Parse(File.ReadAllText(path));
        }
        catch (Exception Ex)
        {
            string log = Path.Combine(Application.temporaryCachePath, "error_log.txt");
            string error = "本地化资源文件读取发生错误\n";
            error += "Locale file loading error\n\n";
            error += Ex.ToString();
            File.WriteAllText(log, error);
            Utility.Shell.OpenExplorer(log);
        }
    }

    public void OnLanguageChanged(Dropdown dropdown)
    {
        currentLocale = (ILocale)dropdown.value;
        ReloadLocale();
        OnLocaleChanged.Invoke();
    }

    public void OnOpenLanguageDialog(Dropdown dropdown)
    {
        dropdown.value = (int)currentLocale;
    }
}
