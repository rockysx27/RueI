namespace RueI.Utils.Collections;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RueI.API.Elements;

/// <summary>
/// Represents a dictionary that is sorted on <typeparamref name="TValue"/>.
/// </summary>
/// <typeparam name="TKey">The type of the keys of the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values of the dictionary.</typeparam>
public sealed class ValueSortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    where TValue : IComparable<TValue>
{
    private readonly Dictionary<TKey, LinkedListNode<TValue>> dictionary;
    private readonly LinkedList<TValue> linkedList = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSortedDictionary{TKey, TValue}"/> class.
    /// </summary>
    public ValueSortedDictionary()
    {
        this.dictionary = new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSortedDictionary{TKey, TValue}"/> class with the given capacity.
    /// </summary>
    /// <param name="capacity">The capacity of the dictionary.</param>
    public ValueSortedDictionary(int capacity)
    {
        this.dictionary = new(capacity);
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
            throw new ArgumentException(nameof(key));
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
    public bool Contains(KeyValuePair<TKey, TValue> item) => ((IEnumerable<KeyValuePair<TKey, TValue>>)this).Contains(item);

    /// <inheritdoc/>
    public bool ContainsKey(TKey key)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Remove(TKey key)
    {
        if (this.dictionary.TryGetValue(key, out var value))
        {
            this.linkedList.Remove(value);
            this.dictionary.Remove(key);

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TValue> item) => this.Remove(item.Key);

    /// <inheritdoc/>
    public bool TryGetValue(TKey key, out TValue value)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new EnumeratorAdapter<KeyValuePair<TKey, LinkedListNode<TValue>>, KeyValuePair<TKey, TValue>>(this.dictionary.GetEnumerator(), x => new(x.Key, x.Value.Value));
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    // add without a check
    private void Insert(TKey key, TValue value)
    {
        LinkedListNode<TValue> valueNode;

        for (var node = this.linkedList.First; node != null; node = node.Next)
        {
            // less than 0 = precedes
            if (value.CompareTo(node.Value) < 0)
            {
                valueNode = this.linkedList.AddBefore(node, value);

                goto SkipAddLast;
            }
        }

        valueNode = this.linkedList.AddLast(value);

    SkipAddLast:
        this.dictionary[key] = valueNode;
    }

    private bool RemoveFromLinkedList(TKey key)
    {
        if (this.dictionary.TryGetValue(key, out var value))
        {
            this.linkedList.Remove(value);

            return true;
        }

        return false;
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