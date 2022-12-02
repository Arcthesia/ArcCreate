using System.Collections.Generic;

namespace ArcCreate.Gameplay.Data
{
    public class ScenecontrolEvent : ArcEvent
    {
        public string Typename { get; set; }

        public List<object> Arguments { get; set; }

        public override ArcEvent Clone()
        {
            return new ScenecontrolEvent()
            {
                Timing = Timing,
                Typename = Typename,
                Arguments = new List<object>(Arguments),
                TimingGroup = TimingGroup,
            };
        }

        public override void Assign(ArcEvent newValue)
        {
            base.Assign(newValue);
            ScenecontrolEvent sc = newValue as ScenecontrolEvent;
            Typename = sc.Typename;
            Arguments = sc.Arguments;
        }
    }
}
