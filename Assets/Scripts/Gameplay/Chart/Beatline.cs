using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    public class Beatline
    {
        private BeatlineBehaviour instance;
        private readonly int timing;
        private readonly double floorPosition;
        private readonly float thickness;
        private readonly Color color;

        public Beatline(int timing, double floorPosition, float thickness, Color color)
        {
            this.timing = timing;
            this.floorPosition = floorPosition;
            this.color = color;
            this.thickness = thickness;
        }

        public int Timing => timing;

        public bool IsAssignedInstance => instance != null;

        public double FloorPosition => floorPosition;

        public void AssignInstance(BeatlineBehaviour behaviour)
        {
            instance = behaviour;
        }

        public BeatlineBehaviour RevokeInstance()
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
                instance.transform.localPosition = new Vector3(0, 0, z);
                instance.transform.localScale = new Vector3(
                    instance.transform.localScale.x,
                    ArcFormula.CalculateBeatlineSizeScalar(thickness, z),
                    1);
            }

            instance.SetColor(color);
        }
    }
}