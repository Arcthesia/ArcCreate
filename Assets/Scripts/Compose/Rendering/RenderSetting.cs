using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ArcCreate.Compose.Rendering
{
    public class RenderSetting
    {
        private const string SettingsPrefKey = "Compose.RenderSettings.DefaultSettings";
        private const string LastPrefKey = "Compose.RenderSettings.LastUsedSetting";

        private static Dictionary<string, RenderSetting> settings = DefaultSettings;
        private static string currentSelected = "hd";

        public static Dictionary<string, RenderSetting> DefaultSettings => new Dictionary<string, RenderSetting>
        {
            {
                "4k", new RenderSetting
                {
                    Crf = 20,
                    Fps = 60,
                    Width = 3840,
                    Height = 2160,
                    MusicVolume = 1,
                    EffectVolume = 0.2f,
                }
            },
            {
                "hd", new RenderSetting
                {
                    Crf = 20,
                    Fps = 60,
                    Width = 1920,
                    Height = 1080,
                    MusicVolume = 1,
                    EffectVolume = 0.2f,
                }
            },
            {
                "sd", new RenderSetting
                {
                    Crf = 30,
                    Fps = 60,
                    Width = 1280,
                    Height = 720,
                    MusicVolume = 1,
                    EffectVolume = 0.2f,
                }
            },
        };

        public static RenderSetting Current => settings[currentSelected];

        public int Crf { get; set; }

        public float Fps { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public float MusicVolume { get; set; }

        public float EffectVolume { get; set; }

        public static void LoadSettings()
        {
            string data = PlayerPrefs.GetString(SettingsPrefKey, null);
            if (data == null)
            {
                settings = DefaultSettings;
            }
            else
            {
                try
                {
                    settings = JsonConvert.DeserializeObject<Dictionary<string, RenderSetting>>(data);
                }
                catch
                {
                    settings = DefaultSettings;
                }
            }

            if (settings == null || settings.Count == 0)
            {
                settings = DefaultSettings;
            }

            currentSelected = PlayerPrefs.GetString(LastPrefKey, "hd");
            if (!settings.ContainsKey(currentSelected))
            {
                currentSelected = "hd";
            }
        }

        public static void SaveSettings()
        {
            string data = JsonConvert.SerializeObject(settings);
            PlayerPrefs.SetString(SettingsPrefKey, data);
            PlayerPrefs.SetString(LastPrefKey, currentSelected);
        }

        public static void SetSelectedSetting(string key)
        {
            currentSelected = key;
        }

        public static void ResetSelectedSetting()
        {
            if (settings.ContainsKey(currentSelected) && DefaultSettings.ContainsKey(currentSelected))
            {
                settings[currentSelected] = DefaultSettings[currentSelected];
            }
        }
    }
}