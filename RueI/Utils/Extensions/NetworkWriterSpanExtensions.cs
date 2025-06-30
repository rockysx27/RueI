namespace RueI.Utils.Extensions;

extern alias mscorlib;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Mirror;
using mscorlib::System.Buffers.Text;
using RueI.API.Parsing.Structs;
using UnityEngine;
using YamlDotNet.Core.Tokens;

/// <summary>
/// Provides extensions for the <see cref="NetworkWriter"/> class to write <see cref="ReadOnlySpan{T}"/>s.
/// </summary>
internal static class NetworkWriterSpanExtensions
{
    private const int MaxArraySize = 16777216; // 2 ^ 24;
    private static readonly UTF8Encoding Encoding = new(false, true);

    /// <summary>
    /// Writes a <see cref="ReadOnlySpan{T}"/> and the number of elements in it to a <see cref="NetworkWriter"/>.
    /// </summary>
    /// <typeparam name="T">The type to write. Should have a <see cref="Writer{T}"/>.</typeparam>
    /// <param name="writer">The <see cref="NetworkWriter"/> to write to.</param>
    /// <param name="span">The <see cref="Span{T}"/> to write.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when there are more
    /// than 16777216 elements in <paramref name="span"/>.</exception>
    internal static void WriteSpanAndLength<T>(this NetworkWriter writer, ReadOnlySpan<T> span)
    {
        if (span.Length > 16777216)
        {
            throw new IndexOutOfRangeException($"NetworkWriter.WriteSpan - ReadOnlySpan<{typeof(T)}> too big: {span.Length} elements. Limit: {MaxArraySize}");
        }

        writer.WriteInt(span.Length);
        for (int i = 0; i < span.Length; i++)
        {
            writer.Write(span[i]);
        }
    }

    internal static void WriteStringNoSize(this NetworkWriter writer, string str)
    {
        int maxByteCount = Encoding.GetMaxByteCount(str.Length);

        writer.EnsureLength(2 + maxByteCount);

        int bytes = Encoding.GetBytes(str, writer.PositionSpan());
        if (bytes > 65534)
        {
            throw new IndexOutOfRangeException(string.Format("NetworkWriter.WriteString - Value too long: {0} bytes. Limit: {1} bytes", bytes, 65534));
        }

        writer.Position += bytes;
    }

    /// <summary>
    /// Writes a string using a <see langword="char"/> <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="writer">The <see cref="NetworkWriter"/> to write to.</param>
    /// <param name="span">The <see cref="Span{T}"/> to write.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="span"/> is larger
    /// than or equal to <see cref="ushort.MaxValue"/>.</exception>
    internal static void WriteCharsAndLength(this NetworkWriter writer, ReadOnlySpan<char> span)
    {
        const int USHORT_SIZE = sizeof(ushort);

        int maxByteCount = Encoding.GetMaxByteCount(span.Length);

        writer.EnsureLength(maxByteCount + USHORT_SIZE);

        int count = Encoding.GetBytes(span, writer.PositionSpan(USHORT_SIZE));

        if (count > NetworkWriter.MaxStringLength)
        {
            throw new IndexOutOfRangeException($"NetworkWriter.WriteSpan - Value too long: {count} bytes. Limit: {ushort.MaxValue} bytes");
        }

        writer.WriteUShort(checked((ushort)(count + 1))); // don't ask why this is checked, this is just how it does it's done in mirror
        writer.Position += count;
    }

    /// <summary>
    /// Writes a string using a <see langword="char"/> <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="writer">The <see cref="NetworkWriter"/> to write to.</param>
    /// <param name="span">The <see cref="Span{T}"/> to write.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="span"/> is larger
    /// than or equal to <see cref="ushort.MaxValue"/>.</exception>
    internal static void WriteChars(this NetworkWriter writer, ReadOnlySpan<char> span)
    {
        const int USHORT_SIZE = sizeof(ushort);

        int maxByteCount = Encoding.GetMaxByteCount(span.Length);

        writer.EnsureLength(maxByteCount + USHORT_SIZE);

        int count = Encoding.GetBytes(span, writer.buffer.AsSpan(writer.Position));

        writer.Position += count;
    }

    internal static void WriteFormatItemNoBreak(this NetworkWriter writer, int i, List<NoBreakInfo> nobreaks)
    {
        int start = writer.Position;

        writer.WriteFormatItem(i);

        nobreaks.Add(new()
        {
            Start = start,
            End = writer.Position,
        });
    }

    internal static void WriteFormatItem(this NetworkWriter writer, int i)
    {
        writer.WriteUtf8Char('{');
        writer.WriteIntAsString(i);
        writer.WriteUtf8Char('}');
    }

    /// <summary>
    /// Writes a positive <see langword="int"/> as a string, without allocating.
    /// </summary>
    /// <param name="writer">The <see cref="NetworkWriter"/> to write to.</param>
    /// <param name="value">The <see langword="int"/> to write.</param>
    internal static void WriteIntAsString(this NetworkWriter writer, int value)
    {
        const int MaxIntegerDigits = 10;

        writer.EnsureLength(MaxIntegerDigits);

        Span<byte> span = writer.PositionSpan();

        do
        {
            span[writer.Position++] = unchecked((byte)((value % 10) + '0'));

            value /= 10;
        }
        while (value >= 0);

        // reverse since we added the values in reverse order
        span[..writer.Position].Reverse();
    }

    /// <summary>
    /// Writes a <see langword="float"/> as a string, without allocating.
    /// </summary>
    /// <param name="writer">The <see cref="NetworkWriter"/> to write to.</param>
    /// <param name="value">The <see langword="int"/> to write.</param>
    internal static void WriteFloatAsString(this NetworkWriter writer, float value)
    {
        const int Length = 32;

        ////writer.EnsureLength(Length);

        int bytes;
        StandardFormat format = new('F', 5);

        while (!Utf8Formatter.TryFormat(value, writer.PositionSpan(), out bytes, format))
        {
            writer.EnsureLength(Length);
        }

        writer.Position += bytes;
    }

    /// <summary>
    /// Writes a <see langword="char"/> as UTF8.
    /// </summary>
    /// <param name="writer">The <see cref="NetworkWriter"/> to write to.</param>
    /// <param name="ch">The <see langword="char"/> to write.</param>
    internal static void WriteUtf8Char(this NetworkWriter writer, char ch) => writer.WriteByte((byte)ch);

    internal static void WriteBytes(this NetworkWriter writer, ReadOnlySpan<byte> bytes)
    {
        writer.EnsureLength(bytes.Length);

        bytes.CopyTo(writer.PositionSpan());

        writer.Position += bytes.Length;
    }

    internal static void WriteBytesAndSize(this NetworkWriter writer, ReadOnlySpan<byte> bytes)
    {
        writer.EnsureLength(bytes.Length + sizeof(ushort));

        writer.WriteUShort((ushort)bytes.Length);

        bytes.CopyTo(writer.PositionSpan());

        writer.Position += bytes.Length;
    }

    internal static void WriteNetworkWriterAndLength(this NetworkWriter writer, NetworkWriter otherWriter)
    {
        int otherWriterPos = otherWriter.Position;

        writer.EnsureLength(otherWriterPos);

        writer.WriteUShort((ushort)otherWriterPos);

        otherWriter.buffer.AsSpan(0, otherWriterPos).CopyTo(writer.PositionSpan());

        /* unsafe
        {
            Buffer.MemoryCopy(Unsafe.AsPointer(ref writer.buffer[thisWriterPos]), Unsafe.AsPointer(ref writer.buffer[thisWriterPos]), long.MaxValue, thisWriterPos);
        } */
    }

    internal static void CopyToWriter(this NetworkWriter writer, NetworkWriter otherWriter)
    {
        int thisWriterPos = writer.Position;

        otherWriter.EnsureLength(thisWriterPos);

        writer.buffer.AsSpan(0, thisWriterPos).CopyTo(otherWriter.PositionSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Span<byte> PositionSpan(this NetworkWriter writer, int offset = 0) => writer.buffer.AsSpan(writer.Position + offset);

    private static int GetDigitsFast(int i)
    {
        return i switch
        {
            < 10 => 1,
            < 100 => 2,
            < 1_000 => 3,
            < 10_000 => 4,
            < 100_000 => 5,
            < 1_000_000 => 6,
            < 10_000_000 => 7,
            < 100_000_000 => 8,
            < 1_000_000_000 => 9,
            _ => 10,
        };
    }
}