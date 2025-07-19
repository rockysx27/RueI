namespace RueI.API.Parsing.Modifications;

using System;
using RueI.API.Parsing;

/// <summary>
/// Represents a modification to the text of an element.
/// </summary>
internal abstract class Modification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Modification"/> class.
    /// </summary>
    /// <param name="position">The position of the modification.</param>
    public Modification(int position)
    {
        this.Position = position;
    }

    /// <summary>
    /// Gets the position at which the modification should be applied.
    /// </summary>
    internal int Position { get; }

    /// <summary>
    /// Applies this modification.
    /// </summary>
    /// <param name="context">The context of the parser.</param>
    /// <param name="buffer">The character buffer of the parser's element's text.</param>
    internal abstract void Apply(CombinerContext context, ref ReadOnlySpan<char> buffer);
}