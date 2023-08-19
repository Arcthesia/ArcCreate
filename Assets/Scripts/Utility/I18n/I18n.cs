using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ArcCreate;
using ArcCreate.Utility;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Internationalization.
/// </summary>
public static class I18n
{
    public const string DefaultLocale = "en-us";
    private const string MissingLocale = "Missing locale: ({0}){1}";

    private static readonly Dictionary<string, string> DefaultStrings = new Dictionary<string, string>();

    private static Dictionary<string, string> strings = new Dictionary<string, string>();

    public static event Action OnLocaleChanged;

    /// <summary>
    /// Gets the currently active locale.
    /// </summary>
    public static string CurrentLocale { get; private set; } = DefaultLocale;

    public static List<LocaleEntry> LocaleList { get; private set; } = new List<LocaleEntry>();

    public static string LocaleDirectory => Path.Combine(Application.streamingAssetsPath, "Locales");

    public static string LocaleListPath => Path.Combine(Application.streamingAssetsPath, "locale_list.yml");

    /// <summary>
    /// Runs at startup.
    /// </summary>
    /// <returns>UniTask instance.</returns>
    public static async UniTask Initialize()
    {
        string currentLocale = Settings.Locale.Value;
        await UpdateLocaleList();

        bool found = false;
        foreach (var locale in LocaleList)
        {
            if (locale.Id == currentLocale)
            {
                found = true;
                break;
            }
        }

        currentLocale = found ? currentLocale : string.Empty;

        if (string.IsNullOrEmpty(currentLocale))
        {
            string systemLanguage = Application.systemLanguage.ToString();
            foreach (LocaleEntry locale in LocaleList)
            {
                if (locale.CodeName == systemLanguage)
                {
                    currentLocale = locale.Id;
                    break;
                }
            }
        }

        currentLocale = string.IsNullOrEmpty(currentLocale) ? DefaultLocale : currentLocale;
        CurrentLocale = currentLocale;

        string path = Path.Combine(LocaleDirectory, CurrentLocale) + ".yml";
        if (CurrentLocale != DefaultLocale)
        {
            string defaultPath = Path.Combine(LocaleDirectory, DefaultLocale) + ".yml";
            if (Application.platform == RuntimePlatform.Android)
            {
                await StartLoadingLocaleWithUnityWeb(defaultPath, DefaultStrings, false);
            }
            else
            {
                LoadLocale(defaultPath, DefaultStrings, false);
            }
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            await StartLoadingLocaleWithUnityWeb(path, strings);
        }
        else
        {
            LoadLocale(path, strings);
        }
    }

    /// <summary>
    /// Gets the translated string from a key.
    /// </summary>
    /// <param name="key">The key of the translated string.</param>
    /// <returns>The translated string.</returns>
    public static string S(string key)
    {
        if (strings.Count == 0)
        {
            if (CurrentLocale != DefaultLocale)
            {
                string defaultPath = Path.Combine(LocaleDirectory, DefaultLocale) + ".yml";
                LoadLocale(defaultPath, DefaultStrings, false);
            }

            string path = Path.Combine(LocaleDirectory, CurrentLocale) + ".yml";
            LoadLocale(path, strings);
        }

        if (strings.ContainsKey(key))
        {
            return strings[key];
        }
        else if (DefaultStrings.ContainsKey(key))
        {
            return DefaultStrings[key];
        }
        else
        {
            string alert = string.Format(MissingLocale, CurrentLocale, key);
            Debug.LogWarning(alert);
            return alert;
        }
    }

    /// <summary>
    /// Gets the translated string from a key, and format it with additional arguments.
    /// Original numbered blocks (e.g. {0}, {1}) will be replaced with the provided arguments based on its index.
    /// </summary>
    /// <param name="key">The key of the translated string.</param>
    /// <param name="args">The arguments to format the string.</param>
    /// <returns>The translated and formatted string.</returns>
    public static string S(string key, params object[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            object arg = args[i];
            args[i] = args[i].ToString().Replace(@"\", @"\\");
        }

        return string.Format(S(key), args);
    }

    /// <summary>
    /// Gets the translated string from a key, and format it with additional arguments.
    /// </summary>
    /// <param name="key">The key of the translated string.</param>
    /// <param name="args">The arguments to format the string.</param>
    /// <returns>The translated and formatted string.</returns>
    public static string S(string key, Dictionary<string, object> args)
    {
        return Regex.Replace(S(key), @"{([^}]+)}", m =>
            args.TryGetValue(m.Groups[1].Value, out var v) ? v.ToString().Replace(@"\", @"\\") : "???");
    }

    /// <summary>
    /// Change the locale.
    /// Will revert the change and throw an exception if the locale string is invalid (i.e, the locale file is invalid).
    /// </summary>
    /// <param name="locale">The locale string, which is the same as the file name of the locale file.</param>
    public static void SetLocale(string locale)
    {
        string previousLocale = CurrentLocale;
        var previousStrings = new Dictionary<string, string>(strings);

        CurrentLocale = locale;

        try
        {
            if (CurrentLocale != DefaultLocale)
            {
                string defaultPath = Path.Combine(LocaleDirectory, DefaultLocale) + ".yml";
                LoadLocale(defaultPath, DefaultStrings, false);
            }

            string path = Path.Combine(LocaleDirectory, CurrentLocale) + ".yml";
            LoadLocale(path, strings);
        }
        catch (Exception ex)
        {
            CurrentLocale = previousLocale;
            strings = previousStrings;

            string log = Path.Combine(Application.temporaryCachePath, "error_log.txt");
            string error = $"Locale file loading error\n\n{ex}";
            File.WriteAllText(log, error);
            throw ex;
        }
    }

    public static async UniTask ReportMissingEntries()
    {
        string path = Path.Combine(LocaleDirectory, "report.txt");
        Debug.Log($"Checking for missing localization entries of locale {CurrentLocale}");
        await UniTask.NextFrame();

        using (var stream = File.OpenWrite(path))
        {
            StreamWriter writer = new StreamWriter(stream);
            await writer.WriteLineAsync($"Report missing localization entries for locale {CurrentLocale}");
            await writer.WriteLineAsync("-----");

            int count = 0;
            foreach (var pair in DefaultStrings)
            {
                if (!strings.ContainsKey(pair.Key))
                {
                    await writer.WriteLineAsync($"- Missing entry: \"{pair.Key}\" with content: \"{pair.Value}\"");
                    count++;
                }

                await UniTask.NextFrame();
            }

            await writer.WriteLineAsync("-----");
            await writer.WriteLineAsync($"Found {count} missing entries.");
            writer.Flush();
            writer.Close();
        }

        Debug.Log($"Check complete. Report file was created at {path}");
        Shell.OpenExplorer(path);
    }

    public static string GetLocalName(string id)
    {
        foreach (var locale in LocaleList)
        {
            if (locale.Id == id)
            {
                return locale.LocalName;
            }
        }

        return id;
    }

    private static void LoadLocale(string path, Dictionary<string, string> target, bool invoke = true)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            StartLoadingLocaleWithUnityWeb(path, target, invoke).Forget();
        }
        else
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            using (FileStream stream = File.OpenRead(path))
            {
                target.Clear();
                YamlExtractor.ExtractTo(target, new StreamReader(stream));
            }

            if (invoke)
            {
                OnLocaleChanged?.Invoke();
            }
        }
    }

    private static async UniTask StartLoadingLocaleWithUnityWeb(string path, Dictionary<string, string> target, bool invoke = true)
    {
        target.Clear();
        using (var req = UnityWebRequest.Get(path))
        {
            await req.SendWebRequest();
            if (!string.IsNullOrEmpty(req.error))
            {
                Debug.LogWarning($"Couldn't load locale file at {path}");
                return;
            }

            string data = req.downloadHandler.text;
            YamlExtractor.ExtractTo(target, new StringReader(data));
        }

        if (invoke)
        {
            OnLocaleChanged?.Invoke();
        }
    }

    private static async UniTask UpdateLocaleList()
    {
        string data = string.Empty;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (var req = UnityWebRequest.Get(LocaleListPath))
            {
                await req.SendWebRequest();
                if (!string.IsNullOrEmpty(req.error))
                {
                    Debug.LogWarning($"Couldn't load locale file at {LocaleListPath}");
                    return;
                }

                data = req.downloadHandler.text;
            }
        }
        else
        {
            data = File.ReadAllText(LocaleListPath);
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(new PascalCaseNamingConvention())
            .IgnoreUnmatchedProperties()
            .Build();
        LocaleList = deserializer.Deserialize<List<LocaleEntry>>(data);
    }

    public class LocaleEntry
    {
        public string Id { get; set; }

        public string CodeName { get; set; }

        public string LocalName { get; set; }
    }
}
