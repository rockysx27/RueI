namespace RueI.API.Elements;

using System;
using RueI.API.Parsing;

/// <summary>
/// Represents an element that obtains its text from a supplied function and regenerates when a display is updated.
/// </summary>
public class DynamicElement : Element
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicElement"/> class.
    /// </summary>
    /// <param name="contentGetter">A <see cref="Func{T, TResult}"/> that takes in a <see cref="ReferenceHub"/>
    /// and returns a <see langword="string"/> to use for the content.</param>
    /// <param name="position"><inheritdoc cref="Element(float)" path="/param[@name='position']"/></param>
    public DynamicElement(Func<ReferenceHub, string> contentGetter, float position)
        : base(position)
    {
        this.ContentGetter = contentGetter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicElement"/> class.
    /// </summary>
    /// <param name="contentGetter">A <see cref="Func{TResult}"/> that returns a <see langword="string"/> to use for the content.</param>
    /// <param name="position"><inheritdoc cref="Element(float)" path="/param[@name='position']"/></param>
    public DynamicElement(Func<string> contentGetter, float position)
        : base(position)
    {
        this.ContentGetter = _ => contentGetter();
    }

    /// <summary>
    /// Gets the <see cref="Func{T, TResult}"/> used to obtain data for this <see cref="DynamicElement"/>.
    /// </summary>
    protected Func<ReferenceHub, string> ContentGetter { get; }

    /// <inheritdoc/>
    protected internal override ParsedData GetParsedData(ReferenceHub hub) => Parser.Parse(this.ContentGetter(hub));
}