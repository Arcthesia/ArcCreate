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
    /// <typeparam name="TKey">Type of node's key.</typeparam>
    /// <typeparam name="TValue">Type of node's value.</typeparam>
    public class RangeTreeNode<TKey, TValue> : IComparer<RangeValuePair<TKey, TValue>>
    {
        private readonly IComparer<TKey> comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeTreeNode{TKey, TValue}"/> class.
        /// The initialized node is empty.
        /// </summary>
        /// <param name="comparer">The comparer used to compare two items.</param>
        public RangeTreeNode(IComparer<TKey> comparer)
        {
            this.comparer = comparer ?? Comparer<TKey>.Default;

            Center = default;
            LeftNode = null;
            RightNode = null;
            Items = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeTreeNode{TKey, TValue}"/> class.
        /// Initializes a node with a list of items, builds the sub tree.
        /// </summary>
        /// <param name="items">The list of items of this node.</param>
        /// <param name="comparer">The comparer used to compare two items.</param>
        public RangeTreeNode(IList<RangeValuePair<TKey, TValue>> items, IComparer<TKey> comparer)
        {
            this.comparer = comparer ?? Comparer<TKey>.Default;

            // first, find the median
            var endPoints = new List<TKey>(items.Count * 2);
            foreach (var item in items)
            {
                endPoints.Add(item.From);
                endPoints.Add(item.To);
            }

            endPoints.Sort(this.comparer);

            // the median is used as center value
            Center = endPoints[endPoints.Count / 2];

            var inner = new List<RangeValuePair<TKey, TValue>>();
            var left = new List<RangeValuePair<TKey, TValue>>();
            var right = new List<RangeValuePair<TKey, TValue>>();

            // iterate over all items
            // if the range of an item is completely left of the center, add it to the left items
            // if it is on the right of the center, add it to the right items
            // otherwise (range overlaps the center), add the item to this node's items
            foreach (var o in items)
            {
                if (this.comparer.Compare(o.To, Center) < 0)
                {
                    left.Add(o);
                }
                else if (this.comparer.Compare(o.From, Center) > 0)
                {
                    right.Add(o);
                }
                else
                {
                    inner.Add(o);
                }
            }

            // sort the items, this way the query is faster later on
            if (inner.Count > 0)
            {
                if (inner.Count > 1)
                {
                    inner.Sort(this);
                }

                this.Items = inner.ToArray();
            }
            else
            {
                this.Items = null;
            }

            // create left and right nodes, if there are any items
            if (left.Count > 0)
            {
                LeftNode = new RangeTreeNode<TKey, TValue>(left, this.comparer);
            }

            if (right.Count > 0)
            {
                RightNode = new RangeTreeNode<TKey, TValue>(right, this.comparer);
            }
        }

        public TKey Center { get; private set; }

        public RangeTreeNode<TKey, TValue> LeftNode { get; private set; }

        public RangeTreeNode<TKey, TValue> RightNode { get; private set; }

        public RangeValuePair<TKey, TValue>[] Items { get; private set; }

        public TKey Max
        {
            get
            {
                if (RightNode != null)
                {
                    return RightNode.Max;
                }
                else if (Items != null)
                {
                    return Items.Max(i => i.To);
                }
                else
                {
                    return default;
                }
            }
        }

        public TKey Min
        {
            get
            {
                if (LeftNode != null)
                {
                    return LeftNode.Max;
                }
                else if (Items != null)
                {
                    return Items.Max(i => i.From);
                }
                else
                {
                    return default;
                }
            }
        }

        /// <summary>
        /// Returns less than 0 if this range's From is less than the other, greater than 0 if greater.
        /// If both are equal, the comparison of the To values is returned.
        /// 0 if both ranges are equal.
        /// </summary>
        /// <param name="x">One node.</param>
        /// <param name="y">Other node.</param>
        /// <returns>Integer value comparing the two node.</returns>
        int IComparer<RangeValuePair<TKey, TValue>>.Compare(RangeValuePair<TKey, TValue> x, RangeValuePair<TKey, TValue> y)
        {
            var fromComp = comparer.Compare(x.From, y.From);
            if (fromComp == 0)
            {
                return comparer.Compare(x.To, y.To);
            }

            return fromComp;
        }
    }
}