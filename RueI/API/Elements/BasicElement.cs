namespace RueI.API.Elements;

using RueI.API.Parsing;

/// <summary>
/// Represents a basic element with fixed text.
/// </summary>
public class BasicElement : Element
{
    private static ParsedData parsedData;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicElement"/> class.
    /// </summary>
    /// <param name="content">The content of the element.</param>
    /// <param name="position">The position of the element.</param>
    public BasicElement(string content, float position)
        : base(position)
    {
        parsedData = Parser.Parse(content);
    }

    /// <inheritdoc/>
    protected internal sealed override ParsedData GetParsedData(ReferenceHub hub) => parsedData;
}