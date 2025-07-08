namespace RueI.API.Parsing.Modifications;

using System.Collections.Generic;
using Mirror;
using RueI.API.Parsing.Structs;

/// <summary>
/// Represents the context of the <see cref="ElementCombiner"/> at a certain point.
/// </summary>
internal class CombinerContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CombinerContext"/> class.
    /// </summary>
    /// <param name="contentWriter">The <see cref="NetworkWriter"/> to write <see cref="Elements.Element"/> content to.</param>
    /// <param name="nobreaks">The <see cref="List{T}"/> of nobreak positions.</param>
    /// <param name="paramHandler">The <see cref="ParameterHandler"/> for the <see cref="ElementCombiner"/>.</param>
    internal CombinerContext(NetworkWriter contentWriter, List<NobreakInfo> nobreaks, ParameterHandler paramHandler)
    {
        this.ContentWriter = contentWriter;
        this.Nobreaks = nobreaks;
        this.ParameterHandler = paramHandler;
    }

    /// <summary>
    /// Gets the <see cref="NetworkWriter"/> that the content of <see cref="Elements.Element"/>s are written to.
    /// </summary>
    internal NetworkWriter ContentWriter { get; }

    /// <summary>
    /// Gets the <see cref="List{T}"/> of spots that the <see cref="ElementCombiner"/> should not break on.
    /// </summary>
    internal List<NobreakInfo> Nobreaks { get; }

    /// <summary>
    /// Gets the <see cref="Parsing.ParameterHandler"/> for the <see cref="ElementCombiner"/>.
    /// </summary>
    internal ParameterHandler ParameterHandler { get; }
}