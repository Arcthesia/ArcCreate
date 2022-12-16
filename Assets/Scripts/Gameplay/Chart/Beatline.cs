using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    public class Beatline
    {
        private Transform instance;
        private readonly double floorPosition;

        public Beatline(double floorPosition)
        {
            this.floorPosition = floorPosition;
        }

        public bool IsAssignedInstance => instance != null;

        public double FloorPosition => floorPosition;

        public void AssignInstance(Transform transform)
        {
            instance = transform;
        }

        public Transform RevokeInstance()
        {
            var result = instance;
            instance = null;
            return result;
        }

        public void UpdateInstance(double floorPosition)
        {
            if (instance != null)
            {
                float z = ArcFormula.FloorPositionToZ(FloorPosition - floorPosition);
                instance.localPosition = new Vector3(0, 0, z);
                instance.localScale = new Vector3(
                    instance.localScale.x,
                    ArcFormula.CalculateBeatlineSizeScalar(z),
                    1);
            }
        }
    }
}