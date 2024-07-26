using System.Collections.Generic;
using System.IO;
using EmmySharp;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using UnityEngine;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmySingleton]
    [EmmyGroup("Macros")]
    public class Persistent
    {
        private static readonly string FilePath = Path.Combine(Application.persistentDataPath, "macro_persistent.json");
        private Dictionary<string, string> data;
        
        private Dictionary<string, string> Data 
        {
            get
            {
                if (data == null)
                {
                    if (File.Exists(FilePath))
                    {
                        data = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(FilePath));
                    }
                    else
                    {
                        data = new Dictionary<string, string>();
                    }
                }

                return data;
            }
        }

        [EmmyDoc("Get the string data for a given key. Returns defaultValue if valid data can not be found.")]
        public string GetString(string key, string defaultValue = null)
        {
            return Data.TryGetValue(key, out string result) ? result : defaultValue;
        }

        [EmmyDoc("Set the string data for a given key.")]
        public void SetString(string key, string value)
        {
            if (Data.ContainsKey(key))
            {
                Data[key] = value;
            }
            else
            {
                Data.Add(key, value);
            }
        }

        [EmmyDoc("Save all configuration. This is also done automatically on application shutdown.")]
        public void Save()
        {
            if (data != null)
            {
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(data));
            }
        }
    }
}