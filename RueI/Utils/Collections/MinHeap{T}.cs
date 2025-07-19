namespace RueI.Utils.Collections;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a priority queue backed by a min-heap.
/// </summary>
/// <typeparam name="T">The type of the value to store and to use for ordering.</typeparam>
/// <remarks>
/// The <see cref="MinHeap{T}"/> provides an efficient way to find the smallest
/// value, as determined by the sort order of <typeparamref name="T"/>.
/// </remarks>
internal class MinHeap<T>
    where T : IComparable<T>
{
    private readonly List<Node> heap = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MinHeap{T}"/> class.
    /// </summary>
    public MinHeap()
    {
    }

    /// <summary>
    /// Gets the number of elements in the <see cref="MinHeap{T}"/>.
    /// </summary>
    public int Count => this.heap.Count;

    /// <summary>
    /// Adds a node to the <see cref="MinHeap{T}"/> and returns a
    /// <see cref="Node"/> that can be used to later reference it.
    /// </summary>
    /// <param name="value">The value to store.</param>
    /// <returns>The <see cref="Node"/> that refers to the value and priority pair within the <see cref="MinHeap{T}"/>.</returns>
    public Node Add(T value)
    {
        int index = this.heap.Count;

        Node node = new(value, index);

        this.heap.Add(node);

        this.HeapifyUp(index);

        return node;
    }

    /// <summary>
    /// Tries to get the smallest node of the <see cref="MinHeap{T}"/>
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
    /// Removes the smallest value from the <see cref="MinHeap{T}"/>.
    /// </summary>
    public void Pop() => this.Remove(0);

    /// <summary>
    /// Removes the <see cref="Node"/> from the <see cref="MinHeap{T}"/>.
    /// </summary>
    /// <param name="node">The <see cref="Node"/> to remove.</param>
    /// <remarks>
    /// For performance, this does not check to see if <paramref name="node"/> belongs to
    /// this <see cref="MinHeap{T}"/>, or that the <see cref="Node"/>
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

        if (this.Count != 0)
        {
            this.HeapifyDown(index);
            this.HeapifyUp(index);
        }
    }

    private bool IsFirstLarger(int firstIndex, int secondIndex) => this.heap[firstIndex].Value.CompareTo(this.heap[secondIndex].Value) >= 0;

    /// <summary>
    /// Represents a node within a <see cref="MinHeap{T}"/>.
    /// </summary>
    internal class Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="position">The position to store the node at.</param>
        internal Node(T value, int position)
        {
            this.Value = value;
            this.Position = position;
        }

        /// <summary>
        /// Gets the stored <typeparamref name="T"/> of the <see cref="Node"/>.
        /// </summary>
        internal T Value { get; }

        /// <summary>
        /// Gets or sets index of the <see cref="Node"/> within the array backing the <see cref="MinHeap{T}"/>.
        /// </summary>
        internal int Position { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Node"/> is the first in the <see cref="MinHeap{T}"/>.
        /// </summary>
        internal bool IsFirst => this.Position == 0;
    }
}