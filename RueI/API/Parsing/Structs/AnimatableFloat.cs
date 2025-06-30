namespace RueI.API.Parsing.Structs;

using System.Collections.Generic;
using RueI.API.Elements.Parameters;
using UnityEngine;

/// <summary>
/// Represents a float that may have an animated value.
/// </summary>
internal struct AnimatableFloat
{
    /// <summary>
    /// The parameter id, or <c>-2</c> if there is no parameter.
    /// </summary>
    internal int ParameterId;

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
        this.ParameterId = -2;
        this.AddendOrValue = value;
    }

    internal AnimatableFloat(int id, float sum, float multiplier, bool abs)
    {
        this.ParameterId = id;
        this.AddendOrValue = sum;
        this.Multiplier = multiplier;
        this.AbsoluteValue = abs;
    }

    private AnimatableFloat(int id)
    {
        this.ParameterId = id;
        this.Multiplier = 1;
    }

    /// <summary>
    /// Gets an invaldi <see cref="AnimatableFloat"/>.
    /// </summary>
    public static AnimatableFloat Invalid => new(-1);

    /// <summary>
    /// Gets a value indicating whether this <see cref="AnimatableFloat"/> is animated.
    /// </summary>
    public readonly bool IsAnimated => this.ParameterId == -2;

    /// <summary>
    /// Gets a value indicating whether this <see cref="AnimatableFloat"/> is invalid.
    /// </summary>
    public readonly bool IsInvalid => this.ParameterId == -1;

    /// <summary>
    /// Gets the inverse of this <see cref="AnimatableFloat"/>.
    /// </summary>
    public readonly AnimatableFloat Inverse => this with { Multiplier = -this.Multiplier };
}