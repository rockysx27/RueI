namespace RueI.API.Elements.Enums;

/// <summary>
/// Specifies how an element should be aligned vertically when there are multiple lines.
/// </summary>
public enum VerticalAlign
{
    /// <summary>
    /// Positions the bottom of the last line to be at the vertical position. The previous lines will appear above it.
    /// </summary>
    Up,

    /// <summary>
    /// Positions the center of the text at the vertical position.
    /// </summary>
    /// <remarks>
    /// This is the default behavior for Secret Lab hints.
    /// </remarks>
    Center,

    /// <summary>
    /// Positions the top of the first line to be at the vertical position. Subsequent lines will appear below it.
    /// </summary>
    Down,
}