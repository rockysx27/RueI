namespace RueI.API.Elements.Parameters;

using global::Utils.Networking;
using Mirror;

/// <summary>
/// Represents a parameter that is replaced by the name of an item.
/// </summary>
/// <remarks>
/// This is the RueI equivalent of the base-game <see cref="Hints.ItemHintParameter"/>.
/// </remarks>
public class ItemParameter : ContentParameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ItemParameter"/> class.
    /// </summary>
    /// <param name="itemType">The <see cref="ItemType"/> of the item.</param>
    public ItemParameter(ItemType itemType)
    {
        this.ItemType = itemType;
    }

    /// <summary>
    /// Gets the <see cref="ItemType"/> of the item.
    /// </summary>
    public ItemType ItemType { get; }

    /// <inheritdoc/>
    internal override HintParameterReaderWriter.HintParameterType HintParameterType => HintParameterReaderWriter.HintParameterType.Item;

    /// <inheritdoc/>
    internal override void Write(NetworkWriter writer)
    {
        writer.WriteInt((int)this.ItemType);
    }
}