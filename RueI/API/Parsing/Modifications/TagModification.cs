namespace RueI.API.Parsing.Modifications;

using System;
using Mirror;
using RueI.Utils.Extensions;

/// <summary>
/// Represents a modification that adds a tag.
/// </summary>
internal class TagModification : SkipNextModification
{
    private readonly string tagName;
    private readonly float value;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagModification"/> class.
    /// </summary>
    /// <param name="position">The position to add the <see cref="TagModification"/> at.</param>
    /// <param name="skipCount"><inheritdoc cref="SkipNextModification(int, int)" path="/param[@name='skipCount']"/></param>
    /// <param name="tagName">The name of the tag.</param>
    /// <param name="value">The value of the tag.</param>
    internal TagModification(int position, int skipCount, string tagName, float value)
        : base(position, skipCount)
    {
        this.tagName = tagName;
        this.value = value;
    }

    /// <inheritdoc/>
    internal override void Apply(CombinerContext context, ref ReadOnlySpan<char> buffer)
    {
        NetworkWriter writer = context.ContentWriter;

        writer.WriteUtf8Char('<');
        writer.WriteStringNoSize(this.tagName);
        writer.WriteUtf8Char('=');
        writer.WriteFloatAsString(this.value);

        writer.WriteUtf8Char('e');
        writer.WriteUtf8Char('>');

        base.Apply(context, ref buffer);
    }
}