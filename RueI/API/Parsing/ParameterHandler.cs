namespace RueI.API.Parsing;

using System;
using System.Collections.Generic;
using global::Utils.Networking;
using Mirror;
using NorthwoodLib.Pools;
using RueI.API.Elements.Parameters;
using RueI.API.Parsing.Structs;
using RueI.Utils.Extensions;

/// <summary>
/// Handles the parameters for <see cref="ElementCombiner"/>.
/// </summary>
internal class ParameterHandler
{
    private readonly List<ContentParameter> currentParameters = new();
    private readonly List<NoBreakInfo> noBreaks;

    private NetworkWriter writer = null!;
    private int index = 0;
    private int elementIndex = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterHandler"/> class.
    /// </summary>
    /// <param name="noBreaks">The <see cref="List{T}"/> to add nobreaks to.</param>
    internal ParameterHandler(List<NoBreakInfo> noBreaks)
    {
        this.noBreaks = noBreaks;
    }

    /// <summary>
    /// Sets up the <see cref="ParameterHandler"/> for a new batch.
    /// </summary>
    /// <param name="writer">The <see cref="NetworkWriter"/> to write the parameters to.</param>
    internal void Setup(NetworkWriter writer)
    {
        this.writer = writer;
        this.index = 0;

        // number of parameters
        writer.WriteInt(0);
    }

    internal void SetElementParameters(ParameterList list)
    {
        this.currentParameters.Clear();
        this.currentParameters.EnsureCapacity(list.Count);

        foreach (ContentParameter parameter in list)
        {
            this.currentParameters.Add(parameter);

            this.writer.WriteByte((byte)parameter.HintParameterType);
            parameter.Write(this.writer);
        }

        this.elementIndex = this.index;
        this.index += list.Count;
    }

    internal int GetMappedElementParameterId(int paramId) => this.elementIndex + paramId;

    internal int AddStringParameter(ReadOnlySpan<byte> bytes)
    {
        this.writer.WriteByte((byte)HintParameterReaderWriter.HintParameterType.Text);
        this.writer.WriteBytes(bytes, writeSize: true);

        return this.index++;
    }

    internal int AddAnimatableFloat(in AnimatableFloat value)
    {
        this.writer.WriteByte((byte)HintParameterReaderWriter.HintParameterType.Anim)
        value.Parameter!.WriteTransformed(this.writer, value.Multiplier, value.AddendOrValue);

        return this.index++;
    }
}