namespace RueI.Utils.Enums;

/// <summary>
/// Represents all of the options for the alignment of a string of text.
/// </summary>
public enum AlignStyle
{
    /// <summary>
    /// Indicates that the text should be left-aligned.
    /// </summary>
    Left,

    /// <summary>
    /// Indicates that the text should be center-aligned.
    /// </summary>
    Center,

    /// <summary>
    /// Indicates that the text should be right-aligned.
    /// </summary>
    Right,

    /// <summary>
    /// Indicates that every line should be stretched to fill the display area, excluding the last line.
    /// </summary>
    Justified,

    /// <summary>
    /// Indicates that every line should be stretched to fill the display area, including the last line.
    /// </summary>
    Flush,
}