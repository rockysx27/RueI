namespace RueI.API.Parsing;

using System.Collections.Generic;
using Mirror;
using RueI.API.Parsing.Structs;
using RueI.Utils;
using RueI.Utils.Collections;
using RueI.Utils.Extensions;

/// <summary>
/// Represents multiple <see cref="AnimatableFloat"/>s added together.
/// </summary>
internal class CumulativeFloat
{
    private readonly RefList<AnimatableFloat> curves = new();
    private float value;

    /// <summary>
    /// Initializes a new instance of the <see cref="CumulativeFloat"/> class.
    /// </summary>
    public CumulativeFloat()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CumulativeFloat"/> class
    /// from a given <see langword="float"/>.
    /// </summary>
    /// <param name="value">The <see langword="float"/> to initialize the <see cref="CumulativeFloat"/> with.</param>
    public CumulativeFloat(float value)
    {
        this.value = value;
    }

    private CumulativeFloat(RefList<AnimatableFloat> original, float value)
    {
        this.curves = original.Clone();
        this.value = value;
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="CumulativeFloat"/> is invalid.
    /// </summary>
    public bool IsInvalid => float.IsNaN(this.value);

    /// <summary>
    /// Adds a <see cref="CumulativeFloat"/> to this <see cref="CumulativeFloat"/>.
    /// </summary>
    /// <param name="cumulativeFloat">The <see cref="CumulativeFloat"/> to add.</param>
    public void Add(CumulativeFloat cumulativeFloat)
    {
        this.curves.AddList(cumulativeFloat.curves);
        this.value += cumulativeFloat.value;
    }

    /// <summary>
    /// Subtracts a <see cref="CumulativeFloat"/> from this <see cref="CumulativeFloat"/>.
    /// </summary>
    /// <param name="cumulativeFloat">The <see cref="CumulativeFloat"/> to subtract.</param>
    public void Subtract(CumulativeFloat cumulativeFloat)
    {
        this.curves.AddCapacity(cumulativeFloat.curves.Count);

        for (int i = 0; i < cumulativeFloat.curves.Count; i++)
        {
            ref AnimatableFloat animatableFloat = ref cumulativeFloat.curves[i];

            this.Add(animatableFloat.Inverse);
        }

        this.value -= cumulativeFloat.value;
    }

    /// <summary>
    /// Adds a <see cref="AnimatableFloat"/> to the <see cref="CumulativeFloat"/>.
    /// </summary>
    /// <param name="value">The <see cref="AnimatableFloat"/> to add.</param>
    public void Add(in AnimatableFloat value)
    {
        if (value.IsAnimated)
        {
            this.curves.Add(in value);
        }
        else
        {
            this.value += value.AddendOrValue;
        }
    }

    /// <summary>
    /// Subtracts a <see cref="AnimatableFloat"/> from the <see cref="CumulativeFloat"/>.
    /// </summary>
    /// <param name="value">The <see cref="AnimatableFloat"/> to subtract.</param>
    public void Subtract(in AnimatableFloat value)
    {
        if (value.IsAnimated)
        {
            this.curves.Add(value.Inverse);
        }
        else
        {
            this.value -= value.AddendOrValue;
        }
    }

    /// <summary>
    /// Adds a <see langword="float"/> to the <see cref="CumulativeFloat"/>.
    /// </summary>
    /// <param name="value">The <see langword="float"/> to add.</param>
    public void Add(float value)
    {
        this.value += value;
    }

    /// <summary>
    /// Divides all values in the <see cref="CumulativeFloat"/> by a given <see langword="float"/>.
    /// </summary>
    /// <param name="value">The <see langword="float"/> to divide by.</param>
    public void Divide(float value)
    {
        for (int i = 0; i < this.curves.Count; i++)
        {
            this.curves[i].Multiplier /= value;
            this.curves[i].AddendOrValue /= value;
        }

        this.value /= value;
    }

    /// <summary>
    /// Multiplies all values in the <see cref="CumulativeFloat"/> by a given <see langword="float"/>.
    /// </summary>
    /// <param name="value">The <see langword="float"/> to multiply by.</param>
    public void Multiply(float value)
    {
        for (int i = 0; i < this.curves.Count; i++)
        {
            this.curves[i].Multiplier *= value;
            this.curves[i].AddendOrValue *= value;
        }

        this.value *= value;
    }

    /// <summary>
    /// Clears the <see cref="CumulativeFloat"/>.
    /// </summary>
    public void Clear()
    {
        this.curves.Clear();
        this.value = 0;
    }

    /// <summary>
    /// Clones this <see cref="CumulativeFloat"/>.
    /// </summary>
    /// <returns>A clone of this <see cref="CumulativeFloat"/>.</returns>
    public CumulativeFloat Clone() => new(this.curves, this.value);

    /// <summary>
    /// Writes this <see cref="CumulativeFloat"/> as a series of line-height tags and linebreaks.
    /// </summary>
    /// <param name="writer">The <see cref="NetworkWriter"/> to write to.</param>
    /// <param name="paramHandler">The <see cref="ParameterHandler"/> of the <see cref="ElementCombiner"/>.</param>
    /// <param name="nobreaks">A <see cref="List{T}"/> for the position of nobreaks.</param>
    public void WriteAsLineHeight(
        NetworkWriter writer,
        ParameterHandler paramHandler,
        List<NobreakInfo> nobreaks)
    {
        if (this.curves.Count != 0) // uncommon path
        {
            for (int i = 0; i < this.curves.Count; i++)
            {
                ref AnimatableFloat animatableFloat = ref this.curves[i];

                ////animatableFloat.Multiplier /= Constants.EmSize;

                int id = paramHandler.AddAnimatableFloat(in animatableFloat);

                writer.WriteStringNoSize("<line-height=");

                if (animatableFloat.AbsoluteValue)
                {
                    if (float.IsNegative(animatableFloat.Multiplier))
                    {
                        writer.WriteUtf8Char('-');
                    }
                    else
                    {
                        // the negative sign is only considered if it is the first char
                        writer.WriteUtf8Char('A');
                    }
                }

                writer.WriteFormatItemNoBreak(id, nobreaks);
                writer.WriteStringNoSize(">\n");
            }
        }

        // uncommon path
        if (this.value >= Constants.MaxValueSize)
        {
            float value = this.value;
            do
            {
                writer.WriteStringNoSize("<line-height=");
                writer.WriteFloatAsString(UnityEngine.Mathf.Min(value, Constants.MaxValueSize));
                writer.WriteStringNoSize(">\n<line-height=0>");
            }
            while ((value -= Constants.MaxValueSize) > 0);
        }
        else
        {
            writer.WriteStringNoSize("<line-height=");
            writer.WriteFloatAsString(this.value);
            writer.WriteStringNoSize(">\n<line-height=0>");
        }
    }
}