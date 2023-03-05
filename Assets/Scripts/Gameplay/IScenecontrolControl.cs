using ArcCreate.Gameplay.Scenecontrol;

namespace ArcCreate.Gameplay
{
    public interface IScenecontrolControl
    {
        Scene Scene { get; }

        string ScenecontrolFolder { get; set; }

        void Clean();

        string Export();

        void Import(string json);

        void WaitForSceneLoad();
    }
}