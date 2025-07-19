namespace RueI.API.Parsing.Modifications;

using System;
using System.Text;
using RueI.API.Parsing;
using RueI.Utils.Extensions;

/// <summary>
/// Adds a closing noparse tag.
/// </summary>
internal class CloseNoparseModification : Modification
{
    private static readonly byte[] TagBytes = Encoding.UTF8.GetBytes("</noparse>");

    /// <summary>
    /// Initializes a new instance of the <see cref="CloseNoparseModification"/> class.
    /// </summary>
    /// <param name="position">The position at which to add the closing noparse tag.</param>
    internal CloseNoparseModification(int position)
        : base(position)
    {
    }

    /// <inheritdoc/>
    internal override void Apply(CombinerContext context, ref ReadOnlySpan<char> buffer)
    {
        context.ContentWriter.WriteBytes(TagBytes, false);
    }
}