using System.Collections.Generic;

namespace ArcCreate.Utility.RangeTree
{
    // Modified for zero allocation
    public struct RangeTreeEnumerator<T>
    {
        private static readonly Stack<RangeTreeNode<T>> Stack = new Stack<RangeTreeNode<T>>(32);
        private readonly RangeTreeNode<T> root;
        private readonly double from;
        private readonly double to;
        private int index;

        public RangeTreeEnumerator(RangeTreeNode<T> root, double from, double to)
        {
            Stack.Clear();
            Stack.Push(root);
            this.root = root;
            this.from = from;
            this.to = to;
            index = -1;
        }

        public T Current
        {
            get
            {
                RangeTreeNode<T> currentNode = Stack.Peek();
                if (currentNode == null || currentNode.Items == null || index < 0 || index >= currentNode.Items.Count)
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
            RangeTreeNode<T> node = Stack.Peek();
            while (true)
            {
                if (node.Items == null || index >= node.Items.Count || node.Items[index].From > to)
                {
                    Stack.Pop();
                    if (node.LeftNode != null && from < node.Center)
                    {
                        Stack.Push(node.LeftNode);
                    }

                    if (node.RightNode != null && to > node.Center)
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
                else if (to >= node.Items[index].From && from <= node.Items[index].To)
                {
                    break;
                }

                index++;
            }

            return Stack.Count > 0 || index < node.Items.Count;
        }

        public void Reset()
        {
            Stack.Clear();
            Stack.Push(root);
            index = -1;
        }
    }
}