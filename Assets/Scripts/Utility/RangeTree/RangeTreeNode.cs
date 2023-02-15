// Credit: https://github.com/erdomke/RangeTree
// Modified to be more efficient at runtime.
using System.Collections.Generic;

namespace ArcCreate.Utility.RangeTree
{
    /// <summary>
    /// A node of the range tree. Given a list of items, it builds
    /// its subtree. Also contains methods to query the subtree.
    /// Basically, all interval tree logic is here.
    /// </summary>
    /// <typeparam name="T">Type of node's value.</typeparam>
    public class RangeTreeNode<T> : IComparer<RangeValuePair<T>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RangeTreeNode{T}"/> class.
        /// The initialized node is empty.
        /// </summary>
        public RangeTreeNode()
        {
            Center = default;
            LeftNode = null;
            RightNode = null;
            Items = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeTreeNode{T}"/> class with one item.
        /// </summary>
        /// <param name="item">The item of this node.</param>
        public RangeTreeNode(RangeValuePair<T> item)
        {
            Center = (item.From + item.To) / 2;
            LeftNode = null;
            RightNode = null;
            Items = new List<RangeValuePair<T>>() { item };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeTreeNode{T}"/> class.
        /// Initializes a node with a list of items, builds the sub tree.
        /// </summary>
        /// <param name="items">The list of items of this node.</param>
        public RangeTreeNode(UnorderedList<RangeValuePair<T>> items)
        {
            // first, find the median
            double avg = 0;
            for (int i = 0; i < items.Count; i++)
            {
                RangeValuePair<T> item = items[i];
                avg += item.From / (items.Count * 2);
                avg += item.To / (items.Count * 2);
            }

            Center = avg;
            Items = new List<RangeValuePair<T>>();
            var left = new UnorderedList<RangeValuePair<T>>(0);
            var right = new UnorderedList<RangeValuePair<T>>(0);

            // iterate over all items
            // if the range of an item is completely left of the center, add it to the left items
            // if it is on the right of the center, add it to the right items
            // otherwise (range overlaps the center), add the item to this node's items
            for (int i = 0; i < items.Count; i++)
            {
                RangeValuePair<T> o = items[i];
                if (o.To < Center)
                {
                    left.Add(o);
                }
                else if (o.From > Center)
                {
                    right.Add(o);
                }
                else
                {
                    Items.Add(o);
                }
            }

            // sort the items, this way the query is faster later on
            Items.Sort(this);
            if (left.Count > 0)
            {
                LeftNode = new RangeTreeNode<T>(left);
            }

            if (right.Count > 0)
            {
                RightNode = new RangeTreeNode<T>(right);
            }
        }

        public double Center { get; private set; }

        public RangeTreeNode<T> LeftNode { get; private set; }

        public RangeTreeNode<T> RightNode { get; private set; }

        public List<RangeValuePair<T>> Items { get; private set; }

        public void Add(RangeValuePair<T> node)
        {
            if (node.To < Center)
            {
                if (LeftNode == null)
                {
                    LeftNode = new RangeTreeNode<T>(node);
                }
                else
                {
                    LeftNode.Add(node);
                }
            }
            else if (node.From > Center)
            {
                if (RightNode == null)
                {
                    RightNode = new RangeTreeNode<T>(node);
                }
                else
                {
                    RightNode.Add(node);
                }
            }
            else
            {
                Items.Add(node);
                Items.Sort(this);
            }
        }

        public bool Remove(RangeValuePair<T> node)
        {
            if (node.To < Center && LeftNode != null)
            {
                LeftNode.Remove(node);
                return true;
            }
            else if (node.From > Center && RightNode != null)
            {
                RightNode.Remove(node);
                return true;
            }
            else
            {
                return Items.Remove(node);
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
        int IComparer<RangeValuePair<T>>.Compare(RangeValuePair<T> x, RangeValuePair<T> y)
        {
            var fromComp = x.From.CompareTo(y.From);
            if (fromComp == 0)
            {
                return x.To.CompareTo(y.To);
            }

            return fromComp;
        }
    }
}