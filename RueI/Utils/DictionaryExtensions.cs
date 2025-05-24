namespace RueI.Utils;

using System;
using System.Collections.Generic;

using NorthwoodLib.Pools;

/// <summary>
/// Provides extensions for <see cref="IDictionary{TKey, TValue}"/>.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Removes all the elements that match a certain predicate.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The type of the values of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="dict">The <see cref="IDictionary{TKey, TValue}"/> to remove the values from.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that determines which elements are removed. Elements for which this
    /// evaluates to <see langword="true"/> will be removed.
    /// </param>
    public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dict, Predicate<KeyValuePair<TKey, TValue>> predicate)
    {
        List<TKey> removeList = ListPool<TKey>.Shared.Rent();

        foreach (KeyValuePair<TKey, TValue> pair in dict)
        {
            if (predicate(pair))
            {
                removeList.Add(pair.Key);
            }
        }

        foreach (TKey key in removeList)
        {
            dict.Remove(key);
        }
    }
}