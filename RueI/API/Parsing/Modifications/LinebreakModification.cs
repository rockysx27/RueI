namespace RueI.API.Parsing.Modifications;

using System;
using System.Text;
using RueI.API.Parsing;
using RueI.Utils.Extensions;

/// <summary>
/// Represents a modification that adds a line height tag with a certain value.
/// </summary>
internal abstract class LinebreakModification : SkipNextModification
{
    private const string Prefix = "<line-height=";
    private const string Suffix = ">\n<line-height=0>";

    private static readonly byte[] PrefixBytes = Encoding.UTF8.GetBytes(Prefix);
    private static readonly byte[] SuffixBytes = Encoding.UTF8.GetBytes(Suffix);

    /// <summary>
    /// Initializes a new instance of the <see cref="LinebreakModification"/> class.
    /// </summary>
    /// <param name="position">The position to add the <see cref="LinebreakModification"/> at.</param>
    /// <param name="skipCount"><inheritdoc cref="SkipNextModification(int, int)" path="/param[@name='skipCount']"/></param>
    internal LinebreakModification(int position, int skipCount)
        : base(position, skipCount)
    {
    }

    /// <inheritdoc/>
    internal override void Apply(CombinerContext context, ref ReadOnlySpan<char> buffer)
    {
        context.ContentWriter.WriteBytes(PrefixBytes, false);
        this.WriteLineHeightValue(context);
        context.ContentWriter.WriteBytes(SuffixBytes, false);

        base.Apply(context, ref buffer);
    }

    /// <summary>
    /// Writes the value of the line-height tag for this <see cref="LinebreakModification"/>.
    /// </summary>
    /// <param name="context">The context of the <see cref="ElementCombiner"/>.</param>
    protected abstract void WriteLineHeightValue(CombinerContext context);
}