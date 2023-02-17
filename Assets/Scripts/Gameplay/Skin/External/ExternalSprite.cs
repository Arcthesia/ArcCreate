using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcCreate.Gameplay.Skin
{
    /// <summary>
    /// Class for handling loading extenal skin as sprites.
    /// </summary>
    public class ExternalSprite
    {
        private static readonly string[] Extensions = new string[] { ".jpg", ".png" };
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        private readonly Sprite original;
        private readonly string subDirectory;
        private readonly bool fullRect;
        private Sprite external;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalSprite"/> class.
        /// </summary>
        /// <param name="original">The original sprite.</param>
        /// <param name="subDirectory">The sub directory (relative to Skin directory) to look for the file.</param>
        /// <param name="fullRect">Whether the sprite should be created with FullRect mesh mode.</param>
        public ExternalSprite(Sprite original, string subDirectory, bool fullRect = false)
        {
            this.original = original;
            this.subDirectory = subDirectory;
            this.fullRect = fullRect;
        }

        public Sprite Value => external != null ? external : original;

        public static void ClearCache()
        {
            Cache.Clear();
        }

        public async UniTask Load()
        {
            foreach (string ext in Extensions)
            {
                string path = string.IsNullOrEmpty(subDirectory) ?
                    Path.Combine(Values.SkinFolderPath, original.name + ext) :
                    Path.Combine(Values.SkinFolderPath, subDirectory, original.name + ext);

                if (!File.Exists(path))
                {
                    continue;
                }

                if (Cache.TryGetValue(path, out Sprite s))
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

                    Sprite sprite;

                    Vector2 pivot = new Vector2(original.pivot.x / original.rect.width, original.pivot.y / original.rect.height);

                    if (!fullRect)
                    {
                        sprite = Sprite.Create(
                            texture: t,
                            rect: new Rect(0, 0, t.width, t.height),
                            pivot: pivot);
                    }
                    else
                    {
                        sprite = Sprite.Create(
                            texture: t,
                            rect: new Rect(0, 0, t.width, t.height),
                            pivot: pivot,
                            pixelsPerUnit: original.pixelsPerUnit,
                            extrude: 1,
                            meshType: SpriteMeshType.FullRect);
                    }

                    Cache.Add(path, sprite);
                    external = sprite;
                    return;
                }
            }
        }

        public void Unload()
        {
            if (external != null)
            {
                UnityEngine.Object.Destroy(external.texture);
                UnityEngine.Object.Destroy(external);
                external = null;
            }
        }
    }
}