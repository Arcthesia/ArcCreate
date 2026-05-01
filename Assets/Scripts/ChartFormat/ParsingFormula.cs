using UnityEngine;

namespace ArcCreate.ChartFormat
{
    public static class ParsingFormula
    {
        /// <summary>
        /// Convert ArcCreate floated lane into Arcaea scale
        /// <br />
        /// See: https://imgur.com/a/uv44Q9j
        /// </summary>
        /// <param name="lane">Lane number in ArcCreate scale</param>
        /// <returns>Lane number in Arcaea scale.</returns>
        public static double LaneToArcaeaFloatedLane(double lane)
        {
            /*
            var worldX = (-Values.LaneWidth * lane) + (Values.LaneWidth * 2.5f);
            var arcX = (worldX - Values.LaneWidth) / -Values.LaneWidth / 2;

            return (arcX + 0.5f) / 2f;
            */

            // this is the simplified expression, which is equivalent to the calculation above
            return lane / 4 - 0.125f;
        }

        public static double ArcaeaFloatedLaneToLane(double arcFloatedLane)
        {
            return arcFloatedLane + 0.125 * 4;
        }

        public static bool IsFloatedLane(float lane, float epsilon = 0.001f)
        {
            return Mathf.Abs(lane % 1) > epsilon;
        }
    }
}