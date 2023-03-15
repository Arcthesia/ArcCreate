using System.Collections.Generic;
using UltraLiteDB;

namespace ArcCreate.Storage.Data
{
    public static class LevelQuery
    {
        private static UltraLiteCollection<LevelStorage> collection;

        private static UltraLiteCollection<LevelStorage> Collection
        {
            get
            {
                collection = collection ?? Database.Current.GetCollection<LevelStorage>();
                return collection;
            }
        }

        public static LevelStorage Get(string id)
        {
            return Collection.FindById(id);
        }

        public static IEnumerable<LevelStorage> List()
        {
            return Collection.FindAll();
        }

        public static void Clear()
        {
            Collection.Delete(Query.All());
        }
    }
}