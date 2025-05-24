namespace RueI.API.Parsing;

using System.Collections.Generic;

using Hints;

using Mirror;

using RueI.API.Elements;

/// <summary>
/// Combines multiple <see cref="Elements.Element"/>s into a single <see cref="TextHint"/>.
/// </summary>
internal static class ElementCombiner
{
    private static IEnumerable<Element> elements;

    /// <summary>
    /// Used to avoid unnecessary copies by writing directly into the <see cref="NetworkWriter"/>.
    /// </summary>
    private class RueIHint : TextHint
    {
        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            int oldPos = writer.Position;


        }
    }

    internal static void Combine(IEnumerable<Element> elements)
    {
        
    }
}