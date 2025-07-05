namespace RueI.API.Parsing.Modifications;

using System;
using System.Text;
using RueI.Utils.Extensions;

internal abstract class LinebreakModification : SkipNextModification
{
    private static readonly byte[] PrefixBytes;
    private static readonly byte[] SuffixBytes;

    static LinebreakModification()
    {
        const string Prefix = "<line-height=";
        const string Suffix = "e>\n</line-height>";

        Encoding encoding = Encoding.UTF8;

        PrefixBytes = encoding.GetBytes(Prefix);
        SuffixBytes = encoding.GetBytes(Suffix);
    }

    internal LinebreakModification(int skipCount)
        : base(skipCount)
    {
    }

    internal override void Apply(ParserContext context, ref Span<char> buffer)
    {
        context.ContentWriter.WriteBytes(PrefixBytes, false);
        this.WriteValue(context);
        context.ContentWriter.WriteBytes(SuffixBytes, false);

        base.Apply(context, ref buffer);
    }

    protected abstract void WriteValue(ParserContext context);
}