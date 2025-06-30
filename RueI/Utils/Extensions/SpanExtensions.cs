namespace RueI.Utils.Extensions;

using System;

/// <summary>
/// Provides extensions for working with <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/>.
/// </summary>
internal static class SpanExtensions
{
    /// <summary>
    /// Returns a slice of <see cref="ReadOnlySpan{T}"/> that contains all the values until a <see cref="Predicate{T}"/> returns false.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ReadOnlySpan{T}"/>.</typeparam>
    /// <param name="span">The <see cref="ReadOnlySpan{T}"/> to slice.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> to use.</param>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> that contains all the values until <paramref name="predicate"/> returns false, exclusive.</returns>
    internal static ReadOnlySpan<T> TakeWhile<T>(this ReadOnlySpan<T> span, Predicate<T> predicate)
    {
        int i;
        for (i = 0; i < span.Length; i++)
        {
            if (!predicate(span[i]))
            {
                break;
            }
        }

        return span[..i]; // exclusive
    }

    /// <summary>
    /// Returns a slice of <see cref="ReadOnlySpan{T}"/> that contains all the values until a given <typeparamref name="T"/>, or an empty <see cref="ReadOnlySpan{T}"/>
    /// if the character does not appear.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ReadOnlySpan{T}"/>.</typeparam>
    /// <param name="span">The <see cref="ReadOnlySpan{T}"/> to slice.</param>
    /// <param name="value">The value to stop at.</param>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> that contains all the values until <paramref name="value"/>, exclusive, or <see cref="ReadOnlySpan{T}.Empty"/>
    /// if <paramref name="value"/> does not appear.</returns>
    internal static ReadOnlySpan<T> Terminated<T>(this ReadOnlySpan<T> span, T value)
        where T : IEquatable<T>
    {
        // we need length to be one smaller than the length of the span otherwise
        // i would end up becoming span[span.Length] and error
        // in other words, the maximum value of i will always be span[i - 1]
        int newLength = span.Length - 1;
        int i;
        for (i = 0; i < newLength && !span[i].Equals(value); i++)
        {
        }

        if (span[i].Equals(value))
        {
            return span[..i]; // exclusive (doesn't include the terminator)
        }
        else
        {
            return ReadOnlySpan<T>.Empty;
        }
    }

    /// <summary>
    /// Counts the number of times a value appears consecutively, stopping when the first non-value is reached.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ReadOnlySpan{T}"/>.</typeparam>
    /// <param name="span">The <see cref="ReadOnlySpan{T}"/> to slice.</param>
    /// <param name="value">The value to count.</param>
    /// <returns>An <see langword="int"/> of the number of times <paramref name="value"/> appeared before a non-value was reached.</returns>
    internal static int CountConsecutive<T>(this ReadOnlySpan<T> span, T value)
        where T : IEquatable<T>
    {
        int i;
        for (i = 0; i < span.Length && !span[i].Equals(value); i++)
        {
        }

        return i;
    }
}