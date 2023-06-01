using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class NoteTypeChannel : StringChannel
    {
        public NoteTypeChannel()
        {
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>();
        }

        [return: EmmyType(@"""tap"" | ""hold"" | ""arctap"" | ""arc""")]
        public override string ValueAt(int timing)
        {
            switch (NoteIndividualController.CurrentNote)
            {
                case Tap t:
                    return "tap";
                case Hold h:
                    return "hold";
                case ArcTap at:
                    return "arctap";
                case Arc a:
                    return "arc";
            }

            return null;
        }
    }
}