namespace ArcCreate.Gameplay.Scenecontrol
{
    public interface ISceneController : ISerializableUnit
    {
        string SerializedType { get; set; }

        void UpdateController(int timing);

        void CleanController();
    }
}