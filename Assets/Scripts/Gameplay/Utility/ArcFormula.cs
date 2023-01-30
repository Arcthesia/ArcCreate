using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay
{
    public static class ArcFormula
    {
        public static float ArcXToWorld(float x)
        {
            return (-Values.LaneWidth * 2 * x) + Values.LaneWidth;
        }

        public static float ArcYToWorld(float y)
        {
            return Values.ArcY0 + ((Values.ArcY1 - Values.ArcY0) * y);
        }

        public static float WorldXToArc(float x)
        {
            return (x - Values.LaneWidth) / -Values.LaneWidth * 2;
        }

        public static float WorldYToArc(float y)
        {
            return (y - Values.ArcY0) / (Values.ArcY1 - Values.ArcY0);
        }

        public static float LaneToWorldX(int lane)
        {
            return (-Values.LaneWidth * lane) + (Values.LaneWidth * 2.5f);
        }

        public static float LaneToArcX(int lane)
        {
            return (0.5f * lane) - 0.75f;
        }

        public static bool WithinRenderRange(float z)
        {
            return z >= -Values.TrackLengthForward && z <= Values.TrackLengthBackward;
        }

        public static int ArcXToLane(float x)
        {
            return Mathf.RoundToInt((x + 0.75f) / 0.5f);
        }

        public static int WorldXToLane(float x)
        {
            return Mathf.RoundToInt((x - (Values.LaneWidth * 2.5f)) / -Values.LaneWidth);
        }

        public static double ZToFloorPosition(float z)
        {
            return (double)(z / Settings.DropRate.Value * Values.BaseBpm * -1000);
        }

        public static float FloorPositionToZ(double fp)
        {
            return (float)(fp / Values.BaseBpm * Settings.DropRate.Value / -1000);
        }

        public static float S(float start, float end, float t)
        {
            return ((1 - t) * start) + (end * t);
        }

        public static float O(float start, float end, float t)
        {
            return start + ((end - start) * (1 - Mathf.Cos(1.5707963f * t)));
        }

        public static float I(float start, float end, float t)
        {
            return start + ((end - start) * Mathf.Sin(1.5707963f * t));
        }

        public static float B(float start, float end, float t)
        {
            float o = 1 - t;
            return (Mathf.Pow(o, 3) * start)
                 + (3 * Mathf.Pow(o, 2) * t * start)
                 + (3 * o * Mathf.Pow(t, 2) * end)
                 + (Mathf.Pow(t, 3) * end);
        }

        public static float X(float start, float end, float t, ArcLineType type)
        {
            switch (type)
            {
                default:
                case ArcLineType.S:
                    return S(start, end, t);
                case ArcLineType.B:
                    return B(start, end, t);
                case ArcLineType.Si:
                case ArcLineType.SiSi:
                case ArcLineType.SiSo:
                    return I(start, end, t);
                case ArcLineType.So:
                case ArcLineType.SoSi:
                case ArcLineType.SoSo:
                    return O(start, end, t);
            }
        }

        public static float Y(float start, float end, float t, ArcLineType type)
        {
            switch (type)
            {
                default:
                case ArcLineType.S:
                case ArcLineType.Si:
                case ArcLineType.So:
                    return S(start, end, t);
                case ArcLineType.B:
                    return B(start, end, t);
                case ArcLineType.SiSi:
                case ArcLineType.SoSi:
                    return I(start, end, t);
                case ArcLineType.SiSo:
                case ArcLineType.SoSo:
                    return O(start, end, t);
            }
        }

        public static float Qi(float value)
        {
            return value * value * value;
        }

        public static float Qo(float value)
        {
            value--;
            return (value * value * value) + 1;
        }

        public static List<int> CalculateLongNoteJudgeTimings(int from, int to, float bpm)
        {
            List<int> result = new List<int>();

            int u = 0;
            bpm = Mathf.Abs(bpm);
            float interval = 60000f / bpm / (bpm >= 255 ? 1 : 2) / Values.TimingPointDensity;
            int total = (int)((to - from) / interval);
            if ((u ^ 1) >= total)
            {
                result.Add((int)(from + ((to - from) * 0.5f)));
                return result;
            }

            int n = u ^ 1;
            while (true)
            {
                int t = (int)(from + (n * interval));
                if (t < to)
                {
                    result.Add(t);
                }

                if (total == ++n)
                {
                    break;
                }
            }

            return result;
        }

        public static float CalculateTapSizeScalar(float z)
        {
            if (z <= 0)
            {
                return Mathf.Abs(1.5f + (3.25f * -z / Values.TrackLengthForward));
            }

            return Mathf.Abs(1.5f + (3.25f * z / Values.TrackLengthBackward));
        }

        public static float CalculateBeatlineSizeScalar(float thickness, float z)
        {
            if (z <= 0)
            {
                return Mathf.Abs(thickness + (thickness * 3 * -z / Values.TrackLengthForward));
            }

            return Mathf.Abs(thickness + (thickness * 3 * z / Values.TrackLengthBackward));
        }

        public static float CalculateFadeOutAlpha(float z)
        {
            if (z <= 0)
            {
                return Mathf.Clamp((Values.TrackLengthForward + z) / Values.NoteFadeOutLength, 0, 1);
            }

            return Mathf.Clamp((Values.TrackLengthBackward - z) / Values.NoteFadeOutLength, 0, 1);
        }

        public static int CalculateArcLockDuration(float arcJudgeInterval)
        {
            return Mathf.Min(Mathf.RoundToInt(arcJudgeInterval * 4), 1000);
        }

        public static float CalculateArcSegmentLength(int duration)
        {
            float length = Values.ArcSegmentLength;
            return duration < 1000 ? length : length * 2;
        }
    }
}