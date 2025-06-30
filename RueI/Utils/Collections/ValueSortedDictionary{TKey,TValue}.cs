namespace RueI.Utils.Collections;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a dictionary that is sorted on the values.
/// </summary>
/// <typeparam name="TKey">The type of the keys of the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values of the dictionary.</typeparam>
/// <remarks>
/// Unlike a <see cref="SortedDictionary{TKey, TValue}"/>, a <see cref="ValueSortedDictionary{TKey, TValue}"/> is
/// sorted on the values. The time complexity for all operations is the same as a <see cref="Dictionary{TKey, TValue}"/>,
/// except for insertions, which are O(n). Note that it is a logic error for the sorting order of a value within
/// the <see cref="ValueSortedDictionary{TKey, TValue}"/> to change.
/// </remarks>
internal sealed class ValueSortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    where TValue : IComparable<TValue>
{
    private readonly Dictionary<TKey, LinkedListNode<TValue>> dictionary;
    private readonly LinkedList<TValue> linkedList = new();
    private readonly InsertionBehavior behavior;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSortedDictionary{TKey, TValue}"/> class.
    /// </summary>
    public ValueSortedDictionary()
        : this(InsertionBehavior.InsertBeforeEqual)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSortedDictionary{TKey, TValue}"/> class
    /// with the given <see cref="InsertionBehavior"/>.
    /// </summary>
    /// <param name="insertionBehavior">The insertion behavior when inserting a value that has the
    /// same sort order as another value.</param>
    public ValueSortedDictionary(InsertionBehavior insertionBehavior)
    {
        this.dictionary = new();
        this.behavior = insertionBehavior;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSortedDictionary{TKey, TValue}"/> class with the given capacity.
    /// </summary>
    /// <param name="capacity">The capacity of the dictionary.</param>
    public ValueSortedDictionary(int capacity)
        : this(capacity, InsertionBehavior.InsertBeforeEqual)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSortedDictionary{TKey, TValue}"/> class with the given capacity
    /// and the given <see cref="InsertionBehavior"/>.
    /// </summary>
    /// <param name="capacity">The capacity of the dictionary.</param>
    /// <param name="insertionBehavior"><inheritdoc cref="ValueSortedDictionary(InsertionBehavior)" path="/param[@name='insertionBehavior']"/></param>
    public ValueSortedDictionary(int capacity, InsertionBehavior insertionBehavior)
    {
        this.dictionary = new(capacity);
        this.behavior = insertionBehavior;
    }

    /// <summary>
    /// Represents the insertion behavior when inserting a value that
    /// has the same sort order as another element.
    /// </summary>
    public enum InsertionBehavior
    {
        /// <summary>
        /// The value will be inserted after all of the values
        /// with the same sort order.
        /// </summary>
        InsertAfterEqual,

        /// <summary>
        /// The value will be inserted before all of the values
        /// with the same sort order.
        /// </summary>
        InsertBeforeEqual,
    }

    /// <inheritdoc/>
    public ICollection<TKey> Keys => this.dictionary.Keys;

    /// <inheritdoc/>
    public ICollection<TValue> Values => new ValueCollection(this.linkedList);

    /// <inheritdoc/>
    public int Count => this.dictionary.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public TValue this[TKey key]
    {
        get => this.dictionary[key].Value;

        set
        {
            this.RemoveFromLinkedList(key);
            this.Insert(key, value);
        }
    }

    /// <inheritdoc/>
    public void Add(TKey key, TValue value)
    {
        if (this.dictionary.ContainsKey(key))
        {
            throw new ArgumentException("Key is already present within the dictionary.", nameof(key));
        }

        this.Insert(key, value);
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        this.Add(item.Key, item.Value);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        this.linkedList.Clear();
        this.dictionary.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TValue> item) => this.dictionary.TryGetValue(item.Key, out var value) && value.Equals(item.Value);

    /// <inheritdoc/>
    public bool ContainsKey(TKey key) => this.dictionary.ContainsKey(key);

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Remove(TKey key)
    {
        if (this.dictionary.Remove(key, out var value))
        {
            this.linkedList.Remove(value);

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TValue> item) => this.Remove(item.Key);

    /// <inheritdoc/>
    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue value)
    {
        if (this.dictionary.TryGetValue(key, out LinkedListNode<TValue> node))
        {
            value = node.Value;

            return true;
        }

        value = default!;
        return false;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new EnumeratorAdapter<KeyValuePair<TKey, LinkedListNode<TValue>>, KeyValuePair<TKey, TValue>>(this.dictionary.GetEnumerator(), x => new(x.Key, x.Value.Value));
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// Adds a <typeparamref name="TKey"/> and <typeparamref name="TValue"/> without a check.
    /// </summary>
    private void Insert(TKey key, TValue value)
    {
        LinkedListNode<TValue> valueNode;

        if (this.behavior == InsertionBehavior.InsertBeforeEqual)
        {
            for (var node = this.linkedList.First; node != null; node = node.Next)
            {
                // less than 0 = comes before
                if (value.CompareTo(node.Value) <= 0)
                {
                    valueNode = this.linkedList.AddBefore(node, value);

                    goto SkipAddLast;
                }
            }
        }
        else
        {
            for (var node = this.linkedList.Last; node != null; node = node.Previous)
            {
                // greater than or equal to 0 = comes before / same sort order
                if (value.CompareTo(node.Value) >= 0)
                {
                    valueNode = this.linkedList.AddAfter(node, value);

                    goto SkipAddLast;
                }
            }
        }

        valueNode = this.linkedList.AddLast(value);

    SkipAddLast:
        this.dictionary[key] = valueNode;
    }

    private void RemoveFromLinkedList(TKey key)
    {
        if (this.dictionary.TryGetValue(key, out var value))
        {
            this.linkedList.Remove(value);
        }
    }

    // necessary because turning a LinkedList into a collection allows for adding elements (potential logic error)
    private readonly struct ValueCollection : ICollection<TValue>
    {
        private readonly LinkedList<TValue> linkedList;

        public ValueCollection(LinkedList<TValue> linkedList)
        {
            this.linkedList = linkedList;
        }

        public readonly int Count => this.linkedList.Count;

        public readonly bool IsReadOnly => false;

        public void Add(TValue item) => throw new NotImplementedException();

        public void Clear() => throw new NotImplementedException();

        public void CopyTo(TValue[] array, int arrayIndex) => throw new NotImplementedException();

        public bool Remove(TValue item) => throw new NotImplementedException();

        public readonly bool Contains(TValue item) => this.linkedList.Contains(item);

        public readonly IEnumerator<TValue> GetEnumerator() => this.linkedList.GetEnumerator();

        readonly IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}