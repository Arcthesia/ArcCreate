// Credit: https://github.com/erdomke/RangeTree
// Modified to be more efficient at runtime.
using System.Collections.Generic;
using System.Linq;

namespace ArcCreate.Utility.RangeTree
{
    /// <summary>
    /// A node of the range tree. Given a list of items, it builds
    /// its subtree. Also contains methods to query the subtree.
    /// Basically, all interval tree logic is here.
    /// </summary>
    internal class RangeTreeNode<TKey, TValue> : IComparer<RangeValuePair<TKey, TValue>>
    {
        private static Stack<RangeTreeNode<TKey, TValue>> stack = new Stack<RangeTreeNode<TKey, TValue>>();
        private TKey _center;
        private RangeTreeNode<TKey, TValue> _leftNode;
        private RangeTreeNode<TKey, TValue> _rightNode;
        private RangeValuePair<TKey, TValue>[] _items;

        private readonly IComparer<TKey> _comparer;

        /// <summary>
        /// Initializes an empty node.
        /// </summary>
        /// <param name="comparer">The comparer used to compare two items.</param>
        public RangeTreeNode(IComparer<TKey> comparer)
        {
            _comparer = comparer ?? Comparer<TKey>.Default;

            _center = default(TKey);
            _leftNode = null;
            _rightNode = null;
            _items = null;
        }

        /// <summary>
        /// Initializes a node with a list of items, builds the sub tree.
        /// </summary>
        /// <param name="comparer">The comparer used to compare two items.</param>
        public RangeTreeNode(IList<RangeValuePair<TKey, TValue>> items, IComparer<TKey> comparer)
        {
            _comparer = comparer ?? Comparer<TKey>.Default;

            // first, find the median
            var endPoints = new List<TKey>(items.Count * 2);
            foreach (var item in items)
            {
                endPoints.Add(item.From);
                endPoints.Add(item.To);
            }
            endPoints.Sort(_comparer);

            // the median is used as center value
            _center = endPoints[endPoints.Count / 2];

            var inner = new List<RangeValuePair<TKey, TValue>>();
            var left = new List<RangeValuePair<TKey, TValue>>();
            var right = new List<RangeValuePair<TKey, TValue>>();

            // iterate over all items
            // if the range of an item is completely left of the center, add it to the left items
            // if it is on the right of the center, add it to the right items
            // otherwise (range overlaps the center), add the item to this node's items
            foreach (var o in items)
            {
                if (_comparer.Compare(o.To, _center) < 0)
                    left.Add(o);
                else if (_comparer.Compare(o.From, _center) > 0)
                    right.Add(o);
                else
                    inner.Add(o);
            }

            // sort the items, this way the query is faster later on
            if (inner.Count > 0)
            {
                if (inner.Count > 1)
                    inner.Sort(this);
                _items = inner.ToArray();
            }
            else
            {
                _items = null;
            }

            // create left and right nodes, if there are any items
            if (left.Count > 0)
                _leftNode = new RangeTreeNode<TKey, TValue>(left, _comparer);
            if (right.Count > 0)
                _rightNode = new RangeTreeNode<TKey, TValue>(right, _comparer);
        }



        /// <summary>
        /// Performans a "stab" query with a single value.
        /// All items with overlapping ranges are returned.
        /// </summary>
        public IEnumerable<RangeValuePair<TKey, TValue>> Query(TKey value)
        {
            // If the node has items, check for leaves containing the value.
            if (_items != null)
            {
                foreach (var o in _items)
                {
                    if (_comparer.Compare(o.From, value) > 0)
                        break;
                    else if (_comparer.Compare(value, o.From) >= 0 && _comparer.Compare(value, o.To) <= 0)
                        yield return o;
                }
            }

            // go to the left or go to the right of the tree, depending
            // where the query value lies compared to the center
            var centerComp = _comparer.Compare(value, _center);
            if (_leftNode != null && centerComp < 0)
                foreach (var o in _leftNode.Query(value))
                    yield return o;
            else if (_rightNode != null && centerComp > 0)
                foreach (var o in _rightNode.Query(value))
                    yield return o;
        }

        /// <summary>
        /// Performans a range query.
        /// All items with overlapping ranges are returned.
        /// </summary>
        public IEnumerable<RangeValuePair<TKey, TValue>> Query(TKey from, TKey to)
        {
            // Modified by 0thElement to be more efficient.
            stack.Clear();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (node._items != null)
                {
                    for (int i = 0; i < node._items.Length; i++)
                    {
                        RangeValuePair<TKey, TValue> o = node._items[i];
                        if (_comparer.Compare(o.From, to) > 0)
                            break;
                        else if (_comparer.Compare(to, o.From) >= 0 && _comparer.Compare(from, o.To) <= 0)
                            yield return o;
                    }
                }

                if (node._leftNode != null && _comparer.Compare(from, node._center) < 0)
                    stack.Push(node._leftNode);
                if (node._rightNode != null && _comparer.Compare(to, node._center) > 0)
                    stack.Push(node._rightNode);
            }
        }

        /// <summary>
        /// Returns less than 0 if this range's From is less than the other, greater than 0 if greater.
        /// If both are equal, the comparison of the To values is returned.
        /// 0 if both ranges are equal.
        /// </summary>
        /// <param name="y">The other.</param>
        /// <returns></returns>
        int IComparer<RangeValuePair<TKey, TValue>>.Compare(RangeValuePair<TKey, TValue> x, RangeValuePair<TKey, TValue> y)
        {
            var fromComp = _comparer.Compare(x.From, y.From);
            if (fromComp == 0)
                return _comparer.Compare(x.To, y.To);
            return fromComp;
        }

        public TKey Max
        {
            get
            {
                if (_rightNode != null)
                    return _rightNode.Max;
                else if (_items != null)
                    return _items.Max(i => i.To);
                else
                    return default(TKey);
            }
        }

        public TKey Min
        {
            get
            {
                if (_leftNode != null)
                    return _leftNode.Max;
                else if (_items != null)
                    return _items.Max(i => i.From);
                else
                    return default(TKey);
            }
        }
    }
}