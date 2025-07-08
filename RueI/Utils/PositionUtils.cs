namespace RueI.Utils;

using RueI.API.Elements;
using RueI.API.Elements.Parameters;
using RueI.API.Parsing.Structs;

/// <summary>
/// Provides utilities for working with <see cref="Element"/> positions.
/// </summary>
public class PositionUtils
{
    private const float BaselineAddend = 755f;
    private const float BaselineMultiplier = -2.14f;

    /// <summary>
    /// Converts a scaled position (0-1000) to a baseline position (offset from the baseline).
    /// </summary>
    /// <param name="pos">The scaled position to convert.</param>
    /// <returns>The converted position.</returns>
    public static float ScaledToBaseline(float pos) => (pos * BaselineMultiplier) + BaselineAddend;

    /// <summary>
    /// Converts a baseline position (offset from the baseline) to a scaled position (0-1000).
    /// </summary>
    /// <param name="pos">The baseline position to convert.</param>
    /// <returns>The converted position.</returns>
    public static float BaselineToScaled(float pos) => (pos - BaselineAddend) / -BaselineMultiplier;

    /// <summary>
    /// Creates a <see cref="AnimatableFloat"/> for a <see cref="AnimatedParameter"/> as a baseline position.
    /// </summary>
    /// <param name="parameter">The <see cref="AnimatedParameter"/> for the <see cref="AnimatableFloat"/>.</param>
    /// <returns>The <see cref="AnimatableFloat"/> that wraps the <see cref="AnimatableFloat"/> in baseline position form.</returns>
    internal static AnimatableFloat ScaledToBaselineParameter(AnimatedParameter parameter) => new(parameter, BaselineAddend, BaselineMultiplier, false);
}