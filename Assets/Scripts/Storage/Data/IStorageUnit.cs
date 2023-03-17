using System;
using System.Collections.Generic;

namespace ArcCreate.Storage.Data
{
    public interface IStorageUnit : IEquatable<IStorageUnit>
    {
        int Id { get; set; }

        string Identifier { get; set; }

        int Version { get; set; }

        List<string> FileReferences { get; set; }

        DateTime AddedDate { get; set; }

        string Type { get; }

        void Insert();

        void Delete();

        IStorageUnit GetConflictingIdentifier();

        bool ValidateSelf(out string reason);
    }
}