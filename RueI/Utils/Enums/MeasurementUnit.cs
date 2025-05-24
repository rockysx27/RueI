namespace RueI.Utils.Enums;

/// <summary>
/// Represents the unit used for a measurement parameter.
/// </summary>
public enum MeasurementUnit
{
    /// <summary>
    /// Indicates that the measurement is in pixels.
    /// </summary>
    Pixels,

    /// <summary>
    /// Indicates that the measurement is a percentage of the default.
    /// </summary>
    Percentage,

    /// <summary>
    /// Indicates that the measurement is in ems.
    /// </summary>
    /// <remarks>
    /// An em in SCP:SL is equal to 34.7 pixels.
    /// </remarks>
    Ems,
}