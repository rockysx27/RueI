namespace RueI.API.Elements;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using RueI.API.Elements.Enums;
using RueI.API.Elements.Parameters;
using RueI.API.Parsing;

using UnityEngine;

/// <summary>
/// Represents text within a <see cref="Display"/>.
/// </summary>
public abstract class Element
{
    private ReadOnlyCollection<ContentParameter>? parameters;

    /// <summary>
    /// Initializes a new instance of the <see cref="Element"/> class.
    /// </summary>
    /// <param name="position">The vertical position of the <see cref="Element"/>, from 0 to 1000.</param>
    public Element(float position)
    {
        this.VerticalPosition = position;
    }

    /// <summary>
    /// Gets the vertical position of the element, from 0 (the bottom of the screen) to 1000 (the top of the screen).
    /// </summary>
    public float VerticalPosition { get; }

    /// <summary>
    /// Gets a value indicating whether align tags will align to the very edge of the screen, based on the resolution.
    /// </summary>
    public bool ResolutionBasedAlign { get; } = false;

    /// <summary>
    /// Gets or initializes the behavior of <c>noparse</c> tags in the <see cref="Element"/>.
    /// </summary>w
    /// <remarks>
    /// This allows for custom behavior when parsing certain values in the text of the <see cref="Element"/>.
    /// This prevents players from bypassing <c>noparse</c> and breaking hints.
    /// The default value is <see cref="NoparseSettings.ParsesNone"/>. It is recommended to
    /// keep this value as the default.
    /// </remarks>
    public NoparseSettings NoparseSettings { get; init; } = NoparseSettings.ParsesNone;

    /// <summary>
    /// Gets or initializes an animated override for the vertical position.
    /// </summary>
    /// <remarks>
    /// If not <see langword="null"/>, <see cref="VerticalPosition"/> will be ignored.
    /// </remarks>
    public AnimatedValue? AnimatedPosition { get; init; } = null;

    /// <summary>
    /// Gets or initializes the vertical alignment of the element.
    /// </summary>
    /// <remarks>
    /// The default behavior is <see cref="VerticalAlign.Down"/>.
    /// </remarks>
    public VerticalAlign VerticalAlign { get; init; } = VerticalAlign.Down;

    /// <summary>
    /// Gets or initializes the priority of the hint. A higher value indicates that the hint will show above another hint.
    /// </summary>
    /// <remarks>
    /// The default <see cref="ZIndex"/> is 1. If two elements have the same <see cref="ZIndex"/>, the most
    /// recently added element will show above the other element.
    /// </remarks>
    public int ZIndex { get; init; } = 1;

    /// <summary>
    /// Gets or initializes the parameters of the element.
    /// </summary>
    /// <remarks>
    /// Setting this property through <see langword="init"/> (i.e. <c>Parameters = [...]</c>) creates
    /// a copy of the <see cref="IReadOnlyList{T}"/>. Thus, changes to the original
    /// <see cref="IReadOnlyList{T}"/> will not be reflected.
    /// </remarks>
    public IReadOnlyList<ContentParameter>? Parameters
    {
        get => this.parameters;
        init => this.parameters = value?.ToList()?.AsReadOnly();
    }

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