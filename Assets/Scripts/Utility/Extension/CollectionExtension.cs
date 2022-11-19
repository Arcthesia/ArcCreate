using System;
using System.Collections;
using System.Collections.Generic;

namespace Utility.Extension
{
    public static class CollectionExtension
    {
        public static bool OutOfRange(this ICollection collection, int index)
        {
            return index < 0 || index > collection.Count - 1;
        }

        public static int BinarySearchNearest<T, R>(this IList<T> list, R value, Func<T, R> property)
            where R : IComparable<R>
        {
            if (value.CompareTo(property(list[0])) <= 0)
            {
                return 0;
            }

            if (value.CompareTo(property(list[list.Count - 1])) >= 0)
            {
                return list.Count - 1;
            }

            int index;

            int first = 0;
            int last = list.Count - 1;
            int mid = 0;
            R midValue = property(list[mid]);

            while (first < last - 1)
            {
                mid = (first + last) / 2;
                midValue = property(list[mid]);
                if (value.CompareTo(midValue) == 0)
                {
                    index = mid;
                    break;
                }
                else if (value.CompareTo(midValue) < 0)
                {
                    last = mid;
                }
                else
                {
                    first = mid;
                }
            }

            if (midValue.CompareTo(value) <= 0)
            {
                index = mid;
            }
            else
            {
                index = mid - 1;
            }

            index = UnityEngine.Mathf.Clamp(index, 0, list.Count - 1);
            return index;
        }
    }
}