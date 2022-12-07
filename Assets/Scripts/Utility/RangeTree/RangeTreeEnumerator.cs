using System.Collections.Generic;

namespace ArcCreate.Utility.RangeTree
{
    // Modified for zero allocation
    public struct RangeTreeEnumerator<TKey, TValue>
    {
        private static readonly Stack<RangeTreeNode<TKey, TValue>> Stack = new Stack<RangeTreeNode<TKey, TValue>>(32);
        private readonly RangeTreeNode<TKey, TValue> root;
        private readonly TKey from;
        private readonly TKey to;
        private readonly IComparer<TKey> comparer;
        private int index;

        public RangeTreeEnumerator(RangeTreeNode<TKey, TValue> root, TKey from, TKey to, IComparer<TKey> comparer)
        {
            Stack.Clear();
            Stack.Push(root);
            this.root = root;
            this.from = from;
            this.to = to;
            this.comparer = comparer;
            index = -1;
        }

        public TValue Current
        {
            get
            {
                RangeTreeNode<TKey, TValue> currentNode = Stack.Peek();
                if (currentNode == null || currentNode.Items == null || index < 0 || index >= currentNode.Items.Length)
                {
                    throw new System.InvalidOperationException();
                }

                return currentNode.Items[index].Value;
            }
        }

        public bool MoveNext()
        {
            if (Stack.Count == 0)
            {
                return false;
            }

            index++;
            RangeTreeNode<TKey, TValue> node = Stack.Peek();
            while (true)
            {
                if (node.Items == null || index >= node.Items.Length || comparer.Compare(node.Items[index].From, to) > 0)
                {
                    Stack.Pop();
                    if (node.LeftNode != null && comparer.Compare(from, node.Center) < 0)
                    {
                        Stack.Push(node.LeftNode);
                    }

                    if (node.RightNode != null && comparer.Compare(to, node.Center) > 0)
                    {
                        Stack.Push(node.RightNode);
                    }

                    if (Stack.Count == 0)
                    {
                        return false;
                    }

                    node = Stack.Peek();
                    index = 0;
                    continue;
                }
                else if (comparer.Compare(to, node.Items[index].From) >= 0 && comparer.Compare(from, node.Items[index].To) <= 0)
                {
                    break;
                }

                index++;
            }

            return Stack.Count > 0 || index < node.Items.Length;
        }

        public void Reset()
        {
            Stack.Clear();
            Stack.Push(root);
            index = -1;
        }
    }
}