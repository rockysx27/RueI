namespace RueI.API.Parsing.Modifications;

using System;

/// <summary>
/// Represents a modification that prevents breaks.
/// </summary>
internal class NobreakModification : Modification
{
    private int length;

    /// <summary>
    /// Initializes a new instance of the <see cref="NobreakModification"/> class.
    /// </summary>
    /// <param name="position">The position to add the <see cref="NobreakModification"/> at.</param>
    /// <param name="length">The length to not break for.</param>
    public NobreakModification(int position, int length)
        : base(position)
    {
        this.length = length;
    }

    /// <inheritdoc/>
    internal override void Apply(CombinerContext context, ref ReadOnlySpan<char> buffer)
    {
        context.Nobreaks.Add(new Structs.NobreakInfo()
        {
            Start = this.Position,
            End = this.length,
        });
    }
}