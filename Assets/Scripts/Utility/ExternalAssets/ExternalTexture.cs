using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcCreate.Utility.ExternalAssets
{
    /// <summary>
    /// Class for handling loading external skin as textures.
    /// </summary>
    public class ExternalTexture
    {
        private static readonly string[] Extensions = new string[] { ".jpg", ".png" };
        private static readonly Dictionary<string, Texture> Cache = new Dictionary<string, Texture>();

        private readonly Texture original;
        private readonly string subDirectory;
        private Texture external;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalTexture"/> class.
        /// </summary>
        /// <param name="original">The original texture.</param>
        /// <param name="subDirectory">The sub directory (relative to Skin directory) to look for the file.</param>
        public ExternalTexture(Texture original, string subDirectory)
        {
            this.original = original;
            this.subDirectory = subDirectory;
        }

        public Texture Value => external != null ? external : original;

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

                if (Cache.TryGetValue(path, out Texture s))
                {
                    external = s;
                    return;
                }

                using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(
                    Uri.EscapeUriString("file:///" + path.Replace("\\", "/"))))
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

                    var t = DownloadHandlerTexture.GetContent(req);
                    Cache.Add(path, t);
                    external = t;
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