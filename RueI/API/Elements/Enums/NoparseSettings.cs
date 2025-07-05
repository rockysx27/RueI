namespace RueI.API.Elements.Enums;

using System;

/// <summary>
/// Defines the behavior of noparse tags in <see cref="Element"/> text.
/// </summary>
[Flags]
public enum NoparseSettings
{
    /// <summary>
    /// Noparse will parse escape sequences, such as <c>\n</c>.
    /// </summary>
    ParsesEscapeSequences = 1 << 0,

    /// <summary>
    /// Noparse will parse format items, such as <c>{0}</c>.
    /// </summary>
    ParsesFormatItems = 1 << 1,

    /// <summary>
    /// Noparse will parse none of the options.
    /// </summary>
    ParsesNone = 0,

    /// <summary>
    /// Noparse will parse all valid options.
    /// </summary>
    ParsesAll = ~0, // sets all bits
}