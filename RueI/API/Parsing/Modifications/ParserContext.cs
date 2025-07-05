namespace RueI.API.Parsing.Modifications;

using System.Collections.Generic;
using Mirror;
using RueI.API.Parsing.Structs;

internal class ParserContext
{
    public NetworkWriter ContentWriter;

    public List<NoBreakInfo> Nobreaks;

    public ParameterHandler ParameterHandler;
}