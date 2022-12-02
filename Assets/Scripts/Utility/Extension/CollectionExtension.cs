using System;
using System.Collections;
using System.Collections.Generic;

namespace ArcCreate.Utility.Extension
{
    public static class CollectionExtension
    {
        /// <summary>
        /// Whether or not the index is outside the range of the collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="index">Index to check.</param>
        /// <returns>Whether or not the index is out of range.</returns>
        public static bool OutOfRange(this ICollection collection, int index)
        {
            return index < 0 || index > collection.Count - 1;
        }

        /// <summary>
        /// Search for smallest index within a sorted list, whose corresponding item is greater than or equal to the provided value.
        /// Example: for the list [0, 0, 1, 2, 2, 3], searching for 2 will return the index 3.
        /// </summary>
        /// <param name="list">The list to bisect.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="property">Function to extract property <see cref="{R}"/> from items.</param>
        /// <typeparam name="T">Type of the list.</typeparam>
        /// <typeparam name="R">Type of the property to search by.</typeparam>
        /// <returns>The index found, which is always within the index range of the list.</returns>
        public static int BisectLeft<T, R>(this IList<T> list, R value, Func<T, R> property)
            where R : IComparable<R>
        {
            // Copied implementation from python lol
            if (value.CompareTo(property(list[0])) <= 0)
            {
                return 0;
            }

            if (value.CompareTo(property(list[list.Count - 1])) >= 0)
            {
                return list.Count - 1;
            }

            int low = 0;
            int high = list.Count;
            int mid;

            while (low < high)
            {
                mid = (int)((low + high) / 2);
                if (property(list[mid]).CompareTo(value) < 0)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid;
                }
            }

            low = UnityEngine.Mathf.Clamp(low, 0, list.Count - 1);
            return low;
        }

        /// <summary>
        /// Search for largest index within a sorted list, whose corresponding item is less than or equal to the provided value.
        /// Example: for the list [0, 0, 1, 2, 2, 3], searching for 2 will return the index 4.
        /// </summary>
        /// <param name="list">The list to bisect.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="property">Function to extract property <see cref="{R}"/> from items.</param>
        /// <typeparam name="T">Type of the list.</typeparam>
        /// <typeparam name="R">Type of the property to search by.</typeparam>
        /// <returns>The index found, which is always within the index range of the list.</returns>
        public static int BisectRight<T, R>(this IList<T> list, R value, Func<T, R> property)
            where R : IComparable<R>
        {
            // Copied implementation from python lol
            if (value.CompareTo(property(list[0])) <= 0)
            {
                return 0;
            }

            if (value.CompareTo(property(list[list.Count - 1])) >= 0)
            {
                return list.Count - 1;
            }

            int low = 0;
            int high = list.Count;
            int mid;

            while (low < high)
            {
                mid = (int)((low + high) / 2);
                if (value.CompareTo(property(list[mid])) < 0)
                {
                    high = mid;
                }
                else
                {
                    low = mid + 1;
                }
            }

            low = UnityEngine.Mathf.Clamp(low, 0, list.Count - 1);
            return low;
        }
    }
}