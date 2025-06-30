namespace RueI.Utils.Extensions;

using RueI.API.Elements.Enums;

/// <summary>
/// Provides extensions for RueI enums.
/// </summary>
internal static class EnumExtensions
{
    /// <summary>
    /// Quickly determines if a <see cref="NoparseSettings"/> has another <see cref="NoparseSettings"/> as a flag.
    /// </summary>
    /// <param name="settings">The <see cref="NoparseSettings"/> to check the flag for.</param>
    /// <param name="flag">The flag to check.</param>
    /// <returns><see langword="true"/> if <paramref name="settings"/> has <paramref name="flag"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method should always be used in place of <see cref="System.Enum.HasFlag(System.Enum)"/>,
    /// as this method is significantly faster.
    /// </remarks>
    public static bool HasFlagFast(this NoparseSettings settings, NoparseSettings flag) => (settings & flag) == flag;
}