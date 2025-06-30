namespace RueI.Utils.Collections;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents an adaptor for an <see cref="IEnumerator{T}"/>.
/// </summary>
/// <typeparam name="TSource">The old type of the enumerator.</typeparam>
/// <typeparam name="TValue">The type that the adaptor <see cref="IEnumerator{T}"/> should yield.</typeparam>
/// <remarks>
/// The <see cref="EnumeratorAdapter{TSource, TValue}"/> can convert a <see cref="IEnumerator{T}"/> yielding
/// <typeparamref name="TSource"/> to <typeparamref name="TValue"/> using a <see cref="Func{T, TResult}"/>.
/// Unlike <see cref="System.Linq.Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>,
/// this class does not operate on <see cref="IEnumerable{T}"/>.
/// </remarks>
internal sealed class EnumeratorAdapter<TSource, TValue> : IEnumerator<TValue>
{
    private readonly IEnumerator<TSource> enumerator;
    private readonly Func<TSource, TValue> converter;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumeratorAdapter{TSource, TValue}"/> class.
    /// </summary>
    /// <param name="enumerator">The <see cref="IEnumerator{T}"/> to use as the source.</param>
    /// <param name="converter">A <see cref="Func{T, TResult}"/> that converts from <typeparamref name="TSource"/> to <typeparamref name="TValue"/>.</param>
    public EnumeratorAdapter(IEnumerator<TSource> enumerator, Func<TSource, TValue> converter)
    {
        this.enumerator = enumerator;
        this.converter = converter;
    }

    /// <inheritdoc/>
    public TValue Current => this.converter(this.enumerator.Current);

    /// <inheritdoc/>
    object IEnumerator.Current => this.Current!;

    /// <inheritdoc/>
    public void Dispose() => this.enumerator.Dispose();

    /// <inheritdoc/>
    public bool MoveNext() => this.enumerator.MoveNext();

    /// <inheritdoc/>
    public void Reset() => this.enumerator.Reset();
}