using System;
using System.Collections.Generic;

namespace ArcCreate.Storage.Data
{
    public interface IStorageUnit
    {
        string Identifier { get; set; }

        DateTime CreatedAt { get; set; }

        List<string> FileReferences { get; set; }

        string Type { get; }

        void Insert();

        void Delete();

        void Update(IStorageUnit other);

        IStorageUnit GetConflictingIdentifier();
    }
}