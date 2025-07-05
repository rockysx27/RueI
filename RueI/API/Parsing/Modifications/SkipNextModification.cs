namespace RueI.API.Parsing.Modifications;

using System;

internal class SkipNextModification : Modification2
{
    private readonly int count;

    internal SkipNextModification(int count)
    {
        this.count = count;
    }

    internal override void Apply(ParserContext context, ref Span<char> buffer)
    {
        buffer = buffer[this.count..];
    }
}