using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ArcCreate.Data;
using ArcCreate.Gameplay;
using ArcCreate.SceneTransition;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.Extension;
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
        private static readonly HashSet<UnityEngine.Object> PersistentCache = new HashSet<UnityEngine.Object>();
        private static readonly HashSet<UnityEngine.Object> QueuedForDelete = new HashSet<UnityEngine.Object>();
        [SerializeField] private Texture defaultJacket;
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private StringSO transitionPlayCount;
        [SerializeField] private StringSO transitionRetryCount;
        private (LevelStorage level, ChartSettings chart) currentGameplayChart;

        public event Action OnStorageChange;

        public event Action OnSwitchToGameplayScene;

        public event Action OnOpenFilePicker;

        public event Action<Exception> OnSwitchToGameplaySceneException;

        public State<PackStorage> SelectedPack { get; } = new State<PackStorage>();

        public State<(LevelStorage level, ChartSettings chart)> SelectedChart { get; } = new State<(LevelStorage, ChartSettings)>();

        public UltraLiteCollection<LevelStorage> LevelCollection { get; private set; }

        public UltraLiteCollection<PackStorage> PackCollection { get; private set; }

        public bool IsTransitioning => SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning;

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
        }

        public void NotifyOpenFilePicker()
        {
            OnOpenFilePicker?.Invoke();
        }

        public async UniTask AssignTexture(RawImage image, IStorageUnit storage, string jacketPath, CancellationToken ct = default)
        {
            Option<string> realJacketPath = storage.GetRealPath(jacketPath);
            if (!realJacketPath.HasValue)
            {
                image.texture = defaultJacket;
                return;
            }

            jacketPath = realJacketPath.Value;
            Incompletable<Texture> cachedTexture = JacketCache.Get(jacketPath);
            if (cachedTexture != null)
            {
                while (!cachedTexture.Completed)
                {
                    await UniTask.NextFrame();
                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }
                }

                if (cachedTexture.IsSuccess)
                {
                    image.texture = cachedTexture.Value;
                }

                return;
            }

            Incompletable<Texture> loading = new Incompletable<Texture>();
            JacketCache.Add(jacketPath, loading);
            Uri uri = new Uri(jacketPath);
            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(uri))
            {
                await req.SendWebRequest();

                loading.Completed = true;
                if (string.IsNullOrEmpty(req.error))
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(req);
                    loading.Value = texture;
                    loading.IsSuccess = true;
                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }

                    image.texture = texture;
                }
                else
                {
                    loading.IsSuccess = false;
                }
            }
        }

        public void EnsurePersistent(UnityEngine.Object obj)
        {
            PersistentCache.Add(obj);
        }

        public void ReleasePersistent(UnityEngine.Object obj)
        {
            PersistentCache.Remove(obj);
            if (QueuedForDelete.Contains(obj))
            {
                Destroy(obj);
                QueuedForDelete.Remove(obj);
            }
        }

        public bool TryAssignTextureFromCache(RawImage jacket, IStorageUnit level, string jacketPath)
        {
            Option<string> realJacketPath = level.GetRealPath(jacketPath);
            if (!realJacketPath.HasValue)
            {
                return false;
            }

            jacketPath = realJacketPath.Value;

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
            Option<string> realAudioPath = level.GetRealPath(audioPath);
            if (!realAudioPath.HasValue)
            {
                return null;
            }

            audioPath = realAudioPath.Value;
            Uri uri = new Uri(audioPath);
            using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(
                uri,
                audioPath.EndsWith(".ogg") ? AudioType.OGGVORBIS : AudioType.WAV))
            {
                ((DownloadHandlerAudioClip)req.downloadHandler).streamAudio = true;
                await req.SendWebRequest();

                while (req.result == UnityWebRequest.Result.ConnectionError && req.downloadedBytes < 1024)
                {
                    await UniTask.NextFrame();
                }

                if (string.IsNullOrEmpty(req.error))
                {
                    AudioClip clip = ((DownloadHandlerAudioClip)req.downloadHandler).audioClip;
                    return clip;
                }
                else
                {
                    return null;
                }
            }
        }

        public void SwitchToPlayScene((LevelStorage level, ChartSettings chart) selection)
        {
            if (SceneTransitionManager.Instance.IsTransitioning)
            {
                return;
            }

            currentGameplayChart = selection;
            var (level, chart) = selection;

            if (gameplayData.EnableAutoplayMode.Value)
            {
                transitionPlayCount.Value = "AUTOPLAY";
                transitionRetryCount.Value = string.Empty;
            }
            else if (gameplayData.EnablePracticeMode.Value)
            {
                transitionPlayCount.Value = "PRACTICE MODE";
                transitionRetryCount.Value = string.Empty;
            }
            else
            {
                PlayHistory history = PlayHistory.GetHistoryForChart(level.Identifier, chart.ChartPath);
                transitionPlayCount.Value = TextFormat.FormatPlayCount(history.PlayCount + 1);
                transitionRetryCount.Value = TextFormat.FormatRetryCount(1);
            }

            TransitionSequence sequence = new TransitionSequence()
                .OnShow()
                .AddTransition(new SoundTransition(TransitionScene.Sound.EnterGameplay))
                .AddTransition(new TriangleTileTransition())
                .AddTransition(new DecorationTransition())
                .AddTransition(new InfoTransition())
                .OnHide()
                .AddTransition(new SoundTransition(TransitionScene.Sound.GameplayLoadComplete))
                .AddTransition(new InfoTransition())
                .AddTransitionReversed(new PlayRetryCountTransition())
                .AddTransition(new PlayRetryCountTransition(), 1200)
                .AddTransition(new TriangleTileTransition(), 1200)
                .AddTransition(new DecorationTransition(), 1200)
                .SetWaitDuration(2000);

            SceneTransitionManager.Instance.SetTransition(sequence);
            IGameplayControl gameplay = null;
            OnSwitchToGameplayScene?.Invoke();
            SceneTransitionManager.Instance.SwitchScene(
                SceneNames.GameplayScene,
                async (rep) =>
                {
                    if (rep is IGameplayControl gameplayControl)
                    {
                        await new GameplayLoader(gameplayControl, gameplayData).Load(level, chart);
                        gameplay = gameplayControl;
                        gameplay.ShouldNotifyOnAudioEnd = true;
                        gameplay.EnablePauseMenu = true;
                        gameplay.Audio.AudioTiming = -Values.DelayBeforeAudioStart;
                        gameplayData.PlaybackSpeed.Value = 1;
                    }
                },
                e =>
                {
                    OnSwitchToGameplaySceneException?.Invoke(e);
                })
                .ContinueWith(() => gameplay?.Audio.PlayWithDelay(0, Values.DelayBeforeAudioStart));

            gameplayData.OnPlayComplete -= OnPlayComplete;
            gameplayData.OnPlayComplete += OnPlayComplete;
        }

        public void SwitchToResultScene(LevelStorage level, ChartSettings chart, PlayResult result, bool isAuto)
        {
            TransitionSequence transition = new TransitionSequence()
                .OnShow()
                .AddTransition(new TriangleTileTransition())
                .OnBoth()
                .AddTransition(new DecorationTransition());
            SceneTransitionManager.Instance.SetTransition(transition);
            SceneTransitionManager.Instance.SwitchScene(
                SceneNames.ResultScene,
                (rep) =>
                {
                    rep.PassData(level, chart, result, isAuto);
                    return default;
                }).Forget();
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
                if (SelectedPack.Value != null)
                {
                    lv = SelectedPack.Value.Levels.First();
                }
                else
                {
                    lv = GetAllLevels().First();
                }
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

                return (lv, lv.Settings.GetClosestDifficultyToConstant(cc, string.Empty));
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
            if (!PersistentCache.Contains(obj.Value))
            {
                Destroy(obj.Value);
            }
            else
            {
                QueuedForDelete.Add(obj.Value);
            }
        }

        private void OnPlayComplete(PlayResult result)
        {
            var (currentLevel, currentChart) = currentGameplayChart;
            PlayHistory history = PlayHistory.GetHistoryForChart(currentLevel.Identifier, currentChart.ChartPath);
            if (!gameplayData.EnableAutoplayMode.Value && !gameplayData.EnablePracticeMode.Value)
            {
                result.BestScore = history.BestScorePlayOrDefault.Score;
                result.PlayCount = history.PlayCount + 1;
                history.AddPlay(result);
                history.Save();
            }

            SwitchToResultScene(currentLevel, currentChart, result, gameplayData.EnableAutoplayMode.Value);
        }

        private class Incompletable<T>
        {
            public bool Completed { get; set; }

            public T Value { get; set; }

            public bool IsSuccess { get; set; }
        }
    }
}