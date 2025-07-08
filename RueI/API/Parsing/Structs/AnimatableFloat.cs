namespace RueI.API.Parsing.Structs;

using RueI.API.Elements.Parameters;

/// <summary>
/// Represents a float that may have an animated value.
/// </summary>
internal struct AnimatableFloat
{
    /// <summary>
    /// The <see cref="AnimatedParameter"/> that this <see cref="AnimatableFloat"/> refers to, or <see langword="null"/> if there is no parameter.
    /// </summary>
    internal AnimatedParameter? Parameter;

    /// <summary>
    /// The raw value, or a value to add to the curve (after multiplying).
    /// </summary>
    internal float AddendOrValue;

    /// <summary>
    /// A value to multiply the curve by.
    /// </summary>
    internal float Multiplier;

    /// <summary>
    /// Whether to take the absolute value of the curve.
    /// </summary>
    internal bool AbsoluteValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatableFloat"/> struct.
    /// </summary>
    /// <param name="value">The value to use for the <see cref="AnimatableFloat"/>.</param>
    internal AnimatableFloat(float value)
    {
        this.AddendOrValue = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatableFloat"/> struct.
    /// </summary>
    /// <param name="param">The parameter.</param>
    /// <param name="sum">A value to add to each keyframe.</param>
    /// <param name="multiplier">A value to multiply each keyframe's value by.</param>
    /// <param name="abs">Whether to take the absolute value of the values.</param>
    internal AnimatableFloat(AnimatedParameter param, float sum, float multiplier, bool abs)
    {
        this.Parameter = param;
        this.AddendOrValue = sum;
        this.Multiplier = multiplier;
        this.AbsoluteValue = abs;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatableFloat"/> struct.
    /// </summary>
    /// <param name="param">The parameter to use.</param>
    internal AnimatableFloat(AnimatedParameter param)
    {
        this.Parameter = param;
        this.Multiplier = 1;
    }

    /// <summary>
    /// Gets an invaldi <see cref="AnimatableFloat"/>.
    /// </summary>
    public static AnimatableFloat Invalid => new(float.NaN);

    /// <summary>
    /// Gets a value indicating whether this <see cref="AnimatableFloat"/> is animated.
    /// </summary>
    public readonly bool IsAnimated => this.Parameter != null;

    /// <summary>
    /// Gets a value indicating whether this <see cref="AnimatableFloat"/> is invalid.
    /// </summary>
    public readonly bool IsInvalid => float.IsNaN(this.AddendOrValue);

    /// <summary>
    /// Gets the inverse of this <see cref="AnimatableFloat"/>.
    /// </summary>
    public readonly AnimatableFloat Inverse => this with { Multiplier = -this.Multiplier };
}