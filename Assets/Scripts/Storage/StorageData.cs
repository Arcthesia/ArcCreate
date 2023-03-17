using System;
using System.Collections.Generic;
using System.Threading;
using ArcCreate.Data;
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
        private static readonly LRUCache<string, Incompletable<Texture>> JacketCache = new LRUCache<string, Incompletable<Texture>>(50, DestroyTexture);

        [SerializeField] private Texture defaultJacket;

        public event Action OnStorageChange;

        public State<PackStorage> SelectedPack { get; } = new State<PackStorage>();

        public State<(LevelStorage level, ChartSettings chart)> SelectedChart { get; } = new State<(LevelStorage, ChartSettings)>();

        public UltraLiteCollection<LevelStorage> LevelCollection { get; private set; }

        public UltraLiteCollection<PackStorage> PackCollection { get; private set; }

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
            IEnumerable<PackStorage> packs = PackCollection.FindAll();
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

        public async UniTask AssignTexture(RawImage jacket, LevelStorage level, string jacketPath, CancellationToken ct)
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

        public bool TryAssignTextureFromCache(RawImage jacket, LevelStorage level, string jacketPath)
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

        private static void DestroyTexture(Incompletable<Texture> obj)
        {
            Destroy(obj.Value);
        }

        private void Awake()
        {
            SelectedPack.OnValueChange += OnPackChange;
            SelectedChart.OnValueChange += OnChartChange;
        }

        private void OnDestroy()
        {
            SelectedPack.OnValueChange -= OnPackChange;
            SelectedChart.OnValueChange -= OnChartChange;
        }

        private void OnPackChange(PackStorage pack)
        {
            PlayerPrefs.SetString("Selection.LastPack", pack.Identifier);
        }

        private void OnChartChange((LevelStorage level, ChartSettings chart) obj)
        {
            var (level, chart) = obj;
            if (level != null && chart != null)
            {
                PlayerPrefs.SetString($"Selection.LastLevel.{SelectedPack.Value?.Identifier ?? "all"}", level.Identifier);
                PlayerPrefs.SetString("Selection.LastChartPath", chart.ChartPath);
                PlayerPrefs.SetString("Selection.LastDifficultyName", chart.Difficulty);
                PlayerPrefs.SetFloat("Selection.LastCc", (float)chart.ChartConstant);
            }
        }

        private PackStorage GetLastSelectedPack()
        {
            string id = PlayerPrefs.GetString("Selection.LastPack", null);
            if (id == null)
            {
                return null;
            }

            return GetPack(id);
        }

        private (LevelStorage level, ChartSettings chart) GetLastSelectedChart(string packId)
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

                if (lv == null)
                {
                    return (null, null);
                }
            }
            else
            {
                lv = GetLevel(levelId);
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

        private class Incompletable<T>
        {
            public bool Completed { get; set; }

            public T Value { get; set; }

            public bool IsSuccess { get; set; }
        }
    }
}