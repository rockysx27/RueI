namespace RueI.API.Parsing.Modifications;

using RueI.Utils.Extensions;

/// <summary>
/// Represents a <see cref="LinebreakModification"/> with a raw, fixed <see langword="float"/>
/// as the line height.
/// </summary>
internal class RawLinebreakModification : LinebreakModification
{
    private readonly float lineHeight;

    /// <summary>
    /// Initializes a new instance of the <see cref="RawLinebreakModification"/> class.
    /// </summary>
    /// <param name="lineHeight">The line height that the linebreak should have.</param>
    /// <param name="position">The position to add the <see cref="RawLinebreakModification"/> at.</param>
    /// <param name="skipCount"><inheritdoc cref="LinebreakModification(int, int)" path="/param[@name='skipCount']"/></param>
    internal RawLinebreakModification(float lineHeight, int position, int skipCount)
        : base(position, skipCount)
    {
        this.lineHeight = lineHeight;
    }

    /// <inheritdoc/>
    protected override void WriteLineHeightValue(CombinerContext context)
    {
        context.ContentWriter.WriteFloatAsString(this.lineHeight);
    }
}