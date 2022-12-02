// Credit: https://github.com/erdomke/RangeTree
using System;
using System.Collections;
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
    public class RangeTree<TKey, TValue> : IRangeTree<TKey, TValue>
    {
        private RangeTreeNode<TKey, TValue> _root;
        private List<RangeValuePair<TKey, TValue>> _items;
        private bool _isInSync;
        private bool _autoRebuild;
        private IComparer<TKey> _comparer;

        /// <summary>
        /// Whether the tree should be rebuild automatically. Defaults to true.
        /// </summary>
        public bool AutoRebuild
        {
            get { return _autoRebuild; }
            set { _autoRebuild = value; }
        }

        /// <summary>
        /// Count of all items.
        /// </summary>
        public int Count { get { return _items.Count; } }

        /// <summary>
        /// Whether the tree is currently in sync or not. If it is "out of sync"
        /// you can either rebuild it manually (call Rebuild) or let it rebuild
        /// automatically when you query it next.
        /// </summary>
        public bool IsInSync { get { return _isInSync; } }

        public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// Maximum key found in the tree
        /// </summary>
        public TKey Max { get { return _root.Max; } }

        /// <summary>
        /// Minimum key found in the tree
        /// </summary>
        public TKey Min { get { return _root.Min; } }

        /// <summary>
        /// All items of the tree.
        /// </summary>
        public IEnumerable<TValue> Values { get { return _items.Select(i => i.Value); } }

        /// <summary>
        /// Performans a range query.
        /// All items with overlapping ranges are returned.
        /// </summary>
        public IEnumerable<RangeValuePair<TKey, TValue>> this[TKey from, TKey to]
        {
            get
            {
                if (!_isInSync && _autoRebuild)
                    Rebuild();

                return _root.Query(from, to);
            }
        }

        /// <summary>
        /// Performans a "stab" query with a single value.
        /// All items with overlapping ranges are returned.
        /// </summary>
        public IEnumerable<RangeValuePair<TKey, TValue>> this[TKey value]
        {
            get
            {
                if (!_isInSync && _autoRebuild)
                    Rebuild();

                return _root.Query(value);
            }
        }

        /// <summary>
        /// Initializes an empty tree.
        /// </summary>
        public RangeTree() : this(Comparer<TKey>.Default) { }

        /// <summary>
        /// Initializes an empty tree.
        /// </summary>
        public RangeTree(IComparer<TKey> comparer)
        {
            _comparer = comparer;
            _autoRebuild = true;
            Clear();
        }

        /// <summary>
        /// Adds the specified item. Tree will go out of sync.
        /// </summary>
        public void Add(TKey from, TKey to, TValue value)
        {
            if (_comparer.Compare(from, to) == 1)
                throw new ArgumentOutOfRangeException($"{nameof(from)} cannot be larger than {nameof(to)}");

            _isInSync = false;
            _items.Add(new RangeValuePair<TKey, TValue>(from, to, value));
        }

        /// <summary>
        /// Adds the specified item. Tree will go out of sync.
        /// </summary>
        public void Add(RangeValuePair<TKey, TValue> item)
        {
            Add(item.From, item.To, item.Value);
        }

        /// <summary>
        /// Clears the tree (removes all items).
        /// </summary>
        public void Clear()
        {
            _root = new RangeTreeNode<TKey, TValue>(_comparer);
            _items = new List<RangeValuePair<TKey, TValue>>();
            _isInSync = true;
        }

        public bool Contains(RangeValuePair<TKey, TValue> item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(RangeValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<RangeValuePair<TKey, TValue>> GetEnumerator()
        {
            if (!_isInSync && _autoRebuild)
                Rebuild();

            return _items.GetEnumerator();
        }

        /// <summary>
        /// Rebuilds the tree if it is out of sync.
        /// </summary>
        public void Rebuild()
        {
            if (_isInSync)
                return;

            if (_items.Count > 0)
                _root = new RangeTreeNode<TKey, TValue>(_items, _comparer);
            else
                _root = new RangeTreeNode<TKey, TValue>(_comparer);
            _isInSync = true;
            _items.TrimExcess();
        }

        /// <summary>
        /// Removes the specified item. Tree will go out of sync.
        /// </summary>
        public bool Remove(RangeValuePair<TKey, TValue> item)
        {
            var removed = _items.Remove(item);
            _isInSync = _isInSync && !removed;
            return removed;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}