namespace RueI.API.Parsing.Modifications;

using RueI.API.Parsing.Structs;
using RueI.Utils.Extensions;

/// <summary>
/// Represents a modification that adds a linebreak with an animated line height.
/// </summary>
internal class AnimatedLinebreakModification : LinebreakModification
{
    private readonly AnimatableFloat lineHeight;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatedLinebreakModification"/> class.
    /// </summary>
    /// <param name="lineHeight">The <see cref="AnimatableFloat"/> to use as the line height for the modification.</param>
    /// <param name="position">The position to add the <see cref="AnimatedLinebreakModification"/> at.</param>
    /// <param name="skipCount"><inheritdoc cref="LinebreakModification(int, int)" path="/param[@name='skipCount']"/></param>
    internal AnimatedLinebreakModification(in AnimatableFloat lineHeight, int position, int skipCount)
        : base(position, skipCount)
    {
        this.lineHeight = lineHeight;
    }

    /// <inheritdoc/>
    protected override void WriteLineHeightValue(CombinerContext context)
    {
        int id = context.ParameterHandler.AddAnimatableFloat(this.lineHeight);

        context.ContentWriter.WriteFormatItemNoBreak(id, context.Nobreaks);
    }
}