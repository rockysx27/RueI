namespace RueI.API.Elements;

/// <summary>
/// Represents a unique identifier for an element within a <see cref="Display"/>.
/// </summary>
/// <remarks>
/// A <see cref="Display"/> can only have one element with a specific <see cref="Tag"/> at a time. If two <see cref="Tag"/>s are created with
/// the same <see langword="string"/>, they are considered equal. You can use the <see cref="Tag()"/> constructor to create a tag that
/// will only be equal to itself.
/// </remarks>
public sealed class Tag
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Tag"/> class.
    /// </summary>
    public Tag()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tag"/> class with the given <see langword="string"/> as an ID.
    /// </summary>
    /// <param name="id">
    /// A <see langword="string"/> to use as an ID.
    /// </param>
    public Tag(string id)
    {
        this.Id = id;
    }

    /// <summary>
    /// Gets the ID of this <see cref="Tag"/>, or <see langword="null"/> if this <see cref="Tag"/> was not created with an ID.
    /// </summary>
    public string? Id { get; }

    /// <summary>
    /// Gets the hash code for this <see cref="Tag"/>.
    /// </summary>
    /// <returns>The hashcode.</returns>
    /// <remarks>
    /// If this <see cref="Tag"/> has a <see cref="Id"/>, this will be calculated using that <see langword="string"/>.
    /// </remarks>
    public override int GetHashCode()
    {
        return this.Id?.GetHashCode() ?? base.GetHashCode();
    }

    /// <summary>
    /// Returns a value indicating whether this <see cref="Tag"/> is equal to an <see langword="object"/>.
    /// </summary>
    /// <param name="obj">An <see langword="object"/> to check for equality.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="Tag"/> and has the same ID, or if <see cref="Tag"/>
    /// is <paramref name="obj"/>; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object obj)
    {
        // ensure that a null id != null id
        return obj is Tag tag && (tag.Id?.Equals(tag.Id) ?? false || ReferenceEquals(this, obj));
    }   
}