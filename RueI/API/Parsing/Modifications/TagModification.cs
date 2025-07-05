namespace RueI.API.Parsing.Modifications;

using System;
using Mirror;
using RueI.API.Parsing.Structs;
using RueI.Utils.Extensions;

internal class TagModification : SkipNextModification
{
    private string tagName;
    private float value;

    internal TagModification(string tagName, int skipCount, float value)
        : base(skipCount)
    {
        this.tagName = tagName;
        this.value = value;
    }

    internal override void Apply(ParserContext context, ref Span<char> buffer)
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