using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcCreate.Gameplay.Audio
{
    public static class BassHandle
    {
        private static BassStream _tapHitSound;
        private static BassStream _arcHitSound;

        public static async UniTask LoadStream()
        {
            var tapFilePath = Path.Combine(Application.streamingAssetsPath, "Audio", "Tap.wav");
            var arcFilePath = Path.Combine(Application.streamingAssetsPath, "Audio", "Arc.wav");
            var tapFileBytes = await LoadStreamAsync(tapFilePath);
            var arcFileBytes = await LoadStreamAsync(arcFilePath);
            _tapHitSound = new BassStream(tapFileBytes);
            _arcHitSound = new BassStream(arcFileBytes);
        }

        private static async UniTask<byte[]> LoadStreamAsync(string filePath)
        {
            var uri = new Uri(filePath);
            using var request = UnityWebRequest.Get(uri);
            await request.SendWebRequest().ToUniTask();
            return request.downloadHandler.data;
        }
        

        public static void PlayTap()
        {
            _tapHitSound.Play();
        }

        public static void PlayArc()
        {
            _arcHitSound.Play();
        }

        public static void FreeStream()
        {
            _tapHitSound.Dispose();
            _arcHitSound.Dispose();
        }
    }
}