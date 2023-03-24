using ArcCreate.Storage.Data;

namespace ArcCreate.Selection.Select
{
    public interface ISelectService
    {
        event System.Action OnClear;

        bool IsAnySelected { get; }

        void ClearSelection();

        bool IsStorageSelected(IStorageUnit storageUnit);

        void Add(IStorageUnit storageUnit);

        void Remove(IStorageUnit storageUnit);
    }
}