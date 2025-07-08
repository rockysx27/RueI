namespace RueI.API.Parsing.Modifications;

using System;

/// <summary>
/// Represents a <see cref="Modification"/> that skips a certain number of characters
/// from the buffer.
/// </summary>
internal class SkipNextModification : Modification
{
    private readonly int count;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkipNextModification"/> class.
    /// </summary>
    /// <param name="position">The position of the <see cref="Modification"/>.</param>
    /// <param name="skipCount">The number of characters to skip.</param>
    internal SkipNextModification(int position, int skipCount)
        : base(position)
    {
        this.count = skipCount;
    }

    /// <inheritdoc/>
    internal override void Apply(CombinerContext context, ref ReadOnlySpan<char> buffer)
    {
        buffer = buffer[this.count..];
    }
}