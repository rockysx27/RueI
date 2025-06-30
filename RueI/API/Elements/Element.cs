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
    /// Gets a value indicating whether noparse will parse escape sequences.
    /// </summary>
    /// <remarks>
    /// <c>&lt;noparse&gt;</c> tags, according to
    /// normal hint rules, will still parse escape sequences, such as \u003c or <c>\n</c> (not the singular character).
    /// The default value is <see langword="false"/>.
    /// </remarks>
    public bool NoparseParsesEscapeSequences { get; init; } = false;

    /// <summary>
    /// Gets a value indicating whether noparse will parse format items.
    /// </summary>
    /// <remarks>
    /// By default, &lt;noparse&gt; tags will still parse format items and braces, such as {0} or {.
    /// Setting this to <see langword="false"/> disables this behavior. The default value is <see langword="false"/>.
    /// </remarks>
    public bool NoparseParsesFormatItems { get; init; } = false;

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