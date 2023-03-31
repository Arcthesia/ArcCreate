using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Data;
using ArcCreate.Gameplay;
using ArcCreate.SceneTransition;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.LRUCache;
using Cysharp.Threading.Tasks;
using UltraLiteDB;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ArcCreate.Storage
{
    [CreateAssetMenu(fileName = "StorageData", menuName = "ScriptableObject/StorageData")]
    public class StorageData : ScriptableObject
    {
        private static readonly LRUCache<string, Incompletable<Texture>> JacketCache = new LRUCache<string, Incompletable<Texture>>(50, DestroyCache);
        private static readonly LRUCache<string, Incompletable<AudioClip>> AudioClipCache = new LRUCache<string, Incompletable<AudioClip>>(10, DestroyCache);
        [SerializeField] private Texture defaultJacket;
        [SerializeField] private GameplayData gameplayData;

        public event Action OnStorageChange;

        public event Action OnSwitchToGameplayScene;

        public event Action OnOpenFilePicker;

        public event Action<Exception> OnSwitchToGameplaySceneException;

        public State<PackStorage> SelectedPack { get; } = new State<PackStorage>();

        public State<(LevelStorage level, ChartSettings chart)> SelectedChart { get; } = new State<(LevelStorage, ChartSettings)>();

        public UltraLiteCollection<LevelStorage> LevelCollection { get; private set; }

        public UltraLiteCollection<PackStorage> PackCollection { get; private set; }

        public bool IsTransitioning { get; private set; }

        public bool IsLoaded => LevelCollection != null && PackCollection != null;

        public LevelStorage GetLevel(string id)
        {
            return LevelCollection.FindOne(Query.EQ("Identifier", id));
        }

        public IEnumerable<LevelStorage> GetAllLevels()
        {
            return LevelCollection.FindAll();
        }

        public void ClearLevels()
        {
            LevelCollection.Delete(Query.All());
        }

        public PackStorage GetPack(string id)
        {
            PackStorage pack = PackCollection.FindOne(Query.EQ("Identifier", id));
            if (pack == null)
            {
                return null;
            }

            FetchLevelsForPack(pack);
            return pack;
        }

        public IEnumerable<PackStorage> GetAllPacks()
        {
            List<PackStorage> packs = PackCollection.FindAll().ToList();
            foreach (var pack in packs)
            {
                FetchLevelsForPack(pack);
            }

            return packs;
        }

        public void ClearPacks()
        {
            PackCollection.Delete(Query.All());
        }

        public void FetchLevelsForPack(PackStorage pack)
        {
            pack.Levels = new List<LevelStorage>();

            foreach (var lvid in pack.LevelIdentifiers)
            {
                LevelStorage lv = GetLevel(lvid);
                if (lv != null)
                {
                    pack.Levels.Add(lv);
                }
            }
        }

        public void NotifyStorageChange()
        {
            LevelCollection = Database.Current.GetCollection<LevelStorage>();
            PackCollection = Database.Current.GetCollection<PackStorage>();

            SelectedPack.SetValueWithoutNotify(GetLastSelectedPack());
            SelectedChart.SetValueWithoutNotify(GetLastSelectedChart(SelectedPack.Value?.Identifier));

            OnStorageChange?.Invoke();
            IsTransitioning = false;
        }

        public void NotifyOpenFilePicker()
        {
            OnOpenFilePicker?.Invoke();
        }

        public async UniTask AssignTexture(RawImage jacket, IStorageUnit level, string jacketPath)
        {
            jacketPath = level.GetRealPath(jacketPath);
            if (jacketPath == null)
            {
                jacket.texture = defaultJacket;
                return;
            }

            Incompletable<Texture> cachedTexture = JacketCache.Get(jacketPath);
            if (cachedTexture != null)
            {
                while (!cachedTexture.Completed)
                {
                    await UniTask.NextFrame();
                }

                if (cachedTexture.IsSuccess)
                {
                    jacket.texture = cachedTexture.Value;
                }

                return;
            }

            Incompletable<Texture> loading = new Incompletable<Texture>();
            JacketCache.Add(jacketPath, loading);
            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture("file:///" + Uri.EscapeDataString(jacketPath.Replace("\\", "/"))))
            {
                await req.SendWebRequest();

                loading.Completed = true;
                if (string.IsNullOrEmpty(req.error))
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(req);
                    jacket.texture = texture;
                    loading.Value = texture;
                    loading.IsSuccess = true;
                }
                else
                {
                    loading.IsSuccess = false;
                }
            }
        }

        public bool TryAssignTextureFromCache(RawImage jacket, IStorageUnit level, string jacketPath)
        {
            jacketPath = level.GetRealPath(jacketPath);
            if (jacketPath == null)
            {
                return false;
            }

            Incompletable<Texture> texture = JacketCache.Get(jacketPath);
            if (texture != null && texture.Completed && texture.IsSuccess && texture.Value != null)
            {
                jacket.texture = texture.Value;
                return true;
            }

            jacket.texture = defaultJacket;
            return false;
        }

        public async UniTask<AudioClip> GetAudioClipStreaming(IStorageUnit level, string audioPath)
        {
            audioPath = level.GetRealPath(audioPath);
            if (audioPath == null)
            {
                return null;
            }

            Incompletable<AudioClip> cachedClip = AudioClipCache.Get(audioPath);
            if (cachedClip != null)
            {
                while (!cachedClip.Completed)
                {
                    await UniTask.NextFrame();
                }

                if (cachedClip.IsSuccess)
                {
                    return cachedClip.Value;
                }

                return null;
            }

            Incompletable<AudioClip> loading = new Incompletable<AudioClip>();
            AudioClipCache.Add(audioPath, loading);
            using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(
                "file:///" + Uri.EscapeDataString(audioPath.Replace("\\", "/")),
                audioPath.EndsWith(".ogg") ? AudioType.OGGVORBIS : AudioType.WAV))
            {
                ((DownloadHandlerAudioClip)req.downloadHandler).streamAudio = true;
                await req.SendWebRequest();

                while (!req.isNetworkError && req.downloadedBytes < 1024)
                {
                    await UniTask.NextFrame();
                }

                loading.Completed = true;
                if (string.IsNullOrEmpty(req.error))
                {
                    AudioClip clip = ((DownloadHandlerAudioClip)req.downloadHandler).audioClip;
                    loading.Value = clip;
                    loading.IsSuccess = true;
                    return clip;
                }
                else
                {
                    loading.IsSuccess = false;
                    return null;
                }
            }
        }

        public void SwitchToPlayScene((LevelStorage level, ChartSettings chart) selection)
        {
            if (IsTransitioning)
            {
                return;
            }

            var (level, chart) = selection;
            SceneTransitionManager.Instance.SetTransition(new ShutterWithInfoTransition());
            IGameplayControl gameplay = null;
            IsTransitioning = true;
            OnSwitchToGameplayScene.Invoke();
            SceneTransitionManager.Instance.SwitchScene(
                SceneNames.GameplayScene,
                async (rep) =>
                {
                    if (rep is IGameplayControl gameplayControl)
                    {
                        await new GameplayLoader(gameplayControl, gameplayData).Load(level, chart);
                        gameplay = gameplayControl;
                    }

                    IsTransitioning = false;
                },
                e =>
                {
                    OnSwitchToGameplaySceneException?.Invoke(e);
                    IsTransitioning = false;
                })
                .ContinueWith(() => gameplay?.Audio.PlayWithDelay(0, 2000));
        }

        public (LevelStorage level, ChartSettings chart) GetLastSelectedChart(string packId)
        {
            string levelId = PlayerPrefs.GetString($"Selection.LastLevel.{packId ?? "all"}", null);
            string chartPath = PlayerPrefs.GetString($"Selection.LastChartPath", null);
            string difficultyName = PlayerPrefs.GetString($"Selection.LastDifficultyName", null);
            double cc = PlayerPrefs.GetFloat($"Selection.LastCc", 0);

            LevelStorage lv = null;
            if (string.IsNullOrEmpty(levelId))
            {
                PackStorage pack = GetPack(packId);
                if (pack == null || pack.Levels.Count <= 0)
                {
                    lv = LevelCollection.FindOne(Query.All());
                }
                else
                {
                    lv = pack.Levels[0];
                }
            }
            else
            {
                lv = GetLevel(levelId);
            }

            if (lv == null)
            {
                return (null, null);
            }

            if (SelectedChart.Value.chart != null)
            {
                foreach (var c in lv.Settings.Charts)
                {
                    if (c.IsSameDifficulty(SelectedChart.Value.chart))
                    {
                        return (lv, c);
                    }
                }

                return (lv, lv.Settings.GetClosestDifficultyToChart(SelectedChart.Value.chart));
            }
            else
            {
                foreach (var c in lv.Settings.Charts)
                {
                    if (c.IsSameDifficulty(chartPath, difficultyName))
                    {
                        return (lv, c);
                    }
                }

                return (lv, lv.Settings.GetClosestDifficultyToConstant(cc));
            }
        }

        public PackStorage GetLastSelectedPack()
        {
            string id = PlayerPrefs.GetString("Selection.LastPack", null);
            if (id == null)
            {
                return null;
            }

            return GetPack(id);
        }

        private static void DestroyCache<T>(Incompletable<T> obj)
            where T : UnityEngine.Object
        {
            Destroy(obj.Value);
        }

        private class Incompletable<T>
        {
            public bool Completed { get; set; }

            public T Value { get; set; }

            public bool IsSuccess { get; set; }
        }
    }
}