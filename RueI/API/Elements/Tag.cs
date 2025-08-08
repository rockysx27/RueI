namespace RueI.API.Elements;

using System;

/// <summary>
/// Represents a unique identifier for an element within a <see cref="RueDisplay"/>. This class
/// cannot be inherited.
/// </summary>
/// <remarks>
/// A <see cref="RueDisplay"/> can only have one element with a specific <see cref="Tag"/> at a time. If two <see cref="Tag"/>s are created with
/// the same <see langword="string"/>, they are considered equal. You can use the <see cref="Tag()"/> constructor to create a tag that
/// will only be equal to itself.
/// </remarks>
public sealed class Tag : IEquatable<Tag>
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
    /// Checks two <see cref="Tag"/>s to see if they are equal.
    /// </summary>
    /// <param name="first">The first <see cref="Tag"/> to check.</param>
    /// <param name="second">The second <see cref="Tag"/> to check.</param>
    /// <returns> <see langword="true"/> if both <paramref name="first"/> and <paramref name="second"/>
    /// refer to the same tag or both have the same <see cref="Id"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Tag first, Tag second) => first.Equals(second);

    /// <summary>
    /// Checks two <see cref="Tag"/>s to see if they are not equal.
    /// </summary>
    /// <param name="first">The first <see cref="Tag"/> to check.</param>
    /// <param name="second">The second <see cref="Tag"/> to check.</param>
    /// <returns> <see langword="true"/> if <paramref name="first"/> and <paramref name="second"/>
    /// do not refer to the same tag and have a different <see cref="Id"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Tag first, Tag second) => !first.Equals(second);

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
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="Tag"/> and has the same <see cref="Id"/>, or if <see cref="Tag"/>
    /// is <paramref name="obj"/>; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object obj) => this.Equals(obj as Tag);

    /// <summary>
    /// Determines if this <see cref="Tag"/> is equivalent to another <see cref="Tag"/>.
    /// </summary>
    /// <param name="other">A <see cref="Tag"/> to check for equality.</param>
    /// <returns><see langword="true"/> if <see cref="Tag"/> is <paramref name="other"/> or
    /// has the same <see cref="Id"/>; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Tag? other) => other is not null && ((this.Id?.Equals(other.Id) ?? false) || ReferenceEquals(this, other));
}