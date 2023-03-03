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
                Services.Scenecontrol.ReferencedControllers.Add(score);
                return score;
            }
        }
        [SerializeField] private ImageController jacket;
        public ImageController Jacket
        {
            get
            {
                Services.Scenecontrol.ReferencedControllers.Add(jacket);
                return jacket;
            }
        }
        [SerializeField] private TitleController title;
        public TitleController Title
        {
            get
            {
                Services.Scenecontrol.ReferencedControllers.Add(title);
                return title;
            }
        }
        [SerializeField] private ComposerController composer;
        public ComposerController Composer
        {
            get
            {
                Services.Scenecontrol.ReferencedControllers.Add(composer);
                return composer;
            }
        }
        [SerializeField] private DifficultyController difficultyText;
        public DifficultyController DifficultyText
        {
            get
            {
                Services.Scenecontrol.ReferencedControllers.Add(difficultyText);
                return difficultyText;
            }
        }
        [SerializeField] private ImageController difficultyBackground;
        public ImageController DifficultyBackground
        {
            get
            {
                Services.Scenecontrol.ReferencedControllers.Add(difficultyBackground);
                return difficultyBackground;
            }

#pragma warning restore
        }
    }
}