namespace RueI.API.Parsing.Modifications;

using System;
using Mirror;
using RueI.API.Parsing;
using RueI.API.Parsing.Enums;
using RueI.Utils;
using RueI.Utils.Extensions;

/// <summary>
/// Represents a modification that adds a tag.
/// </summary>
internal class TagModification : SkipNextModification
{
    private readonly RichTextTag tagType;
    private readonly float value;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagModification"/> class.
    /// </summary>
    /// <param name="position">The position to add the <see cref="TagModification"/> at.</param>
    /// <param name="skipCount"><inheritdoc cref="SkipNextModification(int, int)" path="/param[@name='skipCount']"/></param>
    /// <param name="tagType">The type of the tag.</param>
    /// <param name="value">The value of the tag.</param>
    internal TagModification(int position, int skipCount, RichTextTag tagType, float value)
        : base(position, skipCount)
    {
        this.tagType = tagType;
        this.value = value;
    }

    /// <inheritdoc/>
    internal override void Apply(CombinerContext context, ref ReadOnlySpan<char> buffer)
    {
        NetworkWriter writer = context.ContentWriter;

        writer.WriteUtf8Char('<');
        writer.WriteStringNoSize(Parser.TagNames[this.tagType]);
        writer.WriteUtf8Char('=');
        writer.WriteFloatAsString(this.value / Constants.EmSize);

        writer.WriteUtf8Char('e');
        writer.WriteUtf8Char('>');

        base.Apply(context, ref buffer);
    }
}