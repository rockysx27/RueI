namespace RueI.API.Elements;

using System;

using RueI.API.Parsing;

/// <summary>
/// Represents a basic element with fixed text.
/// </summary>
/// <remarks>
/// As its name suggests, the <see cref="BasicElement"/> is a simple element
/// that is good enough for most use cases.
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