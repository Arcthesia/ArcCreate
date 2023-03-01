using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class InfoPanelController : ImageController
    {
#pragma warning disable
        public TextController Score;
        public ImageController Jacket;
        public TitleController Title;
        public ComposerController Composer;
        public DifficultyController DifficultyText;
        public ImageController DifficultyBackground;
#pragma warning restore
    }
}