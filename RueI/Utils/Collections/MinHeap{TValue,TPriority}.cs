namespace RueI.Utils.Collections;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a min-heap.
/// </summary>
/// <typeparam name="TValue">The type of the value to store.</typeparam>
/// <typeparam name="TPriority">The priority of the nodes, used to determine the sort order.</typeparam>
/// <remarks>
/// The <see cref="MinHeap{TValue, TPriority}"/> provides an efficient way to find the smallest
/// value, as determined by the <typeparamref name="TPriority"/>.
/// </remarks>
internal class MinHeap<TValue, TPriority>
    where TPriority : IComparable<TPriority>
{
    private readonly List<Node> heap = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MinHeap{TValue, TPriority}"/> class.
    /// </summary>
    public MinHeap()
    {
    }

    /// <summary>
    /// Gets the number of elements in the <see cref="MinHeap{TElement, TPriority}"/>.
    /// </summary>
    public int Count => this.heap.Count;

    /// <summary>
    /// Adds a node to the <see cref="MinHeap{TValue, TPriority}"/> and returns a
    /// <see cref="Node"/> that can be used to later reference it.
    /// </summary>
    /// <param name="value">The value to store.</param>
    /// <param name="priority">The priority of the node.</param>
    /// <returns>The <see cref="Node"/> that refers to the value and priority pair within the <see cref="MinHeap{TValue, TPriority}"/>.</returns>
    public Node Add(TValue value, TPriority priority)
    {
        Node node = new(value, priority);

        int index = this.heap.Count;

        this.heap.Add(node);

        this.HeapifyUp(index);

        return node;
    }

    /// <summary>
    /// Tries to get the smallest node of the <see cref="MinHeap{TValue, TPriority}"/>
    /// without removing it.
    /// </summary>
    /// <param name="value">If this method returns <see langword="true"/>, the stored node.</param>
    /// <returns><see langword="true"/> if there was a value and the heap was not empty; otherwise, <see langword="false"/>.</returns>
    public bool TryPeek(out Node value)
    {
        if (this.heap.Count != 0)
        {
            value = this.heap[0];

            return true;
        }

        value = default!;

        return false;
    }

    /// <summary>
    /// Removes the smallest value from the <see cref="MinHeap{TValue, TPriority}"/>.
    /// </summary>
    public void Pop() => this.Remove(0);

    /// <summary>
    /// Removes the <see cref="Node"/> from the <see cref="MinHeap{TValue, TPriority}"/>.
    /// </summary>
    /// <param name="node">The <see cref="Node"/> to remove.</param>
    /// <remarks>
    /// For performance, this does not check to see if <paramref name="node"/> belongs to
    /// this <see cref="MinHeap{TValue, TPriority}"/>, or that the <see cref="Node"/>
    /// is not invalid. Callers are responsible for ensuring all invariants are met.
    /// </remarks>
    public void Remove(Node node) => this.Remove(node.Position);

    private static int GetParent(int index) => (index - 1) / 2;

    private static int GetLeftChild(int index) => (index * 2) + 1;

    private static int GetRightChild(int index) => (index * 2) + 2;

    private void SwapNodes(int first, int second)
    {
        Node firstNode = this.heap[first];
        Node secondNode = this.heap[second];

        (firstNode.Position, secondNode.Position) = (second, first);

        (this.heap[first], this.heap[second]) = (secondNode, firstNode);
    }

    private void HeapifyUp(int index)
    {
        // not implemented recursively to improve performance
        while (index > 0)
        {
            int parent = GetParent(index);

            // greater than zero means that *this* comes after *other* (i.e., *this* > other)
            // we need to break whenever *this* > other
            if (this.IsFirstLarger(index, parent))
            {
                break;
            }

            this.SwapNodes(index, parent);

            index = parent;
        }
    }

    private void HeapifyDown(int index)
    {
        int max = this.heap.Count;

        while (true)
        {
            int left = GetLeftChild(index);
            int right = GetRightChild(index);

            int currentSmallest = index;

            if (left < max && !this.IsFirstLarger(left, currentSmallest))
            {
                currentSmallest = left;
            }

            if (right < max && !this.IsFirstLarger(right, currentSmallest))
            {
                currentSmallest = right;
            }

            if (currentSmallest == index)
            {
                break;
            }

            this.SwapNodes(index, currentSmallest);
            index = currentSmallest;
        }
    }

    private void Remove(int index)
    {
        int last = this.heap.Count - 1;

        this.SwapNodes(index, last);
        this.heap.RemoveAt(last);

        this.HeapifyDown(index);
        this.HeapifyUp(index);
    }

    private bool IsFirstLarger(int firstIndex, int secondIndex) => this.heap[firstIndex].Priority.CompareTo(this.heap[secondIndex].Priority) >= 0;

    /// <summary>
    /// Represents a node within a <see cref="MinHeap{TElement, TPriority}"/>.
    /// </summary>
    internal class Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="priority">The priority of the element.</param>
        internal Node(TValue value, TPriority priority)
        {
            this.Value = value;
            this.Priority = priority;
        }

        /// <summary>
        /// Gets the stored <typeparamref name="TValue"/> of the <see cref="Node"/>.
        /// </summary>
        internal TValue Value { get; }

        /// <summary>
        /// Gets the priority of the element, used for determining the sort order.
        /// </summary>
        internal TPriority Priority { get; }

        /// <summary>
        /// Gets or sets index of the <see cref="Node"/> within the array backing the <see cref="MinHeap{TElement, TPriority}"/>.
        /// </summary>
        internal int Position { get; set; }
    }
}