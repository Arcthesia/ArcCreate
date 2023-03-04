using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class InfoPanelController : ImageController
    {
#pragma warning disable
        [SerializeField] private TextController score;
        public TextController Score
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(score);
                return score;
            }
        }
        [SerializeField] private ImageController jacket;
        public ImageController Jacket
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(jacket);
                return jacket;
            }
        }
        [SerializeField] private TitleController title;
        public TitleController Title
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(title);
                return title;
            }
        }
        [SerializeField] private ComposerController composer;
        public ComposerController Composer
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(composer);
                return composer;
            }
        }
        [SerializeField] private DifficultyController difficultyText;
        public DifficultyController DifficultyText
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(difficultyText);
                return difficultyText;
            }
        }
        [SerializeField] private ImageController difficultyBackground;
        public ImageController DifficultyBackground
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(difficultyBackground);
                return difficultyBackground;
            }

#pragma warning restore
        }
    }
}