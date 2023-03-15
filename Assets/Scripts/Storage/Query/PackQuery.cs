using System;
using System.Collections.Generic;
using UltraLiteDB;

namespace ArcCreate.Storage.Data
{
    public static class PackQuery
    {
        private static UltraLiteCollection<PackStorage> collection;

        private static UltraLiteCollection<PackStorage> Collection
        {
            get
            {
                collection = collection ?? Database.Current.GetCollection<PackStorage>();
                return collection;
            }
        }

        public static PackStorage Get(string id)
        {
            PackStorage pack = Collection.FindById(id);
            FetchLevelsForPack(pack);
            return pack;
        }

        public static IEnumerable<PackStorage> List()
        {
            IEnumerable<PackStorage> packs = Collection.FindAll();
            foreach (var pack in packs)
            {
                FetchLevelsForPack(pack);
            }

            return packs;
        }

        public static void Clear()
        {
            Collection.Delete(Query.All());
        }

        private static void FetchLevelsForPack(PackStorage pack)
        {
            pack.Levels = new List<LevelStorage>();

            foreach (var lvid in pack.LevelIdentifiers)
            {
                LevelStorage lv = LevelQuery.Get(lvid);
                if (lv != null)
                {
                    pack.Levels.Add(lv);
                }
            }
        }
    }
}