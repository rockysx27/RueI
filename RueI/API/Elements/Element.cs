namespace RueI.API.Elements;

using System;

using RueI.API.Parsing;

/// <summary>
/// Represents an <see cref="Element"/> within a display.
/// </summary>
public abstract class Element
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Element"/> class.
    /// </summary>
    /// <param name="position">The vertical position of the <see cref="Element"/> from 0 to 1000.</param>
    public Element(float position)
    {
        this.Position = position;
    }

    /// <summary>
    /// Gets a value indicating whether or not the position of this element will be pushed back by newlines.
    /// </summary>
    public bool AdjustPosition { get; init; } = false;

    /// <summary>
    /// Gets the vertical position of the element, from 0 (the bottom of the screen) to 1000 (the top of the screen).
    /// </summary>
    public float Position { get; }

    /// <summary>
    /// Gets the priority of the hint (determining if it shows above another hint). A higher value indicates that the hint will show above another hint.
    /// </summary>
    /// <remarks>
    /// The default <see cref="ZIndex"/> is 1.
    /// </remarks>
    public int ZIndex { get; init; } = 1;

    /// <summary>
    /// Gets the <see cref="Parsing.ParsedData"/> for this element.
    /// </summary>
    /// <param name="hub">The <see cref="ReferenceHub"/> to get the data for.</param>
    /// <returns>The <see cref="ParsedData"/> of this <see cref="Element"/>, obtained through <see cref="Parser.Parse(string)"/>.</returns>
    protected internal abstract ParsedData GetParsedData(ReferenceHub hub);
}