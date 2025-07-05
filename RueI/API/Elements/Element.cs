namespace RueI.API.Elements;

using RueI.API.Elements.Enums;
using RueI.API.Elements.Parameters;
using RueI.API.Parsing;
using UnityEngine;

/// <summary>
/// Represents an <see cref="Element"/> within a display.
/// </summary>
public abstract class Element
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Element"/> class.
    /// </summary>
    /// <param name="position">The vertical position of the <see cref="Element"/>, from 0 to 1000.</param>
    public Element(float position)
    {
        this.Position = position;
    }

    /// <summary>
    /// Gets the vertical position of the element, from 0 (the bottom of the screen) to 1000 (the top of the screen).
    /// </summary>
    public float Position { get; }

    /// <summary>
    /// Gets the behavior of <c>noparse</c> tags in the <see cref="Element"/>.
    /// </summary>
    /// <remarks>
    /// This allows for custom behavior when parsing certain values in the text of the <see cref="Element"/>.
    /// This prevents players from breaking hints and bypassing <c>noparse</c>.
    /// The default value is <see cref="NoparseSettings.ParsesNone"/>. It is recommended to
    /// keep this value as the default.
    /// </remarks>
    public NoparseSettings NoparseSettings { get; init; } = NoparseSettings.ParsesNone;

    /// <summary>
    /// Gets an animated override for the position.
    /// </summary>
    public AnimatedValue? AnimatedPosition { get; init; } = null;

    /// <summary>
    /// Gets the vertical alignment of the element.
    /// </summary>
    /// <remarks>
    /// The default behavior is <see cref="VerticalAlign.Down"/>.
    /// </remarks>
    public VerticalAlign VerticalAlign { get; init; } = VerticalAlign.Down;

    /// <summary>
    /// Gets the priority of the hint. A higher value indicates that the hint will show above another hint.
    /// </summary>
    /// <remarks>
    /// The default <see cref="ZIndex"/> is 1. If two elements have the same <see cref="ZIndex"/>, the most
    /// recently added element will show above the other element.
    /// </remarks>
    public int ZIndex { get; init; } = 1;

    /// <summary>
    /// Gets the <see cref="Parameters.ParameterList"/> of the element.
    /// </summary>
    public ParameterList ParameterList { get; init; }

    /// <summary>
    /// Gets the <see cref="ParsedData"/> for this element.
    /// </summary>
    /// <param name="hub">The <see cref="ReferenceHub"/> to get the data for.</param>
    /// <returns>The <see cref="ParsedData"/> of this <see cref="Element"/>, initially obtained through <see cref="Parser.Parse(string, Element)"/>.</returns>
    /// <remarks>
    /// The <see cref="GetParsedData(ReferenceHub)"/> method is called when a <see cref="Display"/>
    /// is updated and RueI combines every <see cref="Element"/> that belongs to it.
    /// Implementations for this method need not call <see cref="Parser.Parse(string, Element)"/> every
    /// time this method is called; instead, the <see cref="ParsedData"/> can be saved and reused.
    /// </remarks>
    protected internal abstract ParsedData GetParsedData(ReferenceHub hub);
}