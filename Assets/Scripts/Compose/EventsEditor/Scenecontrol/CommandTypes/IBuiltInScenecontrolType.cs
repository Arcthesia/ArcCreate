namespace ArcCreate.Compose.EventsEditor
{
    public interface IBuiltInScenecontrolType : IScenecontrolType
    {
        string Typename { get; }

        string[] ArgumentNames { get; }
    }
}