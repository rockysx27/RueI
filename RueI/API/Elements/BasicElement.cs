namespace RueI.API.Elements;

using System;

using RueI.API.Parsing;

/// <summary>
/// Represents a basic element with fixed text.
/// </summary>
/// <remarks>
/// The <see cref="BasicElement"/> is a simple element with fixed text.
/// To update the text of a <see cref="BasicElement"/>, you can send the player
/// a new <see cref="BasicElement"/> with the same <see cref="Tag"/>.
/// </remarks>
public class BasicElement : Element
{
    private readonly string content;
    private ParsedData? parsedData;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicElement"/> class.
    /// </summary>
    /// <param name="position">The position of the element.</param>
    /// <param name="content">The content of the element.</param>
    public BasicElement(float position, string content)
        : base(position)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        this.content = content;
    }

    /// <inheritdoc/>
    protected internal sealed override ParsedData GetParsedData(ReferenceHub hub)
    {
        this.parsedData ??= Parser.Parse(this.content, this);

        return this.parsedData.Value;
    }
}