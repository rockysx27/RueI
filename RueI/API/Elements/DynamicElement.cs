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
    /// <param name="position"><inheritdoc cref="Element(float)" path="/param[@name='position']"/></param>
    /// <param name="contentGetter">A <see cref="Func{T, TResult}"/> that takes in a <see cref="ReferenceHub"/>
    /// and returns a <see langword="string"/> to use for the content.</param>
    public DynamicElement(float position, Func<ReferenceHub, string> contentGetter)
        : base(position)
    {
        this.ContentGetter = contentGetter ?? throw new ArgumentNullException(nameof(contentGetter));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicElement"/> class.
    /// </summary>
    /// <param name="position"><inheritdoc cref="Element(float)" path="/param[@name='position']"/></param>
    /// <param name="contentGetter">A <see cref="Func{TResult}"/> that returns a <see langword="string"/> to use for the content.</param>
    public DynamicElement(float position, Func<string> contentGetter)
        : base(position)
    {
        if (contentGetter == null)
        {
            throw new ArgumentNullException(nameof(contentGetter));
        }

        this.ContentGetter = _ => contentGetter();
    }

    /// <summary>
    /// Gets or initializes an interval for how often a <see cref="Display"/> with this <see cref="DynamicElement"/>
    /// should be automatically updated, or <see langword="null"/> if the <see cref="DynamicElement"/> should not
    /// automatically update a <see cref="Display"/>.
    /// </summary>
    public TimeSpan? UpdateInterval { get; init; }

    /// <summary>
    /// Gets the <see cref="Func{T, TResult}"/> used to obtain data for this <see cref="DynamicElement"/>.
    /// </summary>
    protected Func<ReferenceHub, string> ContentGetter { get; }

    /// <inheritdoc/>
    protected internal override ParsedData GetParsedData(ReferenceHub hub) => Parser.Parse(this.ContentGetter(hub), this);
}