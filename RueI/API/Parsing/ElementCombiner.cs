namespace RueI.API.Parsing;

using System;
using System.Collections.Generic;

using global::Utils.Networking;
using Hints;

using Mirror;

using RueI.API.Elements;
using RueI.API.Elements.Parameters;
using RueI.API.Parsing.Modifications;
using RueI.API.Parsing.Structs;
using RueI.Utils;
using RueI.Utils.Extensions;

/// <summary>
/// Combines multiple <see cref="Elements.Element"/>s into a single <see cref="TextHint"/>
/// and sends it to a <see cref="ReferenceHub"/>.
/// </summary>
internal static class ElementCombiner
{
    /// <summary>
    /// Gets the ID of an animated parameter.
    /// </summary>
    internal const byte AnimatedParameterID = (byte)HintParameterReaderWriter.HintParameterType.AnimationCurve;

    private const ushort MaxStringLength = ushort.MaxValue - 2;
    private const byte StringParameterID = (byte)HintParameterReaderWriter.HintParameterType.Text;

    private const int TextHintID = 1;

    private static readonly CumulativeFloat CumulativeOffset = new();
    private static readonly CumulativeFloat SubOffset = new();
    private static readonly List<NobreakInfo> Nobreaks = new();
    ////private static readonly List<List<Keyframe>> Keyframes = new();
    private static readonly List<ContentParameter> CurrentParameters = new();

    private static readonly ParameterHandler ParameterHandler = new(Nobreaks);

    /// <summary>
    /// Combines an <see cref="Element"/> <see cref="IEnumerable{T}"/> and sends it to
    /// a <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">The <see cref="ReferenceHub"/> to combine the <see cref="Element"/>s for and
    /// to send the <see cref="TextHint"/> to.</param>
    /// <param name="elements">The elements to combine.</param>
    internal static void Combine(ReferenceHub hub, IEnumerable<Element> elements)
    {
        using NetworkWriterPooled totalWriter = NetworkWriterPool.Get();

        totalWriter.WriteUShort(NetworkMessageId<HintMessage>.Id);
        totalWriter.WriteByte(TextHintID);
        totalWriter.WriteInt(-1); // hint effects

        ParameterHandler.Setup(totalWriter);

        using NetworkWriterPooled contentWriter = NetworkWriterPool.Get();

        // prevent any overflows from line breaking
        contentWriter.WriteStringNoSize("<line-height=0>");

        CumulativeFloat lastPosition = CumulativeFloat.Invalid;
        CumulativeFloat lastOffset = default;

        bool isFirst = true;

        CombinerContext context = new(contentWriter, Nobreaks, ParameterHandler);

        foreach (Element element in elements)
        {
            if (element.Parameters != null)
            {
                ParameterHandler.SetElementParameters(element.Parameters);
            }

            ParsedData data = element.GetParsedData(hub);
            string text = data.ParsedString!; // TODO: remove null check

            CumulativeFloat position;

            if (element.AnimatedPosition != null)
            {
                // TODO: add offset, make scaled pos
                AnimatedParameter parameter = new(element.AnimatedPosition.Value);

                position = new();
                position.Add(PositionUtils.ScaledToBaselineParameter(parameter));
            }
            else
            {
                position = new(PositionUtils.ScaledToBaseline(element.Position));
            }

            switch (element.VerticalAlign)
            {
                case Elements.Enums.VerticalAlign.Up:
                    position.Subtract(data.Offset);
                    position.Subtract(data.Offset);
                    break;
                case Elements.Enums.VerticalAlign.Center:
                    position.Subtract(data.Offset);
                    break;
                default:
                    break;
            }

            if (isFirst)
            {
                isFirst = false;

                CumulativeOffset.Add(position);
            }
            else
            {
                // TODO: check
                CalculateOffset(lastPosition, lastOffset, position);

                SubOffset.WriteAsLineHeight(contentWriter, ParameterHandler, Nobreaks);

                CumulativeOffset.Add(SubOffset);
            }

            // begin writing
            ReadOnlySpan<char> buffer = text.AsSpan();
            for (int i = 0; i < data.Modifications.Count; i++)
            {
                Modification current = data.Modifications[i];

                int pos = current.Position;

                // TODO: make faster perhaps
                contentWriter.WriteChars(SplitUntil(ref buffer, pos));

                current.Apply(context, ref buffer);
            }

            // write the remaining characters
            contentWriter.WriteChars(buffer);

            CumulativeOffset.Add(data.Offset);
            lastPosition = position;
            lastOffset = data.Offset;
        } // foreach (Element element in elements)

        contentWriter.WriteStringNoSize("<size=0></mspace></cspace><line-height=0>."); // trigger trailing newlines

        using NetworkWriterPooled offsetWriter = NetworkWriterPool.Get();

        CumulativeOffset.WriteAsLineHeight(offsetWriter, ParameterHandler, Nobreaks);

        // TODO: support offsetwriter for this
        int length = contentWriter.Position + offsetWriter.Position;
        if (contentWriter.Position + offsetWriter.Position > MaxStringLength)
        {
            using NetworkWriterPooled contentWithParameterWriter = NetworkWriterPool.Get();

            contentWithParameterWriter.WriteNetworkWriter(offsetWriter, false);

            CumulativeOffset.WriteAsLineHeight(contentWithParameterWriter, ParameterHandler, Nobreaks);

            int pos = 0;
            int i = 0;

            do
            {
                // TODO: rewrite this to be slightly less dumb
                if (i < Nobreaks.Count)
                {
                    // get how much we can write before we encounter a linebreak
                    NobreakInfo nobreak = Nobreaks[i];
                    int start = nobreak.Start;
                    int end = nobreak.End;

                    // "Hello|world|"
                    //   ^   ^     ^
                    //   1   5     11
                    //  pos start  end
                    // max = 5 - 1 = 4
                    int maxWritable = start - pos;

                    // if false, the start of the nobreak is greater than what we can write in one pass
                    // if that's the case, we just ignore it for now
                    // otherwise, just write it as usual
                    if (maxWritable < MaxStringLength)
                    {
                        ParameterHandler.AddStringParameter(contentWriter.buffer.AsSpan(pos, maxWritable));

                        // write the text that we can't break during
                        contentWithParameterWriter.WriteBytes(contentWriter.buffer.AsSpan(start, end - start), false);

                        i++;
                        pos = end;

                        continue;
                    }
                }

                int cap = Math.Min(length - pos, MaxStringLength);

                ParameterHandler.AddStringParameter(contentWriter.buffer.AsSpan(pos, cap));

                pos += cap;
            }
            while (length > pos); // write while there is still stuff left

            totalWriter.WriteNetworkWriter(contentWithParameterWriter, true);
        }
        else
        {
            // TODO: make sure we write cumulativeoffset at the correct spot / length
            totalWriter.WriteUShort((ushort)(contentWriter.Position + offsetWriter.Position));

            totalWriter.WriteNetworkWriter(offsetWriter, false);
            totalWriter.WriteNetworkWriter(contentWriter, false);
        }

        ParameterHandler.Finish();

        // TODO: look into optimizing this further by reducing copies even more (directly add to batch, for example)
        hub.connectionToClient.Send(totalWriter.ToArraySegment());
    }

    /// <summary>
    /// Calculates the offset for two elements and places the result in <see cref="SubOffset"/>.
    /// </summary>
    private static void CalculateOffset(CumulativeFloat hintOnePos, CumulativeFloat hintOneTotalLines, CumulativeFloat hintTwoPos)
    {
        SubOffset.Add(hintOneTotalLines);
        hintOneTotalLines.Multiply(2);
        SubOffset.Add(hintOnePos);
        SubOffset.Subtract(hintTwoPos);
        SubOffset.Divide(2);
    }

    private static ReadOnlySpan<T> SplitUntil<T>(scoped ref ReadOnlySpan<T> span, int position)
    {
        ReadOnlySpan<T> until = span[..position];

        span = span[position..];

        return until;
    }
}