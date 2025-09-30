namespace RueI.Utils.Extensions;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides extensions for <see cref="IEnumerable{T}"/>.
/// </summary>
internal static class EnumerableExtensions
{
    /// <summary>
    /// Tries to get a singular value from an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to use.</param>
    /// <param name="value">When the method returns, the only value in the <see cref="IEnumerable{T}"/>, if it only has one value.</param>
    /// <returns><see langword="true"/> if <paramref name="enumerable"/> only has one value; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetSingle<T>(this IEnumerable<T> enumerable, out T value)
    {
        using IEnumerator<T> enumerator = enumerable.GetEnumerator();

        if (enumerator.MoveNext())
        {
            value = enumerator.Current;

            if (!enumerator.MoveNext())
            {
                return true;
            }
        }

        value = default!;

        return false;
    }

    /// <summary>
    /// Merges two sorted <see cref="IEnumerable{T}"/>s in ascending order.
    /// </summary>
    /// <typeparam name="T">The type to compare.</typeparam>
    /// <param name="first">The first sorted <see cref="IEnumerable{T}"/>.</param>
    /// <param name="second">The second sorted <see cref="IEnumerable{T}"/>.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains all of the elements, in order.</returns>
    /// <remarks>
    /// In case two elements are equal, the value from <paramref name="first"/> will come first.
    /// </remarks>
    public static IEnumerable<T> MergeAscending<T>(this IEnumerable<T> first, IEnumerable<T> second)
        where T : IComparable<T>
    {
        using var firstEnumerator = first.GetEnumerator();
        using var secondEnumerator = second.GetEnumerator();

        bool hasFirst = firstEnumerator.MoveNext();
        bool hasSecond = secondEnumerator.MoveNext();

        while (hasFirst && hasSecond)
        {
            if (firstEnumerator.Current.CompareTo(secondEnumerator.Current) <= 0)
            {
                yield return firstEnumerator.Current;
                hasFirst = firstEnumerator.MoveNext();
            }
            else
            {
                yield return secondEnumerator.Current;
                hasSecond = secondEnumerator.MoveNext();
            }
        }

        while (hasFirst)
        {
            yield return firstEnumerator.Current;
            hasFirst = firstEnumerator.MoveNext();
        }

        while (hasSecond)
        {
            yield return secondEnumerator.Current;
            hasSecond = secondEnumerator.MoveNext();
        }
    }
}