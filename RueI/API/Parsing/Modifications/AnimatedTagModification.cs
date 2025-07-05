namespace RueI.API.Parsing.Modifications;

using System;
using Mirror;
using RueI.API.Parsing.Structs;
using RueI.Utils.Extensions;

internal class AnimatedTagModification : SkipNextModification
{
    private string tagName;
    private AnimatableFloat value;

    internal AnimatedTagModification(string tagName, int skipCount, in AnimatableFloat value)
        : base(skipCount)
    {
        this.tagName = tagName;
        this.value = value;
    }

    internal override void Apply(ParserContext context, ref Span<char> buffer)
    {
        NetworkWriter writer = context.ContentWriter;

        int pos = writer.Position;

        writer.WriteUtf8Char('<');
        writer.WriteStringNoSize(this.tagName);
        writer.WriteUtf8Char('=');

        int id = context.ParameterHandler.AddAnimatableFloat(this.value);

        writer.WriteFormatItemNoBreak(id, context.Nobreaks);

        writer.WriteUtf8Char('>');

        base.Apply(context, ref buffer);
    }
}