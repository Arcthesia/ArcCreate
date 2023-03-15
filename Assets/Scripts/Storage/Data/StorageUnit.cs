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
        [BsonId] public int Id { get; set; }

        public string Identifier { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string> FileReferences { get; set; }

        public abstract string Type { get; }

        public void Delete()
        {
            foreach (string refr in FileReferences)
            {
                FileStorage.DeleteReference(string.Join("/", Type, Identifier, refr));
            }

            Database.Current.GetCollection<T>().Delete(Id);
        }

        public IStorageUnit GetConflictingIdentifier()
        {
            return Database.Current.GetCollection<T>().FindOne(Query.EQ("Identifier", Identifier));
        }

        public void Insert()
        {
            Database.Current.GetCollection<T>().Insert(this as T);
        }

        public void Update(IStorageUnit other)
        {
            foreach (string refr in FileReferences)
            {
                FileStorage.DeleteReference(string.Join("/", Type, Identifier, refr));
            }

            T newValue = other as T;
            Database.Current.GetCollection<T>().Update(Id, newValue);
        }

        public string GetRealPath(string virtualPath)
        {
            return FileStorage.GetFilePath(string.Join("/", Type, Identifier, virtualPath));
        }
    }
}