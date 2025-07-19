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
    private readonly Dictionary<TKey, LinkedListNode<TValue>> dictionary = new();

    // SortedSet is technically an option to use here, but it doesn't allow duplicates,
    // doesn't allow for arbitrary O(1) removals by storing nodes. we could use a custom
    // tree implementation, but that is likely overkill when we would expect maybe 10 values
    // at max, and would not offer the same iteration speed as a LinkedList.
    private readonly LinkedList<TValue> linkedList = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSortedDictionary{TKey, TValue}"/> class.
    /// </summary>
    public ValueSortedDictionary()
    {
    }

    /// <inheritdoc/>
    public ICollection<TKey> Keys => this.dictionary.Keys;

    /// <inheritdoc/>
    public ICollection<TValue> Values => throw new NotImplementedException();

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
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();

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

    /// <summary>
    /// Removes the element with the given key from the <see cref="ValueSortedDictionary{TKey, TValue}"/>,
    /// and copies the value to the <paramref name="value"/> parameter.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <param name="value">If this method returns <see langword="true"/>, the value of the removed element.</param>
    /// <returns><see langword="true"/> if the element was in the <see cref="ValueSortedDictionary{TKey, TValue}"/> and removed;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Remove(TKey key, [NotNullWhen(true)] out TValue value)
    {
        if (this.dictionary.Remove(key, out var node))
        {
            value = node.Value;

            this.linkedList.Remove(value);

            return true;
        }

        value = default!;
        return false;
    }

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

        // because we add nodes AFTER all equal nodes, it is more efficient to
        // iterate backwards (so we don't have to enumerate through the nodes
        // that are equal)
        for (var node = this.linkedList.Last; node != null; node = node.Previous)
        {
            // greater than or equal to 0 = comes before / same sort order
            if (value.CompareTo(node.Value) >= 0)
            {
                valueNode = this.linkedList.AddAfter(node, value);

                goto SkipAddLast;
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
}