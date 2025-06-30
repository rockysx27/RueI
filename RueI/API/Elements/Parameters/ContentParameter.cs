namespace RueI.API.Elements.Parameters;

using global::Utils.Networking;

using Mirror;

/// <summary>
/// Represents a parameter for an element.
/// </summary>
public abstract class ContentParameter
{
    /// <summary>
    /// Gets the <see cref="HintParameterReaderWriter.HintParameterType"/>
    /// for this <see cref="ContentParameter"/>.
    /// </summary>
    internal abstract HintParameterReaderWriter.HintParameterType HintParameterType { get; }

    /// <summary>
    /// Writes this <see cref="ContentParameter"/> to a <see cref="NetworkWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="NetworkWriter"/> to write to.</param>
    internal abstract void Write(NetworkWriter writer);
}