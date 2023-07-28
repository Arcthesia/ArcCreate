using System.Collections.Generic;

namespace ArcCreate.Utility
{
    /// <summary>
    /// A list that does not keep a consistent order of elements, but has O(1) addition and removal.
    /// </summary>
    /// <remarks>
    /// Removing element is done by swapping the element with the last element, and deleting it.
    /// Therefore, iterating this list while removing elements should be done backward
    /// (i.e from the last element to the first element).
    /// </remarks>
    /// <typeparam name="T">The type of the list elements.</typeparam>
    public readonly struct UnorderedList<T>
    {
        private readonly List<T> list;

        public UnorderedList(int capacity)
        {
            list = new List<T>(capacity);
        }

        public int Count => list.Count;

        public T this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        public void Add(T element)
        {
            list.Add(element);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index > list.Count - 1)
            {
                return;
            }

            list[index] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
        }

        public bool Contains(T element)
        {
            return list.Contains(element);
        }

        public void Clear()
        {
            list.Clear();
        }
    }
}