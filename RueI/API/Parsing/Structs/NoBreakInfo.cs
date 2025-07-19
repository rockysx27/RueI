namespace RueI.API.Parsing.Structs;

/// <summary>
/// Represents a position where the batcher should not break.
/// </summary>
/// <remarks>
/// The purpose of this class is mainly to prevent format items
/// (e.g. {0}) from being sent in parameters, which would break them.
/// </remarks>
internal struct NobreakInfo
{
    /// <summary>
    /// The position at which to prevent breaking, inclusive.
    /// </summary>
    public int Start;

    /// <summary>
    /// The length of the nobreak.
    /// </summary>
    public int Length;
}