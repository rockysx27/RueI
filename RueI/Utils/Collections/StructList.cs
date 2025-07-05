namespace RueI.Utils.Collections;

using System;

/// <summary>
/// Represents a <see langword="struct"/> list that offers <see langword="ref"/> access to elements.
/// </summary>
/// <typeparam name="T">The type of elements to store.</typeparam>
/// <remarks>
/// As an optimization, this method is a <see langword="struct"/>. Any copies will point to the same list,
/// but may get out of sync.
/// </remarks>
internal struct StructList<T>
{
    private T[] values;

    /// <summary>
    /// Initializes a new instance of the <see cref="StructList{T}"/> struct with no elements.
    /// </summary>
    public StructList()
    {
        this.values = Array.Empty<T>();
    }

    /// <summary>
    /// Gets the number of items in the <see cref="StructList{T}"/>.
    /// </summary>
    public int Length { get; private set; }

    private readonly int Capacity => this.values.Length;

    /// <summary>
    /// Gets the item at the given index.
    /// </summary>
    /// <param name="index">The index to get the item at.</param>
    /// <returns>The item at the index.</returns>
    /// <remarks>
    /// The value is unspecified if <paramref name="index"/> is out of bounds.
    /// </remarks>
    public readonly ref T this[int index] => ref this.values[index];

    /// <summary>
    /// Ensures that the <see cref="StructList{T}"/> has a certain amount of additional capacity.
    /// </summary>
    /// <param name="capacity">The capacity to add.</param>
    public void AddCapacity(int capacity)
    {
        if (capacity == 0)
        {
            return;
        }

        this.GrowTo(this.Capacity + capacity);
    }

    /// <summary>
    /// Adds an item to this <see cref="StructList{T}"/>.
    /// </summary>
    /// <param name="value">The item to add.</param>
    public void Add(in T value)
    {
        if (this.values.Length == this.Length)
        {
            int capacity;

            if (this.Capacity == 0)
            {
                capacity = 1;
            }
            else
            {
                capacity = this.Capacity * 2;
            }

            Array.Resize(ref this.values, capacity);
        }

        this[this.Length++] = value;
    }

    /// <summary>
    /// Adds a <see cref="StructList{T}"/> to the end of this <see cref="StructList{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="StructList{T}"/> to add.</param>
    public void AddList(StructList<T> list)
    {
        int length = list.Length;

        if (length == 0)
        {
            return;
        }

        this.GrowTo(length);

        list.values.CopyTo(this.values, this.Length);
        this.Length += length;
    }

    /// <summary>
    /// Clears this <see cref="StructList{T}"/>.
    /// </summary>
    public void Clear()
    {
        this.Length = 0;
    }

    /// <summary>
    /// Gets the values of the <see cref="StructList{T}"/> as a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="start">The position to start at.</param>
    /// <returns>A <see cref="Span{T}"/> that encompasses all values from <paramref name="start"/> to <see cref="Length"/>.</returns>
    public readonly Span<T> AsSpan(int start) => this.values.AsSpan(start, this.Length - start);

    private void GrowTo(int capacity)
    {
        if (this.Capacity + this.Length < capacity)
        {
            int newCapacity = this.Capacity * 2;

            if (newCapacity < capacity)
            {
                newCapacity = capacity;
            }

            Array.Resize(ref this.values, newCapacity);
        }
    }
}