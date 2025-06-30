namespace RueI.API.Elements.Parameters;

/// <summary>
/// Represents a parameter that can be formatted.
/// </summary>
public abstract class FormattableParameter : ContentParameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormattableParameter"/> class.
    /// </summary>
    /// <param name="format">The format to use.</param>
    public FormattableParameter(string? format)
    {
        this.Format = format;
    }

    /// <summary>
    /// Gets the format used for the parameter.
    /// </summary>
    public string? Format { get; }
}