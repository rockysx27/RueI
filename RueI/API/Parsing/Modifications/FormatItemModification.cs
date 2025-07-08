namespace RueI.API.Parsing.Modifications;

using System;

using RueI.Utils.Extensions;

/// <summary>
/// Represents a <see cref="Modification"/> that adds a format item.
/// </summary>
internal class FormatItemModification : SkipNextModification
{
    private readonly int paramId;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatItemModification"/> class.
    /// </summary>
    /// <param name="position">The position to add the <see cref="FormatItemModification"/> at.</param>
    /// <param name="skipCount"><inheritdoc cref="SkipNextModification(int, int)" path="/param[@name='skipCount']"/></param>
    /// <param name="paramId">The ID of the parameter (e.g. the 0 in {0}).</param>
    internal FormatItemModification(int position, int skipCount, int paramId)
        : base(position, skipCount)
    {
        this.paramId = paramId;
    }

    /// <inheritdoc/>
    internal override void Apply(CombinerContext context, ref ReadOnlySpan<char> buffer)
    {
        int index = context.ParameterHandler.MappedElementParameterId(this.paramId);

        context.ContentWriter.WriteFormatItemNoBreak(index, context.Nobreaks);

        base.Apply(context, ref buffer);
    }
}