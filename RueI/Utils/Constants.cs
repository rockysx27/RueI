namespace RueI.Utils;

/// <summary>
/// Provides hint-related constants for use within RueI.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Gets the em size of SCP:SL.
    /// </summary>
    internal const float EmSize = 34.7f;

    /// <summary>
    /// Gets the default line height, in pixels.
    /// </summary>
    internal const float DefaultLineHeight = 40.6640648767f;

    /// <summary>
    /// Gets the maximum legnth of a rich text tag before it is no longer parsed, including the ending angle brackets (&lt; and &gt;).
    /// </summary>
    internal const int MaxTagLength = 129;

    /// <summary>
    /// Gets the maximum value for a tag's value.
    /// </summary>
    internal const int MaxValueSize = 32768;

    /// <summary>
    /// Gets the default line height for a given size.
    /// </summary>
    /// <param name="size">The size, in pixels.</param>
    /// <returns>The default line height for that size, as a <see langword="float"/>.</returns>
    internal static float GetLineHeight(float size) => (1.117129f * size) + 1.13576f;
}