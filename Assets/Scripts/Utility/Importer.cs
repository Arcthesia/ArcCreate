using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcCreate.Utility
{
    public static class Importer
    {
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        private static readonly Dictionary<string, Texture> TextureCache = new Dictionary<string, Texture>();

        public static async UniTask<Sprite> GetSprite(string path)
        {
            if (SpriteCache.TryGetValue(path, out Sprite spr))
            {
                return spr;
            }

            if (!File.Exists(path))
            {
                return null;
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
                    return null;
                }

                var t = DownloadHandlerTexture.GetContent(req);
                Sprite sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
                SpriteCache.Add(path, sprite);
                return sprite;
            }
        }

        public static async UniTask<Texture> GetTexture(string path)
        {
            if (TextureCache.TryGetValue(path, out Texture txt))
            {
                return txt;
            }

            if (!File.Exists(path))
            {
                return null;
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
                    return null;
                }

                var t = DownloadHandlerTexture.GetContent(req);
                TextureCache.Add(path, t);
                return t;
            }
        }
    }
}