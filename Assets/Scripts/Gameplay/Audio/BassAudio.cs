using System;
using UnityEngine;
using ManagedBass;

namespace ArcCreate.Gameplay.Audio
{
    public class BassAudio: IDisposable
    {
        private static readonly Lazy<BassAudio> _instance = new(() => new BassAudio());
        public static BassAudio Instance => _instance.Value;
        private bool initialized;

        private BassAudio()
        {
            {
                Initialize();
                Application.quitting += OnApplicationQuit;
            }
        }
        
        private void Initialize()
        {
            if (initialized) return;

            if (Bass.Init())
            {
                initialized = true;
                Debug.Log("BASS_INIT");
            }
            else
            {
                Debug.LogError(Bass.LastError);
            }
        }
        
        private void OnApplicationQuit()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            if (!initialized) return;
            BassHandle.FreeStream();
            Bass.Free();
            initialized = false;
        }
    }
}