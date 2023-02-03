using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

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
        /// Search for smallest index within a sorted list, whose corresponding item is greater than or equal to the provided value.<br/>
        /// Example: for the list [0, 0, 1, 2, 2, 3], searching for 2 will return the index 3.<br/>
        /// The returned index is guaranteed to be within the range of the list.
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
        /// Search for smallest index within a sorted list, whose corresponding item is greater than or equal to the provided value.<br/>
        /// Example: for the list [0, 0, 1, 2, 2, 3], searching for 2 will return the index 3.<br/>
        /// The returned index is guaranteed to be within the range of the list.
        /// </summary>
        /// <param name="list">The list to bisect.</param>
        /// <param name="value">The value to search for.</param>
        /// <typeparam name="T">Type of the list.</typeparam>
        /// <returns>The index found, which is always within the index range of the list.</returns>
        public static int BisectLeft<T>(this IList<T> list, T value)
            where T : IComparable<T>
        {
            // Copied implementation from python lol
            if (value.CompareTo(list[0]) <= 0)
            {
                return 0;
            }

            if (value.CompareTo(list[list.Count - 1]) >= 0)
            {
                return list.Count - 1;
            }

            int low = 0;
            int high = list.Count;
            int mid;

            while (low < high)
            {
                mid = (int)((low + high) / 2);
                if (list[mid].CompareTo(value) < 0)
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
        /// Search for smallest index within a sorted list, whose corresponding item is greater than the provided value.<br/>
        /// Example: for the list [0, 0, 1, 2, 2, 3], searching for 2 will return the index 5.<br/>
        /// If the search value is greater than any item in the list, the list's count value will be returned.<br/>
        /// If the search value is smaller than any item in the list, -1 will be returned.
        /// </summary>
        /// <param name="list">The list to bisect.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="property">Function to extract property <see cref="{R}"/> from items.</param>
        /// <typeparam name="T">Type of the list.</typeparam>
        /// <typeparam name="R">Type of the property to search by.</typeparam>
        /// <returns>The index found.</returns>
        public static int BisectRight<T, R>(this IList<T> list, R value, Func<T, R> property)
            where R : IComparable<R>
        {
            // Copied implementation from python lol
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

            return low;
        }

        /// <summary>
        /// Search for smallest index within a sorted list, whose corresponding item is greater than the provided value.<br/>
        /// Example: for the list [0, 0, 1, 2, 2, 3], searching for 2 will return the index 5.<br/>
        /// If the search value is greater than any item in the list, the list's count value will be returned.<br/>
        /// If the search value is smaller than any item in the list, -1 will be returned.
        /// </summary>
        /// <param name="list">The list to bisect.</param>
        /// <param name="value">The value to search for.</param>
        /// <typeparam name="T">Type of the list.</typeparam>
        /// <returns>The index found.</returns>
        public static int BisectRight<T>(this IList<T> list, T value)
            where T : IComparable<T>
        {
            // Copied implementation from python lol
            int low = 0;
            int high = list.Count;
            int mid;

            while (low < high)
            {
                mid = (int)((low + high) / 2);
                if (value.CompareTo(list[mid]) < 0)
                {
                    high = mid;
                }
                else
                {
                    low = mid + 1;
                }
            }

            return low;
        }

        public static void SetCapacity<T>(this HashSet<T> hs, int capacity)
        {
            HashSetDelegateHolder<T>.InitializeMethod.Invoke(hs, new object[] { capacity });
        }

        public static HashSet<T> GetHashSet<T>(int capacity)
        {
            var hashSet = new HashSet<T>();
            hashSet.SetCapacity(capacity);
            return hashSet;
        }

        public static void SetNumberDigitsToArray(this char[] array, int number, out int length)
        {
            bool isNegative = number < 0;
            number = isNegative ? -number : number;

            int i = array.Length - 1;
            length = 0;
            while (i >= 0 && number > 0)
            {
                array[i] = (char)('0' + (number % 10));
                number /= 10;
                length++;
                i--;
            }

            if (isNegative)
            {
                if (i > 0)
                {
                    length++;
                }
                else
                {
                    i = 0;
                }

                array[i] = '-';
            }
        }

        private static class HashSetDelegateHolder<T>
        {
            private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

            public static MethodInfo InitializeMethod { get; } = typeof(HashSet<T>).GetMethod("Initialize", Flags);
        }
    }
}