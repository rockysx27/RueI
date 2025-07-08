namespace RueI.API.Elements.Parameters;

using global::Utils.Networking;

using Mirror;

/// <summary>
/// Represents a parameter for an <see cref="Element"/>.
/// </summary>
/// <remarks>
/// A <see cref="ContentParameter"/> allows customized values on the client-side, such as replacing a value with a
/// keybind or changing a value over time. The parameters of an <see cref="Element"/> can be specified by setting the
/// <see cref="Element.Parameters"/> property when creating the <see cref="Element"/>. The corresponding format item (such as <c>{0}</c>) will
/// then be replaced on the client with the parameter at that index in <see cref="Element.Parameters"/>.
/// </remarks>
public abstract class ContentParameter
{
    /// <summary>
    /// Gets the <see cref="HintParameterReaderWriter.HintParameterType"/> for this <see cref="ContentParameter"/>.
    /// </summary>
    internal abstract HintParameterReaderWriter.HintParameterType HintParameterType { get; }

    /// <summary>
    /// Writes this <see cref="ContentParameter"/> to a <see cref="NetworkWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="NetworkWriter"/> to write to.</param>
    internal abstract void Write(NetworkWriter writer);
}