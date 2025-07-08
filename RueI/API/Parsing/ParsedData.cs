namespace RueI.API.Parsing;

using System.Collections.Generic;
using RueI.API.Parsing.Modifications;
using RueI.API.Parsing.Structs;

/// <summary>
/// Represents a parsed string, used for combining elements.
/// </summary>
public readonly struct ParsedData
{
    /// <summary>
    /// The string that was parsed.
    /// </summary>
    internal readonly string? ParsedString;

    /// <summary>
    /// Gets the total vertical offset of the linebreaks within the string.
    /// </summary>
    internal readonly CumulativeFloat Offset;

    /// <summary>
    /// A <see cref="List{T}"/> containing all modifications and additions.
    /// </summary>
    internal readonly List<Modification> Modifications;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsedData"/> struct.
    /// </summary>
    /// <param name="parsedString">The parsed <see langword="string"/>.</param>
    /// <param name="offset">The total offset of the vertical breaks within the <see langword="string"/>.</param>
    /// <param name="modifications">A <see cref="List{T}"/> of modifications..</param>
    internal ParsedData(string parsedString, CumulativeFloat offset, List<Modification> modifications)
    {
        this.ParsedString = parsedString;
        this.Offset = offset;
        this.Modifications = modifications;
    }
}