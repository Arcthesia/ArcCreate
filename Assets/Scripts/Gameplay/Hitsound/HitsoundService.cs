using System;
using System.Collections.Generic;
using System.IO;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.ExternalAssets;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcCreate.Gameplay.Hitsound
{
    public class HitsoundService : MonoBehaviour, IHitsoundService
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioClip tapClip;
        [SerializeField] private AudioClip arcClip;
        private ExternalAudioClip tapClipLoader;
        private ExternalAudioClip arcClipLoader;
        private IHitsoundPlayer hitsoundPlayer;
        private bool muted = false;

        private readonly Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

        public bool IsLoaded { get; private set; }

        public AudioClip TapHitsoundClip => tapClipLoader.Value;

        public AudioClip ArcHitsoundClip => arcClipLoader.Value;

        public Dictionary<string, AudioClip> SfxAudioClips => sfxClips;

        public void PlayTapHitsound()
        {
            if (Services.Audio.IsPlaying && !muted)
            {
                hitsoundPlayer?.PlayTap();
            }
        }

        public void PlayArcHitsound()
        {
            if (Services.Audio.IsPlaying && !muted)
            {
                hitsoundPlayer?.PlayArc();
            }
        }

        public void PlayArcTapHitsound(string sfx, bool isFromJudgement)
        {
            if (Services.Audio.IsPlaying)
            {
                if ((muted ^ isFromJudgement) && !string.IsNullOrEmpty(sfx) && sfxClips.TryGetValue(sfx, out AudioClip clip))
                {
                    musicAudioSource.PlayOneShot(clip);
                    print("Played clip");
                }
                else if (!muted && isFromJudgement)
                {
                    PlayArcHitsound();
                }
            }
        }

        public async UniTask LoadCustomSfxs(string parentUri)
        {
            IsLoaded = false;

            foreach (var clip in sfxClips.Values)
            {
                Destroy(clip);
            }

            sfxClips.Clear();

            List<UniTask> loadTasks = new List<UniTask>();
            HashSet<string> sfxs = new HashSet<string>();

            IEnumerable<ArcTap> arcTaps = Services.Chart.GetAll<ArcTap>();
            foreach (var at in arcTaps)
            {
                if (!string.IsNullOrEmpty(at.Sfx) && at.Sfx != "none")
                {
                    sfxs.Add(at.Sfx);
                }
            }

            foreach (string sfx in sfxs)
            {
                string finalSfx = sfx;
                if (sfx.EndsWith("_wav"))
                {
                    finalSfx = sfx.Substring(0, sfx.Length - "_wav".Length) + ".wav";
                }

                if (!sfx.EndsWith(".wav"))
                {
                    finalSfx = finalSfx + ".wav";
                }

                string uri = Path.Combine(parentUri, finalSfx);
                loadTasks.Add(LoadCustomSfx(sfx, uri));
            }

            await UniTask.WhenAll(loadTasks);
            IsLoaded = true;
        }

        private async UniTask LoadCustomSfx(string sfx, string uri)
        {
            try
            {
                using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(
                    Uri.EscapeUriString(uri.Replace("\\", "/")), AudioType.WAV))
                {
                    await req.SendWebRequest();
                    if (!string.IsNullOrWhiteSpace(req.error))
                    {
                        return;
                    }

                    AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
                    sfxClips.Add(sfx, clip);
                }
            }
            catch
            {
            }
        }

        private void Awake()
        {
#if (UNITY_ANDROID || UNITY_IOS) && USE_NATIVE_AUDIO && !UNITY_EDITOR
            hitsoundPlayer = new NativeAudioHitsoundPlayer();
#else
            hitsoundPlayer = new UnityHitsoundPlayer(audioSource);
#endif

            Settings.EffectAudio.OnValueChanged.AddListener(OnEffectAudioSettings);
            OnEffectAudioSettings(Settings.EffectAudio.Value);
            LoadExternalClips().Forget();
        }

        private async UniTask LoadExternalClips()
        {
            tapClipLoader = new ExternalAudioClip(tapClip, "AudioClips");
            arcClipLoader = new ExternalAudioClip(arcClip, "AudioClips");

            await UniTask.WhenAll(tapClipLoader.Load(), arcClipLoader.Load());

            hitsoundPlayer.LoadTap(tapClipLoader.Value);
            hitsoundPlayer.LoadArc(arcClipLoader.Value);
            IsLoaded = true;
        }

        private void OnDestroy()
        {
            tapClipLoader.Unload();
            arcClipLoader.Unload();
            hitsoundPlayer.Dispose();
            Settings.EffectAudio.OnValueChanged.RemoveListener(OnEffectAudioSettings);
        }

        private void OnEffectAudioSettings(float volume)
        {
            volume = Mathf.Clamp(volume, 0, 1);
            hitsoundPlayer.SetVolume(volume);
            muted = Mathf.Approximately(volume, 0);
        }
    }
}