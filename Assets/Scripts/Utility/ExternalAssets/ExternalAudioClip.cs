using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcCreate.Utilities.ExternalAssets
{
    /// <summary>
    /// Class for handling loading extenal skin as sprites.
    /// </summary>
    public class ExternalAudioClip
    {
        private static readonly string[] Extensions = new string[] { ".wav" };
        private static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>();

        private readonly AudioClip original;
        private readonly string subDirectory;
        private AudioClip external;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalAudioClip"/> class.
        /// </summary>
        /// <param name="original">The original clip.</param>
        /// <param name="subDirectory">The sub directory (relative to Skin directory) to look for the file.</param>
        public ExternalAudioClip(AudioClip original, string subDirectory)
        {
            this.original = original;
            this.subDirectory = subDirectory;
        }

        public AudioClip Value => external != null ? external : original;

        public static void ClearCache()
        {
            Cache.Clear();
        }

        public async UniTask Load()
        {
            foreach (string ext in Extensions)
            {
                string path = string.IsNullOrEmpty(subDirectory) ?
                    Path.Combine(ExternalAssetsCommon.SkinFolderPath, original.name + ext) :
                    Path.Combine(ExternalAssetsCommon.SkinFolderPath, subDirectory, original.name + ext);

                if (!File.Exists(path))
                {
                    continue;
                }

                if (Cache.TryGetValue(path, out AudioClip s))
                {
                    external = s;
                    return;
                }

                using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(
                    Uri.EscapeUriString("file:///" + path.Replace("\\", "/")), AudioType.WAV))
                {
                    await req.SendWebRequest();
                    if (!string.IsNullOrWhiteSpace(req.error))
                    {
                        Debug.LogWarning(I18n.S("Gameplay.Exception.Skin", new Dictionary<string, object>()
                        {
                            { "Path", path },
                            { "Error", req.error },
                        }));
                        return;
                    }

                    AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
                    Cache.Add(path, clip);
                    external = clip;
                    return;
                }
            }
        }

        public void Unload()
        {
            if (external != null)
            {
                UnityEngine.Object.Destroy(external);
                external = null;
            }
        }
    }
}