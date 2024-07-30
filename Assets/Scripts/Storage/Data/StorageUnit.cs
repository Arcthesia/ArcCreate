using System;
using System.Collections.Generic;
using System.IO;
using UltraLiteDB;

namespace ArcCreate.Storage.Data
{
    public abstract class StorageUnit<T> : IStorageUnit
        where T : StorageUnit<T>
    {
        [BsonId] public int Id { get; set; }

        public string Identifier { get; set; }

        public int Version { get; set; }

        public DateTime AddedDate { get; set; }

        public abstract string Type { get; }

        public bool IsDefaultAsset { get; set; }

        public virtual void Delete()
        {
            string dir = GetParentDirectory();
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            Database.Current.GetCollection<T>().Delete(Id);
        }

        public IStorageUnit GetConflictingIdentifier()
        {
            return Database.Current.GetCollection<T>().FindOne(Query.EQ("Identifier", Identifier));
        }

        public virtual void Insert()
        {
            Database.Current.GetCollection<T>().Insert(this as T);
        }

        public string GetParentDirectory()
        {
            return Path.Combine(FileStatics.FileStoragePath, Type, Identifier);
        }

        public Option<string> GetRealPath(string relativePath)
        {
            if (relativePath == null)
            {
                return Option<string>.None();
            }

            string realPath = Path.Combine(GetParentDirectory(), relativePath);
            if (!File.Exists(realPath))
            {
                return Option<string>.None();
            }

            return realPath;
        }

        public virtual bool ValidateSelf(out string reason)
        {
            if (!string.IsNullOrEmpty(Identifier))
            {
                reason = "Identifier is empty";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public bool Equals(IStorageUnit other)
        {
            return other != null && Type == other.Type && Id == other.Id;
        }

        public override int GetHashCode()
        {
            int hashCode = -31773061;
            hashCode = (hashCode * -1521134295) + Id.GetHashCode();
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Type);
            return hashCode;
        }
    }
}