using System;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
    [Serializable]
    public struct CameraViewProperty
    {
        public string Name;
        public bool Orthographic;
        public float OrthographicSize;
        public Vector3 Position;
        public Vector3 Rotation;
    }
}