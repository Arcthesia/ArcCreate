using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose.EventsEditor
{
    public interface IScenecontrolType
    {
        void ExecuteCommand(ScenecontrolEvent ev);
    }
}