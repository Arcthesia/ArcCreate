// Credit: https://github.com/erdomke/RangeTree
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcCreate.Utility.RangeTree
{
    /// <summary>
    /// The standard range tree implementation. Keeps a root node and
    /// forwards all queries to it.
    /// Whenenver new items are added or items are removed, the tree
    /// goes "out of sync" and is rebuild when it's queried next.
    /// </summary>
    /// <typeparam name="TKey">The type of the range.</typeparam>
    /// <typeparam name="TValue">The type of the data items.</typeparam>
    public class RangeTree<TKey, TValue>
    {
        private RangeTreeNode<TKey, TValue> root;
        private List<RangeValuePair<TKey, TValue>> items;
        private bool isInSync;
        private readonly IComparer<TKey> comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeTree{TKey, TValue}"/> class.
        /// </summary>
        public RangeTree()
            : this(Comparer<TKey>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeTree{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="comparer">Comparer for two keys.</param>
        public RangeTree(IComparer<TKey> comparer)
        {
            this.comparer = comparer;
            AutoRebuild = true;
            Clear();
        }

        /// <summary>
        /// Gets or sets a value indicating whether whether the tree should be rebuild automatically. Defaults to true.
        /// </summary>
        public bool AutoRebuild { get; set; }

        /// <summary>
        /// Gets count of all items.
        /// </summary>
        public int Count => items.Count;

        /// <summary>
        /// Gets a value indicating whether whether the tree is currently in sync or not. If it is "out of sync"
        /// you can either rebuild it manually (call Rebuild) or let it rebuild
        /// automatically when you query it next.
        /// </summary>
        public bool IsInSync => isInSync;

        /// <summary>
        /// Gets maximum key found in the tree.
        /// </summary>
        public TKey Max => root.Max;

        /// <summary>
        /// Gets minimum key found in the tree.
        /// </summary>
        public TKey Min => root.Min;

        /// <summary>
        /// Gets all items of the tree.
        /// </summary>
        public IEnumerable<TValue> Values => items.Select(i => i.Value);

        /// <summary>
        /// Performans a range query.
        /// All items with overlapping ranges are returned.
        /// </summary>
        /// <param name=from">The lower end of the query.</param>
        /// <param name=to">The upper end of the query.</param>
        public RangeTreeEnumerator<TKey, TValue> this[TKey from, TKey to]
        {
            get
            {
                if (!isInSync && AutoRebuild)
                {
                    Rebuild();
                }

                return new RangeTreeEnumerator<TKey, TValue>(root, from, to, comparer);
            }
        }

        /// <summary>
        /// Adds the specified item. Tree will go out of sync.
        /// </summary>
        /// <param name="from">Lower range of the new item.</param>
        /// <param name="to">Upper range of the new item.</param>
        /// <param name="value">Value of the new item.</param>
        public void Add(TKey from, TKey to, TValue value)
        {
            if (comparer.Compare(from, to) == 1)
            {
                throw new ArgumentOutOfRangeException($"{nameof(from)} cannot be larger than {nameof(to)}");
            }

            isInSync = false;
            items.Add(new RangeValuePair<TKey, TValue>(from, to, value));
        }

        /// <summary>
        /// Adds the specified item. Tree will go out of sync.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(RangeValuePair<TKey, TValue> item)
        {
            Add(item.From, item.To, item.Value);
        }

        /// <summary>
        /// Clears the tree (removes all items).
        /// </summary>
        public void Clear()
        {
            root = new RangeTreeNode<TKey, TValue>(comparer);
            items = new List<RangeValuePair<TKey, TValue>>();
            isInSync = true;
        }

        public bool Contains(RangeValuePair<TKey, TValue> item)
        {
            return items.Contains(item);
        }

        public void CopyTo(RangeValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<RangeValuePair<TKey, TValue>> GetEnumerator()
        {
            if (!isInSync && AutoRebuild)
            {
                Rebuild();
            }

            return items.GetEnumerator();
        }

        /// <summary>
        /// Rebuilds the tree if it is out of sync.
        /// </summary>
        public void Rebuild()
        {
            if (isInSync)
            {
                return;
            }

            if (items.Count > 0)
            {
                root = new RangeTreeNode<TKey, TValue>(items, comparer);
            }
            else
            {
                root = new RangeTreeNode<TKey, TValue>(comparer);
            }

            isInSync = true;
            items.TrimExcess();
        }

        /// <summary>
        /// Removes the specified item. Tree will go out of sync.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>Whether or not the item was successfully removed.</returns>
        public bool Remove(RangeValuePair<TKey, TValue> item)
        {
            var removed = items.Remove(item);
            isInSync = isInSync && !removed;
            return removed;
        }
    }
}