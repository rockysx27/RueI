namespace RueI.Utils;

using System;
using System.Collections.Generic;

using NorthwoodLib.Pools;

/// <summary>
/// Provides extensions for <see cref="ICollection{T}"/>.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Removes all the elements that match a certain predicate.
    /// </summary>
    /// <typeparam name="T">The type of the values of the <see cref="ICollection{T}"/>.</typeparam>
    /// <param name="collection">The <see cref="ICollection{T}"/> to remove the values from.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that determines which elements are removed. Elements for which this
    /// evaluates to <see langword="true"/> will be removed.
    /// </param>
    public static void RemoveAll<T>(this ICollection<T> collection, Predicate<T> predicate)
    {
        List<T> removeList = ListPool<T>.Shared.Rent();

        foreach (T value in collection)
        {
            if (predicate(value))
            {
                removeList.Add(value);
            }
        }

        foreach (T value in removeList)
        {
            collection.Remove(value);
        }
    }
}