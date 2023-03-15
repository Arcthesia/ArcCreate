using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UltraLiteDB;

namespace ArcCreate.Storage.Data
{
    public abstract class StorageUnit<T> : IStorageUnit
        where T : StorageUnit<T>
    {
        [BsonId] public string Identifier { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string> FileReferences { get; set; }

        public abstract string Type { get; }

        public void Delete()
        {
            foreach (string refr in FileReferences)
            {
                FileStorage.DeleteReference(Path.Combine(Type, Identifier, refr));
            }

            Database.Current.GetCollection<T>().Delete(Identifier);
        }

        public IStorageUnit GetConflictingIdentifier()
        {
            return Database.Current.GetCollection<T>().FindById(Identifier);
        }

        public void Insert()
        {
            Database.Current.GetCollection<T>().Insert(this as T);
        }

        public void Update(IStorageUnit other)
        {
            foreach (string refr in FileReferences)
            {
                FileStorage.DeleteReference(Path.Combine(Type, Identifier, refr));
            }

            T newValue = other as T;
            Database.Current.GetCollection<T>().Update(Identifier, newValue);
        }
    }
}