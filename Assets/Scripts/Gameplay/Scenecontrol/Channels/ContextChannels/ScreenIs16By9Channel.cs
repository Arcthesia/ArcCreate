using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ScreenIs16By9Channel : ValueChannel
    {
        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return null;
        }

        public override float ValueAt(int timing)
        {
            Camera cam = Services.Camera.GameplayCamera;
            return (1.77777779f - (1f * cam.pixelWidth / cam.pixelHeight) < 0.1f) ? 1 : 0;
        }
    }
}