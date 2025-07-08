namespace RueI.API.Parsing.Modifications;

using System;

using Mirror;

using RueI.API.Parsing.Structs;
using RueI.Utils.Extensions;

/// <summary>
/// Represents a <see cref="Modification"/> that adds a tag with an animated value.
/// </summary>
internal class AnimatedTagModification : SkipNextModification
{
    private readonly string tagName;
    private AnimatableFloat value;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatedTagModification"/> class.
    /// </summary>
    /// <param name="position">The position to add the <see cref="AnimatedTagModification"/> at.</param>
    /// <param name="skipCount"><inheritdoc cref="SkipNextModification(int, int)" path="/param[@name='skipCount']"/>.</param>
    /// <param name="tagName">The name of the tag.</param>
    /// <param name="value">The <see cref="AnimatableFloat"/> value for the tag.</param>
    internal AnimatedTagModification(int position, int skipCount, string tagName, in AnimatableFloat value)
        : base(position, skipCount)
    {
        this.tagName = tagName;
        this.value = value;
    }

    /// <inheritdoc/>
    internal override void Apply(CombinerContext context, ref ReadOnlySpan<char> buffer)
    {
        // TODO: remove some of this excess code
        NetworkWriter writer = context.ContentWriter;

        writer.WriteUtf8Char('<');
        writer.WriteStringNoSize(this.tagName);
        writer.WriteUtf8Char('=');

        int id = context.ParameterHandler.AddAnimatableFloat(this.value);

        writer.WriteFormatItemNoBreak(id, context.Nobreaks);

        writer.WriteUtf8Char('>');

        base.Apply(context, ref buffer);
    }
}