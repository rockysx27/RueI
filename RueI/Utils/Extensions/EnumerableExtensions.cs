namespace RueI.Utils.Extensions;

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
}