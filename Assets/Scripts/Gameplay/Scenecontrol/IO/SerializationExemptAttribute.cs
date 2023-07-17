using System;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class SerializationExemptAttribute : Attribute
    {
    }
}