namespace RueI.Utils.Extensions;

using System;
using System.Collections.Generic;

using NorthwoodLib.Pools;

/// <summary>
/// Provides extensions for <see cref="IDictionary{TKey, TValue}"/>.
/// </summary>
internal static class DictionaryExtensions
{
    /// <summary>
    /// Removes all the elements that match a certain predicate, and yields the elements that do not.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The type of the values of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="dict">The <see cref="IDictionary{TKey, TValue}"/> to remove the values from.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that determines which elements are removed. Elements for which this
    /// evaluates to <see langword="true"/> will be removed.
    /// </param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all values that do not pass <paramref name="predicate"/> and
    /// are not removed.</returns>
    public static IEnumerable<TValue> FilterOut<TKey, TValue>(this IDictionary<TKey, TValue> dict, Predicate<KeyValuePair<TKey, TValue>> predicate)
    {
        List<TKey> removeList = ListPool<TKey>.Shared.Rent();

        foreach (KeyValuePair<TKey, TValue> pair in dict)
        {
            if (predicate(pair))
            {
                removeList.Add(pair.Key);
            }
            else
            {
                yield return pair.Value;
            }
        }

        foreach (TKey key in removeList)
        {
            dict.Remove(key);
        }

        ListPool<TKey>.Shared.Return(removeList);
    }
}