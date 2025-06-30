namespace RueI.API.Parsing.Modifications;

using System.Collections;

/// <summary>
/// Represents a modification to the text of an element.
/// </summary>
internal struct Modification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Modification"/> struct.
    /// </summary>
    /// <param name="type">The type of the modification.</param>
    /// <param name="position">The position of the modification.</param>
    /// <param name="additionalInfo">Any additional info.</param>
    public Modification(ModificationType type, int position, int additionalInfo)
        : this(type, position)
    {
        this.AdditionalInfo = additionalInfo;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Modification"/> struct.
    /// </summary>
    /// <param name="type">The type of the modification.</param>
    /// <param name="position">The position of the modification.</param>
    public Modification(ModificationType type, int position)
    {
        this.Type = type;
        this.Position = position;
    }

    /// <summary>
    /// Defines the type of a modification.
    /// </summary>
    public enum ModificationType
    {
        /// <summary>
        /// An additional backslash needs to be added.
        /// </summary>
        AdditionalBackslash,

        /// <summary>
        /// An additional forwards bracket needs to be added.
        /// </summary>
        AdditionalForwardBracket,

        /// <summary>
        /// An additional backwards bracket needs to be added.
        /// </summary>
        AdditionalBackwardsBracket,

        /// <summary>
        /// There should be no breaks for the given length.
        /// </summary>
        DoNotBreakFor,

        /// <summary>
        /// A noparse tag should be inserted.
        /// </summary>
        InsertNoparse,

        /// <summary>
        /// A closing noparse tag should be inserted.
        /// </summary>
        InsertCloseNoparse,

        /// <summary>
        /// The next <see cref="AdditionalInfo"/> characters should be skipped.
        /// </summary>
        SkipNext,

        /// <summary>
        /// There is a format item with the given number.
        /// </summary>
        FormatItem,

        /// <summary>
        /// There is an invalid format item.
        /// </summary>
        InvalidFormatItem,
    }

    /// <summary>
    /// Gets the type of the modification.
    /// </summary>
    public readonly ModificationType Type { get; }

    /// <summary>
    /// Gets the position of the modification.
    /// </summary>
    public readonly int Position { get; }

    /// <summary>
    /// Gets any additional info about the modification.
    /// </summary>
    public readonly int AdditionalInfo { get; }
}