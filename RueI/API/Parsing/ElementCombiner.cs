namespace RueI.API.Parsing;

using System;
using System.Collections.Generic;
using System.Text;

using global::Utils.Networking;
using Hints;

using Mirror;

using RueI.API.Elements;
using RueI.API.Elements.Parameters;
using RueI.API.Parsing.Modifications;
using RueI.API.Parsing.Structs;
using RueI.Utils.Extensions;
using Unity.Collections.LowLevel.Unsafe;

/// <summary>
/// Combines multiple <see cref="Elements.Element"/>s into a single <see cref="TextHint"/>.
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
    private static readonly List<NoBreakInfo> Nobreaks = new();
    ////private static readonly List<List<Keyframe>> Keyframes = new();
    private static readonly List<ContentParameter> CurrentParameters = new();

    internal static void Combine(ReferenceHub hub, IEnumerable<Element> elements)
    {
        using NetworkWriterPooled totalWriter = NetworkWriterPool.Get();

        int paramIndex = 0;

        totalWriter.WriteUShort(NetworkMessageId<HintMessage>.Id);
        totalWriter.WriteByte(TextHintID);
        totalWriter.WriteInt(-1); // hint effects

        int paramCountPos = totalWriter.Position;

        totalWriter.WriteInt(0);

        ////writer.WriteStringNoLength("<line-height=");
        int paddedPos = totalWriter.Position;

        ////writer.Write(PaddingBytes);

        using NetworkWriterPooled contentWriter = NetworkWriterPool.Get();

        CumulativeFloat lastPosition = CumulativeFloat.Invalid;
        CumulativeFloat lastOffset = default;

        bool isFirst = true;

        foreach (Element element in elements)
        {
            int elemParamIndex = paramIndex;
            int paramCount = element.ParameterList.Count;

            // TODO: reverse (parameter list is read back to front while we want to read it front to back)
            CurrentParameters.Clear();
            CurrentParameters.EnsureCapacity(paramCount);
            foreach (ContentParameter parameter in element.ParameterList)
            {
                CurrentParameters.Add(parameter);

                totalWriter.WriteByte((byte)parameter.HintParameterType);
                parameter.Write(totalWriter);
            }

            paramIndex += element.ParameterList.Count;

            ParsedData data = element.GetParsedData(hub);
            string text = data.ParsedString!; // TODO: remove null check

            CumulativeFloat position;

            if (element.AnimatedPosition != null)
            {
                // TODO: add offset, make func pos
                AddAnimatedParameter(totalWriter, element.AnimatedPosition.Value);

                AnimatedParameter parameter = new(element.AnimatedPosition.Value);

                position = new();
                position.Add(new AnimatableFloat(parameter));
            }
            else
            {
                position = new(element.Position);
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

                SubOffset.WriteAsLineHeight(contentWriter, totalWriter, CurrentParameters, Nobreaks, ref paramIndex);

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

                switch (current.Type)
                {
                    // TODO: rewrite as a class (what the fuck was i thinking?)
                    case Modification.ModificationType.AdditionalBackslash:
                        contentWriter.WriteChar('\\');
                        break;
                    case Modification.ModificationType.AdditionalForwardBracket:
                        contentWriter.WriteChar('{');
                        break;
                    case Modification.ModificationType.AdditionalBackwardsBracket:
                        contentWriter.WriteChar('}');
                        break;
                    case Modification.ModificationType.InsertNoparse:
                        contentWriter.WriteStringNoSize("<noparse>");
                        break;
                    case Modification.ModificationType.InsertCloseNoparse:
                        contentWriter.WriteStringNoSize("</noparse>");
                        break;
                    case Modification.ModificationType.FormatItem:
                        contentWriter.WriteFormatItemNoBreak(elemParamIndex + current.AdditionalInfo, Nobreaks);
                        break;
                    case Modification.ModificationType.SkipNext:
                        buffer = buffer[current.AdditionalInfo..];
                        break;
                    case Modification.ModificationType.DoNotBreakFor:
                        int writerPosition = contentWriter.Position;

                        Nobreaks.Add(new NoBreakInfo()
                        {
                            Start = writerPosition,
                            End = writerPosition + current.AdditionalInfo,
                        });

                        break;
                }
            }

            contentWriter.WriteChars(buffer);

            CumulativeOffset.Add(data.Offset);
            lastPosition = position;
            lastOffset = data.Offset;
        } // foreach (Element element in elements)

        contentWriter.WriteStringNoSize("<size=0></mspace></cspace><line-height=0>."); // trigger trailing newlines

        using NetworkWriterPooled offsetWriter = NetworkWriterPool.Get();

        CumulativeOffset.WriteAsLineHeight(offsetWriter, totalWriter, CurrentParameters, Nobreaks, ref paramIndex);

        // TODO: support offsetwriter for this
        int length = contentWriter.Position + offsetWriter.Position;
        if (contentWriter.Position + offsetWriter.Position > MaxStringLength)
        {
            using NetworkWriterPooled contentWithParameterWriter = NetworkWriterPool.Get();

            contentWithParameterWriter.WriteNetworkWriter(offsetWriter, false);

            CumulativeOffset.WriteAsLineHeight(contentWithParameterWriter, totalWriter, CurrentParameters, Nobreaks, ref paramIndex);

            int pos = 0;
            int i = 0;

            do
            {
                // TODO: rewrite this to be slightly less dumb
                if (i < Nobreaks.Count)
                {
                    NoBreakInfo nobreak = Nobreaks[i];
                    int start = nobreak.Start;
                    int maxForNobreak = start - pos;

                    // if false, the start of the nobreak is greater than what we can write in one pass
                    if (maxForNobreak < MaxStringLength)
                    {
                        AddStringParameter(totalWriter, contentWithParameterWriter, contentWriter.buffer.AsSpan(pos, maxForNobreak), ref paramIndex);
                        contentWithParameterWriter.WriteBytes(contentWriter.buffer.AsSpan(start, nobreak.End), false);

                        pos = nobreak.End;

                        continue;
                    }
                }

                int cap = Math.Min(length - pos, MaxStringLength);

                AddStringParameter(totalWriter, contentWithParameterWriter, contentWriter.buffer.AsSpan(pos, cap), ref paramIndex);

                pos += cap;
            }
            while (length > pos); // write while there is still stuff left

            totalWriter.WriteNetworkWriter(contentWithParameterWriter, true);
        }
        else
        {
            // TODO: make sure we write cumulativeoffset at the correct spot / length
            totalWriter.WriteUShort((ushort)(contentWriter.Position + offsetWriter.Position)); // write padding

            totalWriter.WriteNetworkWriter(offsetWriter, false);
            totalWriter.WriteNetworkWriter(contentWriter, false);
        }

        int oldPos = totalWriter.Position;

        totalWriter.Position = paramCountPos;
        totalWriter.WriteInt(paramIndex);

        totalWriter.Position = oldPos;

        hub.connectionToClient.Send(totalWriter.ToArraySegment());
    }

    private static void CalculateOffset(CumulativeFloat hintOnePos, CumulativeFloat hintOneTotalLines, CumulativeFloat hintTwoPos)
    {
        SubOffset.Add(hintOneTotalLines);
        hintOneTotalLines.Multiply(2);
        SubOffset.Add(hintOnePos);
        SubOffset.Subtract(hintTwoPos);
        SubOffset.Divide(2);
    }

    private static void AddAnimatedParameter(NetworkWriter writer, AnimatedValue parameter)
    {
        writer.WriteByte(AnimatedParameterID);
        writer.WriteDouble(0); // offset
        writer.WriteString(null); // format
        writer.WriteBool(false); // integral
        parameter.Write(writer);
    }

    private static void AddStringParameter(NetworkWriter paramWriter, NetworkWriter newContentWriter, ReadOnlySpan<byte> bytes, ref int parameterIndex)
    {
        paramWriter.WriteByte(StringParameterID);
        paramWriter.WriteBytes(bytes, writeSize: true);
        newContentWriter.WriteFormatItem(parameterIndex++);
    }

    private static ReadOnlySpan<T> SplitUntil<T>(scoped ref ReadOnlySpan<T> span, int position)
    {
        ReadOnlySpan<T> until = span[..position];

        span = span[position..];

        return until;
    }
}