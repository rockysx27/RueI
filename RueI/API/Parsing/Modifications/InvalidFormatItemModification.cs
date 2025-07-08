namespace RueI.API.Parsing.Modifications;

using System;

using RueI.Utils.Extensions;

/// <summary>
/// Represents a <see cref="Modification"/> that adds an invalid format item.
/// </summary>
internal class InvalidFormatItemModification : SkipNextModification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidFormatItemModification"/> class.
    /// </summary>
    /// <param name="position">The position to add the <see cref="FormatItemModification"/> at.</param>
    /// <param name="skipCount"><inheritdoc cref="SkipNextModification(int, int)" path="/param[@name='skipCount']"/></param>
    internal InvalidFormatItemModification(int position, int skipCount)
        : base(position, skipCount)
    {
    }

    /// <inheritdoc/>
    internal override void Apply(CombinerContext context, ref ReadOnlySpan<char> buffer)
    {
        context.ContentWriter.WriteFormatItemNoBreak(int.MaxValue, context.Nobreaks);

        base.Apply(context, ref buffer);
    }
}