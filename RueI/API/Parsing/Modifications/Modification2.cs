namespace RueI.API.Parsing.Modifications;

using System;
using Mirror;

internal abstract class Modification2
{
    internal int Position { get; }

    public Modification2(int position)
    {
        this.Position = position;
    }

    internal abstract void Apply(ParserContext context, ref Span<char> buffer);
}