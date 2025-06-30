namespace RueI.API.Elements.Parameters;

using global::Utils.Networking;
using Mirror;

/// <summary>
/// Represents a parameter for a value that changes over time.
/// </summary>
/// <remarks>
/// This is the RueI equivalent of the base-game <see cref="Hints.AnimationCurveHintParameter"/>.
/// </remarks>
public class AnimatedParameter : FormattableParameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatedParameter"/> class.
    /// </summary>
    /// <param name="value">The <see cref="AnimatedValue"/> to use.</param>
    public AnimatedParameter(AnimatedValue value)
        : base(null)
    {
        if (value.IsNull)
        {
            // TODO: implement null check ig
        }

        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatedParameter"/> class
    /// with a given format.
    /// </summary>
    /// <param name="value">The <see cref="AnimatedValue"/> to use.</param>
    /// <param name="format">The format to use.</param>
    public AnimatedParameter(AnimatedValue value, string format)
        : base(format)
    {
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatedParameter"/> class
    /// with a given format and the option to round the value to an <see langword="int"/>.
    /// </summary>
    /// <param name="value">The <see cref="AnimatedValue"/> to use.</param>
    /// <param name="format">The format to use.</param>
    /// <param name="roundToInt">Whether to round the value to an <see langword="int"/>.</param>
    public AnimatedParameter(AnimatedValue value, string format, bool roundToInt)
        : this(value, format)
    {
        this.RoundToInt = roundToInt;
    }

    /// <summary>
    /// Gets a value indicating whether the float should be rounded to an <see langword="int"/>.
    /// </summary>
    public bool RoundToInt { get; }

    /// <summary>
    /// Gets the offset for the value.
    /// </summary>
    public double Offset { get; init; }

    /// <summary>
    /// Gets the <see cref="AnimatedValue"/> for the <see cref="AnimatedParameter"/>.
    /// </summary>
    public AnimatedValue Value { get; }

    /// <inheritdoc/>
    internal override HintParameterReaderWriter.HintParameterType HintParameterType => HintParameterReaderWriter.HintParameterType.AnimationCurve;

    /// <inheritdoc/>
    internal override void Write(NetworkWriter writer)
    {
        writer.WriteDouble(this.Offset);
        writer.WriteString(this.Format);
        writer.WriteBool(this.RoundToInt);
        this.Value.Write(writer);
    }

    private static void WriteFormat(NetworkWriter writer, char formatChar, int precision) => writer.WriteString(formatChar + precision.ToString());
}