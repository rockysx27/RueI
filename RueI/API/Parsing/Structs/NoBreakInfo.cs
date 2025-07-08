namespace RueI.API.Parsing.Structs;

/// <summary>
/// Represents a position where the batcher should not break.
/// </summary>
internal struct NobreakInfo
{
    /// <summary>
    /// The position at which to prevent breaking, inclusive.
    /// </summary>
    public int Start;

    /// <summary>
    /// The position at which to allow breaking.
    /// </summary>
    public int End;
}