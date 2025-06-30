namespace RueI.API.Parsing;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RueI.API.Elements;
using RueI.Utils;

/// <summary>
/// Represents the context of the parser.
/// </summary>
internal class ParserContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParserContext"/> class.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="element">The element to parse.</param>
    public ParserContext(string text, Element element)
    {
        this.Element = element;
        this.Text = text;
    }

    /// <summary>
    /// Gets the text of the parser.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the element of the parser.
    /// </summary>
    public Element Element { get; }

    /// <summary>
    /// Gets the current position of the parser.
    /// </summary>
    public int Position { get; set; } = 0;

    /// <summary>
    /// Gets the size of the parser, or <see cref="float.NaN"/> if no size is set.
    /// </summary>
    public float Size { get; private set; } = float.NaN;

    /// <summary>
    /// Gets the line height of the parser, or <see cref="float.NaN"/> if no line height is set.
    /// </summary>
    public float LineHeight { get; private set; } = float.NaN;

    /// <summary>
    /// Gets a value indicating whether tags are in noparse.
    /// </summary>
    public bool NoParse { get; private set; } = false;

    /// <summary>
    /// Gets the total offset of the text.
    /// </summary>
    public float TotalOffset { get; private set; } = 0;

    private float ActualLineHeight
    {
        get
        {
            if (!float.IsNaN(this.LineHeight))
            {
                return this.LineHeight;
            }
            else if (!float.IsNaN(this.Size))
            {
                return Constants.GetLineHeight(this.Size);
            }
            else
            {
                return Constants.GetLineHeight(Constants.EmSize);
            }
        }
    }

    /// <summary>
    /// Adds a linebreak the <see cref="ParserContext"/>.
    /// </summary>
    public void AddLinebreak()
    {
        this.TotalOffset += this.ActualLineHeight;
    }

    /// <summary>
    /// Creates a <see cref="ReadOnlySpan{T}"/> for this <see cref="ParserContext"/>
    /// that starts at <see cref="Position"/>, inclusive,
    /// and ends at <paramref name="until"/>, exclusive.
    /// </summary>
    /// <param name="until">The position to end before.</param>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> from <see cref="Position"/>
    /// to <paramref name="until"/>.</returns>
    public ReadOnlySpan<char> Slice(int until) => this.Text[this.Position..until];
}