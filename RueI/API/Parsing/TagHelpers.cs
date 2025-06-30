namespace RueI.API.Parsing;

using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using RueI.Utils.Enums;
using UnityEngine;

/// <summary>
/// Provides helper methods for working with tags.
/// </summary>
internal static class TagHelpers
{
    private const char LowercaseMask = (char)0b00100000; // takes advantage of the fact that capitals only differs by one bit
    private const uint AsciiLetterUpperBound = 'z' - 'a';

    private const char Mask = (char)('=' & '>');

    /// <summary>
    /// Returns a value indicating whether a <see langword="char"/> is valid for tag names.
    /// </summary>
    /// <param name="ch">The <see langword="char"/> to check.</param>
    /// <returns><see langword="true"/> if the <paramref name="ch"/> is a valid tag name; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsTagNameChar(char ch) => IsAsciiLetter(ch) || ch is '-' or '/';

    /// <summary>
    /// Quickly converts a <see langword="char"/> to lowercase, provided that the <see langword="char"/> is valid for tags.
    /// </summary>
    /// <param name="ch">The <see langword="char"/> to convert to lowercase.</param>
    /// <returns><paramref name="ch"/> in lowercase.</returns>
    internal static char ToLowercaseFast(char ch) => ch switch
    {
        '/' => '/',
        '-' => '-',
        _ => (char)(ch & LowercaseMask), // TODO: check to make sure this works
    };

    /// <summary>
    /// Quickly determines if a <see cref="char"/> is a digit.
    /// </summary>
    /// <param name="ch">The <see langword="char"/> to check.</param>
    /// <returns><see langword="true"/> if the <paramref name="ch"/> represents a digit; otherwise, <see langword="false"/>.</returns>
    internal static bool IsDigitFast(char ch) => ch >= '0' && ch <= '9';

    /// <summary>
    /// Quickly converts a <see cref="char"/> to an integer digit.
    /// </summary>
    /// <param name="ch">The <see cref="char"/> to convert.</param>
    /// <returns>The <see langword="int"/> representation of <paramref name="ch"/>.</returns>
    internal static int ToDigitFast(char ch) => ch - '0';

    /// <summary>
    /// Returns a value indicating whether a <see langword="char"/> terminates tags.
    /// </summary>
    /// <param name="ch">The <see langword="char"/> to check.</param>
    /// <returns><see langword="true"/> if the char terminates tags; otherwise, <see langword="true"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsTerminatingChar(char ch) => (uint)(ch - 61) < 2; // check if a char is = or >, slightly slightly faster

    /// <summary>
    /// Determines whether the given tag name represents a tag that is ended by an equal sign.
    /// </summary>
    /// <param name="tagName">The tag name to check.</param>
    /// <returns><see langword="true"/> if the tag name ends with an equal sign; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TagEndsWithEqualsSign(ReadOnlySpan<char> tagName) => tagName.SequenceEqual("br");

    /// <summary>
    /// Parses a <see langword="float"/> from a <see cref="ReadOnlySpan{T}"/> containing
    /// an integer part and a <see cref="ReadOnlySpan{T}"/> containing the decimal part.
    /// </summary>
    /// <param name="integerPart">A <see cref="ReadOnlySpan{T}"/> of the integer part.</param>
    /// <param name="decimalPart">A <see cref="ReadOnlySpan{T}"/> of the decimal part.</param>
    /// <returns>The parsed <see langword="float"/>.</returns>
    internal static float FromIntegerAndDecimal(ReadOnlySpan<char> integerPart, ReadOnlySpan<char> decimalPart)
    {
        const System.Globalization.NumberStyles Style = System.Globalization.NumberStyles.None;
        float value = 0;

        if (!integerPart.IsEmpty)
        {
            value += float.Parse(integerPart, Style);
        }

        if (!decimalPart.IsEmpty)
        {
            value += float.Parse(integerPart, Style) / Mathf.Pow(10, decimalPart.Length);
        }

        return value;
    }

    // check if a char is a-z or A-Z, inclusive
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAsciiLetter(char ch) => (uint)((ch | LowercaseMask) - 'a') <= AsciiLetterUpperBound;
}