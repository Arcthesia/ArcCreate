using UnityEngine;

namespace ArcCreate.ChartFormat
{
    public class RawCamera : RawEvent
    {
        public Vector3 Move { get; set; }

        public Vector3 Rotate { get; set; }

        public string CameraType { get; set; }

        public int Duration { get; set; }
    }
}