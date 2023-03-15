using System;
using System.Collections.Generic;
using ArcCreate.Storage.Data;
using UltraLiteDB;
using UnityEngine;

namespace ArcCreate.Storage
{
    [CreateAssetMenu(fileName = "StorageData", menuName = "ScriptableObject/StorageData")]
    public class StorageData : ScriptableObject
    {
        public event Action OnStorageChange;

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
            OnStorageChange?.Invoke();
        }
    }
}