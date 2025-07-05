namespace RueI.API.Parsing.Modifications;

using System;
using Mirror;
using RueI.Utils.Extensions;

internal class FormatItemModification : SkipNextModification
{
    private readonly int paramId;

    internal FormatItemModification(int paramId, int count)
        : base(count)
    {
        this.paramId = paramId;
    }

    internal override void Apply(ParserContext context, ref Span<char> buffer)
    {
        int index = context.ParameterHandler.GetMappedElementParameterId(this.paramId);

        context.ContentWriter.WriteFormatItemNoBreak(index, context.Nobreaks);

        base.Apply(context, ref buffer);
    }
}