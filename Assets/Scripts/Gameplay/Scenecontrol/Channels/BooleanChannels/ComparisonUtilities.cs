using System;
using System.Collections.Generic;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public static class ComparisonUtilities
    {
        public static bool Compare<T, C>(this ComparisonType type, C comparer, T a, T b)
            where C : IComparer<T>
        {
            var c = comparer.Compare(a, b);

            switch (type)
            {
                case ComparisonType.Equals: return c == 0;
                case ComparisonType.GreaterThan: return c > 0;
                case ComparisonType.LessThan: return c < 0;
                case ComparisonType.GreaterEqual: return c >= 0;
                case ComparisonType.LessEqual: return c <= 0;

                default: return false;
            }
        }

        public static bool Compare<T>(this ComparisonType type, T a, T b)
            where T : IComparable<T>
        {
            var c = a.CompareTo(b);

            switch (type)
            {
                case ComparisonType.Equals: return c == 0;
                case ComparisonType.GreaterThan: return c > 0;
                case ComparisonType.LessThan: return c < 0;
                case ComparisonType.GreaterEqual: return c >= 0;
                case ComparisonType.LessEqual: return c <= 0;

                default: return false;
            }
        }
    }
}