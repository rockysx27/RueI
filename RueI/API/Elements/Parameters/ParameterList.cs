namespace RueI.API.Elements.Parameters;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a list of parameters.
/// </summary>
/// <remarks>
/// <see cref="ParameterList"/> is a read-only <see langword="struct"/> that allows for providing <see cref="Hints.HintParameter"/>s
/// to elements.
/// </remarks>
[CollectionBuilder(typeof(ParameterList), nameof(Create))]
public readonly struct ParameterList : IReadOnlyCollection<ContentParameter>
{
    // internally, we store the list of parameters as a linked list, enabling additions to not generate an entirely new list (since we just represent it
    // as another node in a linked list)
    // this also allows for "branching" parameter lists
    private readonly BoxedParameterList? nextList;
    private readonly int count = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterList"/> struct.
    /// </summary>
    public ParameterList()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterList"/> struct using the given <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="parameters">An <see cref="IEnumerable{T}"/> of <see cref="ContentParameter"/>s.</param>
    public ParameterList(IEnumerable<ContentParameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            this.count++;
            this.nextList = new(parameter, this.nextList);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterList"/> struct using the given <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="parameters">A collection of <see cref="ContentParameter"/>s.</param>
    public ParameterList(params ContentParameter[] parameters)
        : this(parameters.AsEnumerable()) // AsEnumerable so we call the right cctor overload
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterList"/> struct using the given <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="parameters">An <see cref="ReadOnlySpan{T}"/> of <see cref="ContentParameter"/>s.</param>
    internal ParameterList(ReadOnlySpan<ContentParameter> parameters)
    {
        this.count = parameters.Length;

        // cannot convert a span to an ienumerable so we can't call the cctor overload that takes an ienumerable
        foreach (var parameter in parameters)
        {
            this.nextList = new(parameter, this.nextList);
        }
    }

    private ParameterList(ParameterList paramList, ContentParameter newParam)
    {
        this.nextList = new BoxedParameterList(newParam, this.nextList);
        this.count = paramList.count + 1; // no ++ syntax allowed
    }

    /// <summary>
    /// Gets the number of <see cref="ContentParameter"/>s in the <see cref="ParameterList"/>.
    /// </summary>
    public readonly int Count => this.count;

    /// <summary>
    /// Creates a new <see cref="ParameterList"/> with the given <see cref="ContentParameter"/>.
    /// </summary>
    /// <param name="parameter">The <see cref="ContentParameter"/> to add.</param>
    /// <returns>A new <see cref="ParameterList"/> with the <see cref="ContentParameter"/>.</returns>
    /// <remarks>
    /// This method does not modify the current <see cref="ParameterList"/>. Thus, you should never ignore the return value.
    /// </remarks>
    public readonly ParameterList Add(ContentParameter parameter) => new(this, parameter);

    /// <summary>
    /// Gets the enumerator for this <see cref="ParameterList"/>.
    /// </summary>
    /// <returns>A new <see cref="Enumerator"/>.</returns>
    public readonly Enumerator GetEnumerator() => new(this);

    /// <inheritdoc/>
    readonly IEnumerator<ContentParameter> IEnumerable<ContentParameter>.GetEnumerator() => this.GetEnumerator();

    /// <inheritdoc/>
    readonly IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// Creates a new <see cref="ParameterList"/> from a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="parameters">The <see cref="ReadOnlySpan{T}"/> to use.</param>
    /// <returns>A new <see cref="ReadOnlySpan{T}"/> to use.</returns>
    /// <remarks>
    /// This method is primarily intended to provide support for
    /// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/collection-expressions">collection expressions</see>.
    /// </remarks>
    internal static ParameterList Create(ReadOnlySpan<ContentParameter> parameters) => new(parameters);

    /// <summary>
    /// Provides an enumerator for a <see cref="ParameterList"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<ContentParameter>
    {
        private BoxedParameterList? current;

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumerator"/> struct.
        /// </summary>
        /// <param name="list">The <see cref="ParameterList"/> to use.</param>
        internal Enumerator(ParameterList list)
        {
            this.current = list.nextList;
        }

        /// <summary>
        /// Gets the current <see cref="ContentParameter"/>.
        /// </summary>
        public readonly ContentParameter Current => this.current!.Parameter;

        /// <summary>
        /// Gets the current <see cref="ContentParameter"/>.
        /// </summary>
        readonly object IEnumerator.Current => this.Current;

        /// <summary>
        /// Disposes the <see cref="Enumerator"/>.
        /// </summary>
        public readonly void Dispose()
        {
        }

        /// <summary>
        /// Moves the <see cref="Enumerator"/> forward.
        /// </summary>
        /// <returns>A value indicating whether there is another value.</returns>
        public bool MoveNext()
        {
            return (this.current = this.current?.Next) != null;
        }

        /// <summary>
        /// Resets the <see cref="Enumerator"/>.
        /// </summary>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    private class BoxedParameterList
    {
        public BoxedParameterList(ContentParameter parameter, BoxedParameterList? next)
        {
            this.Parameter = parameter;
            this.Next = next;
        }

        public ContentParameter Parameter { get; }

        public BoxedParameterList? Next { get; }
    }
}