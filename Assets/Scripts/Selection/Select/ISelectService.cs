using ArcCreate.Selection.Components;

namespace ArcCreate.Selection.Select
{
    public interface ISelectService
    {
        bool IsAnySelected { get; }

        void ClearSelection();

        void AddComponent(Selectable deletable);

        void RemoveComponent(Selectable deletable);
    }
}