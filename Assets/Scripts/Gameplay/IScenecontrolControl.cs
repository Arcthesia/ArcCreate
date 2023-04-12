using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Scenecontrol;

namespace ArcCreate.Gameplay
{
    public interface IScenecontrolControl
    {
        bool IsLoaded { get; }

        Scene Scene { get; }

        PostProcessing PostProcessing { get; }

        Context Context { get; }

        string ScenecontrolFolder { get; set; }

        void Clean();

        string Export();

        void Import(string json, IFileAccessWrapper fileAccess = null);

        void WaitForSceneLoad();
    }
}