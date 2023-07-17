// Credit: https://github.com/erdomke/RangeTree
using System;

namespace ArcCreate.Utility.RangeTree
{
    /// <summary>
    /// The standard range tree implementation. Keeps a root node and
    /// forwards all queries to it.
    /// Whenenver new items are added or items are removed, the tree
    /// goes "out of sync" and is rebuild when it's queried next.
    /// </summary>
    /// <typeparam name="T">The type of the data items.</typeparam>
    public class RangeTree<T>
    {
        private RangeTreeNode<T> root;
        private UnorderedList<RangeValuePair<T>> items = new UnorderedList<RangeValuePair<T>>(0);

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeTree{T}"/> class.
        /// </summary>
        /// <param name="comparer">Comparer for two keys.</param>
        public RangeTree()
        {
            Clear();
        }

        /// <summary>
        /// Gets count of all items.
        /// </summary>
        public int Count => items.Count;

        /// <summary>
        /// Gets the item list.
        /// </summary>
        public UnorderedList<RangeValuePair<T>> Items => items;

        /// <summary>
        /// Performans a range query.
        /// All items with overlapping ranges are returned.
        /// </summary>
        /// <param name=from">The lower end of the query.</param>
        /// <param name=to">The upper end of the query.</param>
        public RangeTreeEnumerator<T> this[double from, double to]
        {
            get
            {
                return new RangeTreeEnumerator<T>(root, from, to);
            }
        }

        /// <summary>
        /// Adds the specified item to a list without updating the tree.
        /// Call <see cref="RangeTree.Rebuild"/> to update the tree.
        /// </summary>
        /// <param name="from">Lower range of the new item.</param>
        /// <param name="to">Upper range of the new item.</param>
        /// <param name="value">Value of the new item.</param>
        public void AddSilent(double from, double to, T value)
        {
            if (from > to)
            {
                throw new ArgumentOutOfRangeException($"{nameof(from)} cannot be larger than {nameof(to)}");
            }

            var pair = new RangeValuePair<T>(from, to, value);
            items.Add(pair);
        }

        /// <summary>
        /// Adds the specified item to the tree.
        /// </summary>
        /// <param name="from">Lower range of the new item.</param>
        /// <param name="to">Upper range of the new item.</param>
        /// <param name="value">Value of the new item.</param>
        public void Add(double from, double to, T value)
        {
            if (from > to)
            {
                throw new ArgumentOutOfRangeException($"{nameof(from)} cannot be larger than {nameof(to)}");
            }

            var pair = new RangeValuePair<T>(from, to, value);
            items.Add(pair);
            root.Add(pair);
        }

        /// <summary>
        /// Clears the tree (removes all items).
        /// </summary>
        public void Clear()
        {
            root = new RangeTreeNode<T>();
            items = new UnorderedList<RangeValuePair<T>>(0);
        }

        /// <summary>
        /// Rebuilds the tree if it is out of sync.
        /// </summary>
        public void Rebuild()
        {
            if (items.Count > 0)
            {
                root = new RangeTreeNode<T>(items);
            }
            else
            {
                root = new RangeTreeNode<T>();
            }
        }

        /// <summary>
        /// Removes the item at the specified index from the tree.
        /// </summary>
        /// <param name="index">The item index to remove.</param>
        public void RemoveAt(int index)
        {
            var item = items[index];
            items.RemoveAt(index);
            root.Remove(item);
        }
    }
}