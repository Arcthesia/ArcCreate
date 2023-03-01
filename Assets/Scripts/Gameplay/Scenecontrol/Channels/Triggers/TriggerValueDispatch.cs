using System;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public struct TriggerValueDispatch
    {
        public ValueChannel Value;
        public Func<float, float, float, float> Easing;
        public ValueChannel Duration;
        public string EasingString;
    }

    public struct TriggerValueDispatchEvent
    {
        public float Value;
        public Func<float, float, float, float> Easing;
        public int StartTiming;
        public int Duration;
    }
}