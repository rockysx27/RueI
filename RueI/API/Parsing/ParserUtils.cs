namespace RueI.API.Parsing;

using System;
using RueI.Utils.Extensions;

/// <summary>
/// Provides generic methods for parsing.
/// </summary>
internal class ParserUtils
{
    private const char BackSlash = '\\';

    /// <summary>
    /// Parses a <see cref="ReadOnlySpan{T}"/> to determine if there are escaped characters.
    /// </summary>
    /// <param name="input">The <see cref="ReadOnlySpan{T}"/> to use as input.</param>
    /// <param name="c">The <see langword="char"/> to use as an escape character.</param>
    /// <returns>A <see cref="Tuple{T1, T2}"/>, where the first value indicates the number of escaped characters
    /// and the second value indicates if there are any unescaped characters.</returns>
    internal static (int Escaped, bool ShouldParse) ParseEscaped(ref ReadOnlySpan<char> input, char c)
    {
        int count = input.CountConsecutive(c);
        input = input[count..];

        // count >> 1 is equivalent to count / 2, and count & 1 == 0 checks if there is a remainder after that
        // the logic behind this is that if you have e.g. 7 characters, it forms 3 pairs of escaped characters and we're left with 1 unescaped character
        // 7 / 2 = 3
        // 7 % 2 = 1 (indicates that there's a character to parse)
        return (count >> 1, (count & 1) == 1);
    }
}