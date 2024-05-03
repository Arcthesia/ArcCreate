using System;
using System.Collections.Generic;
using System.IO;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility;
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

        private readonly Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();
        private readonly UnorderedList<int> playedTapHitsoundTimings = new UnorderedList<int>(30);
        private readonly UnorderedList<int> playedArcHitsoundTimings = new UnorderedList<int>(30);

        public bool IsLoaded { get; private set; }

        public AudioClip TapHitsoundClip => tapClipLoader.Value;

        public AudioClip ArcHitsoundClip => arcClipLoader.Value;

        public Dictionary<string, AudioClip> SfxAudioClips => sfxClips;

        private bool IsMuted => hitsoundPlayer == null || hitsoundPlayer.Volume < 0.1f;

        public void PlayTapHitsound(int timing)
        {
            if (Services.Audio.IsPlaying && !IsMuted)
            {
                if (playedTapHitsoundTimings.Contains(timing))
                {
                    return;
                }

                playedTapHitsoundTimings.Add(timing);
                hitsoundPlayer?.PlayTap();
            }
        }

        public void PlayArcHitsound(int timing)
        {
            if (Services.Audio.IsPlaying && !IsMuted)
            {
                if (playedArcHitsoundTimings.Contains(timing))
                {
                    return;
                }

                playedArcHitsoundTimings.Add(timing);
                hitsoundPlayer?.PlayArc();
            }
        }

        public void PlayArcTapHitsound(int timing, string sfx, bool isFromJudgement)
        {
            if (Services.Audio.IsPlaying)
            {
                if ((IsMuted ^ isFromJudgement) && !string.IsNullOrEmpty(sfx) && sfxClips.TryGetValue(sfx, out AudioClip clip))
                {
                    musicAudioSource.PlayOneShot(clip);
                }
                else if (!IsMuted && isFromJudgement)
                {
                    PlayArcHitsound(timing);
                }
            }
        }

        public async UniTask LoadCustomSfxs(string parentFolder, IFileAccessWrapper fileAccess)
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

                if (!finalSfx.EndsWith(".wav"))
                {
                    finalSfx = finalSfx + ".wav";
                }

                Uri uri = fileAccess == null ? new Uri(Path.Combine(parentFolder, finalSfx)) : fileAccess.GetFileUri(finalSfx);
                if (uri != null)
                {
                    loadTasks.Add(LoadCustomSfx(sfx, uri));
                }
            }

            await UniTask.WhenAll(loadTasks);
            IsLoaded = true;
        }

        public void UpdateHitsoundHistory(int currentTiming)
        {
            PurgeOldSoundPlayedTimings(currentTiming, playedArcHitsoundTimings, Values.HoldMissLateJudgeWindow);
            PurgeOldSoundPlayedTimings(currentTiming, playedTapHitsoundTimings, Values.HoldMissLateJudgeWindow);
        }

        public void ResetHitsoundHistory()
        {
            playedTapHitsoundTimings.Clear();
            playedArcHitsoundTimings.Clear();
        }

        private async UniTask LoadCustomSfx(string sfx, Uri uri)
        {
            try
            {
                using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.WAV))
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

        private void PurgeOldSoundPlayedTimings(int currentTiming, UnorderedList<int> list, int delay)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] < currentTiming - delay)
                {
                    list.RemoveAt(i);
                }
            }
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
            hitsoundPlayer.Volume = volume;
        }
    }
}