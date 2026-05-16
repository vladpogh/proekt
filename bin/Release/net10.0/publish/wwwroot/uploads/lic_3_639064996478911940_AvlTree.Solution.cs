using System;

namespace AVL
{
    public class AvlTree<T> : IAvlTree<T> 
        where T : IComparable<T>
    {
        public Node<T> Root { get; private set; }

        public void Delete(T key)
        {
            if (this.Root == null)
            {
                return;
            }

            this.Root = this.Delete(this.Root, key);
        }

                private Node<T> Delete(Node<T> node, T key)
        {
            if (node == null)
            {
                return null;
            }

            int cmp = key.CompareTo(node.Value);

            if (cmp < 0)
            {
                node.Left = this.Delete(node.Left, key);
            }
            else if (cmp > 0)
            {
                node.Right = this.Delete(node.Right, key);
            }
            else
            {
                // Node found: handle cases with 0/1 child
                if (node.Left == null || node.Right == null)
                {
                    node = node.Left ?? node.Right; // may become null
                }
                else
                {
                    // Two children: replace with inorder successor (smallest in right subtree)
                    Node<T> successor = node.Right;
                    while (successor.Left != null)
                    {
                        successor = successor.Left;
                    }

                    node.Value = successor.Value;
                    node.Right = this.Delete(node.Right, successor.Value);
                }
            }

            // If subtree is now empty
            if (node == null)
            {
                return null;
            }

            // Rebalance and update height on the way up
            node = this.Balance(node);
            this.UpdateHeight(node);

            return node;
        }

        // IMPLEMENTED METHODS

        public void EachInOrder(Action<T> action)
        {
            this.EachInOrder(action, this.Root);
        }

        public void EachInOrder(Action<T> action, Node<T> node)
        {
            if (node == null)
            {
                return;
            }

            this.EachInOrder(action, node.Left);
            action(node.Value);
            this.EachInOrder(action, node.Right);
        }

        public void Insert(T element)
        {
            this.Root = this.Insert(this.Root, element);
        }

        private Node<T> Insert(Node<T> node, T element)
        {
            if (node == null)
            {
                return new Node<T>(element);
            }

            if (element.CompareTo(node.Value) < 0)
            {
                node.Left = this.Insert(node.Left, element);
            }
            else if (element.CompareTo(node.Value) > 0)
            {
                node.Right = this.Insert(node.Right, element);
            }

            node = this.Balance(node);
            this.UpdateHeight(node);

            return node;
        }

        private Node<T> Balance(Node<T> node)
        {
            int balance = this.Height(node.Left) - this.Height(node.Right);
            if (balance > 1)
            {
                int childBalance = this.Height(node.Left.Left) - this.Height(node.Left.Right);
                if (childBalance < 0)
                {
                    node.Left = this.RotateLeft(node.Left);
                }

                node = this.RotateRight(node);
            }
            else if (balance < -1)
            {
                int childBalance = Height(node.Right.Left) - Height(node.Right.Right);
                if (childBalance > 0)
                {
                    node.Right = RotateRight(node.Right);
                }

                node = RotateLeft(node);
            }

            return node;
        }

        private void UpdateHeight(Node<T> node)
        {
            node.Height = Math.Max(Height(node.Left), Height(node.Right)) + 1;
        }

        private int Height(Node<T> node)
        {
            return node?.Height ?? 0;
        }

        private Node<T> RotateRight(Node<T> node)
        {
            Node<T> left = node.Left;
            node.Left = left.Right;
            left.Right = node;

            this.UpdateHeight(node);

            return left;
        }

        private Node<T> RotateLeft(Node<T> node)
        {
            Node<T> right = node.Right;
            node.Right = right.Left;
            right.Left = node;

            this.UpdateHeight(node);

            return right;
        }
    }
}
