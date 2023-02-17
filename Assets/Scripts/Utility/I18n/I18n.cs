using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ArcCreate.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Internationalization.
/// </summary>
public static class I18n
{
    private const string MissingLocale = "Missing locale: ({0}){1}";
    private const string DefaultLocale = "en_us";

    private static Dictionary<string, string> strings = new Dictionary<string, string>();

    public static event Action OnLocaleChanged;

    /// <summary>
    /// Gets the currently active locale.
    /// </summary>
    public static string CurrentLocale { get; private set; } = DefaultLocale;

    private static string LocaleDirectory => Path.Combine(Application.streamingAssetsPath, "Locales");

    /// <summary>
    /// Gets the translated string from a key.
    /// </summary>
    /// <param name="key">The key of the translated string.</param>
    /// <returns>The translated string.</returns>
    public static string S(string key)
    {
        if (strings.Count == 0)
        {
            LoadLocale();
        }

        if (strings.ContainsKey(key))
        {
            return strings[key];
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
            args.TryGetValue(m.Groups[1].Value, out var v) ? v.ToString() : "???");
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
            LoadLocale();
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

    public static async UniTask StartLoadingLocale()
    {
        string path = Path.Combine(LocaleDirectory, CurrentLocale) + ".yml";
        await StartLoadingLocaleWithUnityWeb(path);
    }

    /// <summary>
    /// List all locales available. Each locale is defined by a file in the streaming assets folder.
    /// </summary>
    /// <returns>List of all locales available.</returns>
    public static string[] ListAllLocales()
    {
        DirectoryInfo dir = new DirectoryInfo(LocaleDirectory);
        FileInfo[] yamls = dir.GetFiles("*.yaml");
        FileInfo[] ymls = dir.GetFiles("*.yml");
        return yamls.Select(file => Path.GetFileNameWithoutExtension(file.Name))
            .Union(ymls.Select(file => Path.GetFileNameWithoutExtension(file.Name)))
            .ToArray();
    }

    private static void LoadLocale()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            string path = Path.Combine(LocaleDirectory, CurrentLocale) + ".yml";
            StartLoadingLocaleWithUnityWeb(path).Forget();
        }
        else
        {
            string path = Path.Combine(LocaleDirectory, CurrentLocale) + ".yml";
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            using (FileStream stream = File.OpenRead(path))
            {
                strings.Clear();
                YamlExtractor.ExtractTo(strings, new StreamReader(stream));
            }

            OnLocaleChanged?.Invoke();
        }
    }

    private static async UniTask StartLoadingLocaleWithUnityWeb(string path)
    {
        strings.Clear();
        using (var req = UnityWebRequest.Get(path))
        {
            await req.SendWebRequest();
            if (!string.IsNullOrEmpty(req.error))
            {
                Debug.LogWarning($"Couldn't load locale file at {path}");
                return;
            }

            string data = req.downloadHandler.text;
            YamlExtractor.ExtractTo(strings, new StringReader(data));
        }

        OnLocaleChanged?.Invoke();
    }
}
