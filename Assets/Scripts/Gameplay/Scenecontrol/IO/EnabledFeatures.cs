using System;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [Flags]
    public enum EnabledFeatures : long
    {
        None = 0,
        JudgeManipulation = 1,
        All = None | JudgeManipulation,
    }
}