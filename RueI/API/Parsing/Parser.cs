namespace RueI.API.Parsing;

extern alias mscorlib;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

using RueI.API.Elements;
using RueI.API.Elements.Parameters;
using RueI.API.Parsing.Enums;
using RueI.API.Parsing.Modifications;
using RueI.API.Parsing.Structs;
using RueI.Utils;
using RueI.Utils.Collections;
using RueI.Utils.Enums;

using static RueI.API.Parsing.Modifications.Modification;

/// <summary>
/// Parses <see cref="Element"/>s into <see cref="ParsedData"/>.
/// </summary>
internal static class Parser
{
    // TODO: support tangents and shit by manipulating size
    private const int AdjustedTagLength = Constants.MaxTagLength - 2; // so we don't count < and =

    private static readonly mscorlib.System.Collections.Generic.Stack<AnimatableFloat> SizeStack = new();
    private static readonly CumulativeFloat Offset = new();

    private static readonly char[] CharBuffer = new char[Constants.MaxTagLength];
    private static readonly List<ContentParameter> CurrentParameters = new();
    private static readonly List<Modification> Modifications = new();

    private static readonly Trie<RichTextTag> TagTrie = new(new[]
    {
        ("noparse", RichTextTag.Noparse),
        ("line-height", RichTextTag.LineHeight),
        ("voffset", RichTextTag.VOffset),
        ("size", RichTextTag.Size),
        ("/noparse", RichTextTag.Noparse),
        ("/size", RichTextTag.Size),
        ("/voffset", RichTextTag.VOffset),
        ("/line-height", RichTextTag.CloseLineHeight),
    });

    private static readonly Trie<char> ReplacementTrie = new(new[]
    {
        ("br", '\n'),
        ("cr", '\r'),
        ("nbsp", '\u00a0'), // non breaking space
        ("zwsp", '\u200b'),
        ("zwj", '\u00AD'), // zwj
        ("shy", '\u00AD'),
    });

    private static string text = null!;

    private static bool noparse;
    private static bool noparseParsesEscapeSeq;
    private static bool noparseParsesFormat;

    private static int position = 0;

    private static AnimatableFloat lineHeight = AnimatableFloat.Invalid;

    // TODO: make docs better

    /// <summary>
    /// Parses an <see cref="Element"/> with the given text.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="element">The <see cref="Element"/> to use for settings.</param>
    /// <returns>A <see cref="ParsedData"/> representing the parsed version of the element.</returns>
    [SkipLocalsInit]
    public static ParsedData Parse(string text, Element element)
    {
        Parser.text = text;

        // TODO: support infinite
        CurrentParameters.EnsureCapacity(element.ParameterList.Count);

        foreach (ContentParameter parameter in element.ParameterList)
        {
            CurrentParameters.Add(parameter);
        }

        while (TryGetNext(out char ch))
        {
            HandleChar(ch);
        }

        // reset afterwards to avoid an unnecessary reset when first calling Parse
        Reset();

        return new ParsedData(text, Offset, Modifications);
    }

    private static void Reset()
    {
        SizeStack.Clear();
        Offset.Clear();

        position = 0;

        lineHeight = AnimatableFloat.Invalid;

        CurrentParameters.Clear();
        Modifications.Clear();
    }

    private static void HandleChar(char ch)
    {
        switch (ch)
        {
            case '<':
                TryParseTag();
                break;
            case '\n':
                AddLinebreak();
                break;
        }
    }

    private static bool TryParseTag()
    {
        const int MaxTagNameLength = 15; // stop trying to parse as a tag if the name is too long (</line-height> is 14 characters)

        int count = 0;
        int max = position + MaxTagNameLength;

        //// int start = ++position;

        Trie<RichTextTag>.RadixNode? node = TagTrie.Root;

        while (TryGetNext(out char ch) && position < max)
        {
            RichTextTag tag = node.Value;

            if (tag != default)
            {
                // quick check to see if tag takes in a parameter
                if (tag >= RichTextTag.Noparse)
                {
                    if (noparse && tag != RichTextTag.CloseNoparse)
                    {
                        break;
                    }

                    switch (ch)
                    {
                        case ' ': // space works identical to = for tags that don't take a parameter
                        case '=':
                            if (tag == RichTextTag.Noparse)
                            {
                                noparse = true;
                            }

                            while (TryGetNext(out ch))
                            {
                                if (++count > Constants.MaxTagLength)
                                {
                                    BreakTag();

                                    break;
                                }
                                else if (ch == '>')
                                {
                                    goto case '>';
                                }
                                else if (text[position] == '<' || TryParseFormatItem(out int _))
                                {
                                    BreakTag();

                                    break;
                                }
                            }

                            if (tag == RichTextTag.Noparse)
                            {
                                noparse = false;
                            }

                            return false;
                        case '>':
                            switch (tag)
                            {
                                case RichTextTag.CloseSize:
                                    SizeStack.TryPop(out _);
                                    break;
                                case RichTextTag.CloseVOffset:
                                case RichTextTag.CloseLineHeight:
                                    lineHeight = AnimatableFloat.Invalid;
                                    break;
                                case RichTextTag.Default:
                                    noparse = true;
                                    break;
                                case RichTextTag.CloseNoparse:
                                    noparse = false;
                                    break;
                            }

                            return true;
                    }
                }
                else
                {
                    if (ch != '=')
                    {
                        HandleChar(ch);

                        return false;
                    }

                    int pos = position - 1;

                    if (TryParseMeasurements(count, out MeasurementInfo info))
                    {
                        switch (tag)
                        {
                            case RichTextTag.LineHeight:
                                lineHeight = info.ToAnimatableFloat(Constants.DefaultLineHeight);
                                break;
                            case RichTextTag.Size:
                                float add = info.AddType != MeasurementInfo.AdditionType.Default && info.Unit == MeasurementUnit.Pixels ? Constants.EmSize : 0;
                                SizeStack.Push(info.ToAnimatableFloat(Constants.EmSize, add));

                                break;
                            case RichTextTag.VOffset:
                                // TODO: support voffset
                                return false;
                        }
                    }

                    position = pos;

                    return false;
                }
            }

            if ((node = node[TagHelpers.ToLowercaseFast(ch)]) == null)
            {
                HandleChar(ch);

                return true;
            }

            position++;
            count++;
        }

        return false;
    }

    private static bool TryParseFormatItem(out int num)
    {
        char cur = text[position];

        if (noparse && !noparseParsesFormat)
        {
            switch (cur)
            {
                case '{':
                    Modifications.Add(new Modification(ModificationType.AdditionalForwardBracket, position));
                    Modifications.Add(new Modification(ModificationType.DoNotBreakFor, position, 2));
                    num = -1;

                    return true;
                case '}':
                    Modifications.Add(new Modification(ModificationType.AdditionalBackwardsBracket, position));
                    Modifications.Add(new Modification(ModificationType.DoNotBreakFor, position, 2));
                    num = -1;

                    return true;
            }

            Unsafe.SkipInit(out num);
            return false;
        }

        // TODO: handle singular { or }
        if (cur == '{')
        {
            const int MaxNumLength = 5;

            if (MoveNext())
            {
                if (text[position] == '{')
                {
                    Modifications.Add(new Modification(ModificationType.DoNotBreakFor, position - 1, 2));

                    num = -1;

                    return true;
                }
            }
            else
            {
                num = 0;

                return false;
            }

            int start = position;
            int stopAt = Math.Min(text.Length, position + MaxNumLength);

            while (position < stopAt)
            {
                if (text[position] == '}')
                {
                    int length = position - start;

                    if (int.TryParse(text.AsSpan(start, length), out int result) && result >= 0)
                    {
                        if (result >= CurrentParameters.Count)
                        {
                            Modifications.Add(new Modification(ModificationType.InvalidFormatItem, start, length));
                            Modifications.Add(new Modification(ModificationType.DoNotBreakFor, start, length));

                            // doesn't really matter what we do here
                            Unsafe.SkipInit(out num);
                            return false;
                        }

                        Modifications.Add(new Modification(ModificationType.FormatItem, start));
                        Modifications.Add(new Modification(ModificationType.DoNotBreakFor, start, length));
                        num = result;

                        return true;
                    }

                    Unsafe.SkipInit(out num);
                    return false;
                }

                position++;
            }
        }
        else if (cur == '}' && MoveNext() && text[position] == '}')
        {
            Modifications.Add(new Modification(ModificationType.DoNotBreakFor, position - 1, 2));
            num = -1;

            return true;
        }

        Unsafe.SkipInit(out num);
        return false;
    }

    private static bool TryParseMeasurements(int count, out MeasurementInfo info)
    {
        const MeasurementUnit NotSet = (MeasurementUnit)(-1);
        MeasurementUnit unit = NotSet;

        MeasurementInfo.AdditionType type;

        int numDigits = 0;
        int decimalPoint = -1;

        bool comma = false;

        int paramId = -1;

        char ch;

        bool MoveNextMeasurement()
        {
            if (TryGetNext(out ch) && ++count < Constants.MaxTagLength)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        if (MoveNextMeasurement())
        {
            type = ch switch
            {
                '-' => MeasurementInfo.AdditionType.SubtractiveOrNegative,
                '+' => MeasurementInfo.AdditionType.Additive,
                _ => MeasurementInfo.AdditionType.Default,
            };
        }
        else
        {
            // TODO: fix
            info = default;

            return true;
        }

        do
        {
            switch (ch)
            {
                case '.' when decimalPoint == -1:
                    decimalPoint = numDigits;
                    break;
                case '<':
                    goto EndLoop;
                case ',':
                    comma = true;
                    break;
                case '>':
                    float value = 0;

                    // TODO: check measurement > 32768
                    if (paramId == -1)
                    {
                        value = decimalPoint == -1
                            ? TagHelpers.FromIntegerAndDecimal(CharBuffer[..numDigits], ReadOnlySpan<char>.Empty)
                            : TagHelpers.FromIntegerAndDecimal(CharBuffer[..decimalPoint], CharBuffer[decimalPoint..numDigits]);
                    }

                    info = new()
                    {
                        Unit = unit == NotSet ? MeasurementUnit.Pixels : unit,
                        AddType = type,
                        ParamId = paramId,
                        Value = value,
                    };

                    return true;
                default:
                    if (unit == NotSet)
                    {
                        switch (ch)
                        {
                            case 'e':
                                unit = MeasurementUnit.Pixels;
                                break;
                            case '%':
                                unit = MeasurementUnit.Percentage;
                                break;
                            case ' ':
                            case 'p':
                                unit = MeasurementUnit.Pixels;
                                break;
                        }
                    }
                    else if (TryParseFormatItem(out int num))
                    {
                        if (num == -1) // escape sequence, e.g. {{ -> add additional
                        {
                            count++;
                        }
                        else
                        {
                            // TODO: remove this huge ass check
                            if (paramId != -1 || unit != NotSet || decimalPoint != -1 || numDigits != -1 || comma || CurrentParameters[paramId] is not AnimatedParameter)
                            {
                                goto EndLoop;
                            }

                            paramId = num;
                            decimalPoint = 0; // so we don't also have to check paramId
                        }
                    }
                    else if (TagHelpers.IsDigitFast(ch) && paramId == -1 && !comma)
                    {
                        CharBuffer[numDigits++] = ch;
                    }

                    break;
            }
        }
        while (MoveNextMeasurement());

    EndLoop:

        // TODO: break tag
        BreakTag();

        info = default;

        return false;
    }

    private static bool MoveNext() => ++position < text.Length;

    private static bool TryGetNext(out char ch)
    {
        if (!MoveNext())
        {
            Unsafe.SkipInit(out ch);

            return false;
        }

        char cur = text[position];

        // we handle <br> tags and other similar tags here since they have special behavior (they act more like escape sequences)
        if (cur == '<' && !noparse)
        {
            Trie<char>.RadixNode? node = ReplacementTrie.Root;
            int replaceTagPos = position;

            while (replaceTagPos < text.Length)
            {
                char replacementCh = text[replaceTagPos];

                if (replacementCh is '=' or ' ' or '>')
                {
                    if (node.Value != default)
                    {
                        position = replaceTagPos;
                        ch = node.Value;

                        return true;
                    }

                    break;
                }

                if ((node = node[replacementCh]) == null)
                {
                    break;
                }

                replaceTagPos++;
            }

            ch = '<';
            return true;
        }

        if (cur != '\\')
        {
            ch = cur;

            return true;
        }

        if (noparse && !noparseParsesEscapeSeq)
        {
            // in noparse and noparse doesn't add escape seq, so add an additional backslash
            Modifications.Add(new(ModificationType.AdditionalBackslash, position));

            ch = '\\';

            return true;
        }

        int newPos = position + 1;

        if (newPos < text.Length)
        {
            switch (text[newPos])
            {
                case '\\': // escaped \, so just advance to next \
                    position = newPos;
                    ch = '\\';
                    return true;
                case 'v': // vertical tab
                case 'n': // \n
                    position = newPos;
                    ch = '\n';
                    return true;
                case 't':
                    position = newPos;
                    ch = '\t';
                    return true;
                case 'r':
                    position = newPos;
                    ch = '\r';
                    return true;
                case 'U': // TODO: handle \U
                    Modifications.Add(new(ModificationType.AdditionalBackslash, position));
                    ch = '\\';
                    return true;
                case 'u':
                    if (++newPos + 4 < text.Length)
                    {
                        if (int.TryParse(text.AsSpan(newPos, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value))
                        {
                            // TODO: check
                            position = newPos + 4;
                            ch = (char)value;

                            return true;
                        }
                    }

                    break;
            }
        }

        ch = '\\';

        return true;
    }

    private static void BreakTag()
    {
        ModificationType type = noparse ? ModificationType.InsertNoparse : ModificationType.InsertCloseNoparse;
        Modifications.Add(new Modification(type, position));
    }

    private static void AddLinebreak()
    {
        if (!lineHeight.IsInvalid)
        {
            Offset.Add(in lineHeight);
        }
        else if (SizeStack.TryPeek(out AnimatableFloat value))
        {
            Offset.Add(in value);
        }
        else
        {
            Offset.Add(Constants.DefaultLineHeight);
        }
    }

    private ref struct TagInfo
    {
        public ReadOnlySpan<char> TagName;

        public MeasurementInfo? Info;
    }

    private struct MeasurementInfo
    {
        public MeasurementUnit Unit;

        public AdditionType AddType;

        public float Value;

        public int ParamId;

        internal enum AdditionType
        {
            Default,
            Additive,
            SubtractiveOrNegative,
        }

        public readonly AnimatableFloat ToAnimatableFloat(float pointSize, float add = 0)
        {
            float multi = this.AddType == AdditionType.SubtractiveOrNegative ? -1f : 1f;

            if (this.ParamId == -1)
            {
                return this.Unit switch
                {
                    MeasurementUnit.Percentage => new(((this.Value / 100 * pointSize) + add) * multi),
                    MeasurementUnit.Ems => new(((this.Value * Constants.EmSize) + add) * multi),
                    _ => new((this.Value + add) * multi),
                };
            }
            else
            {
                bool abs = this.AddType != AdditionType.Default;
                return this.Unit switch
                {
                    MeasurementUnit.Percentage => new(this.ParamId, add, pointSize / 100 * multi, abs),
                    MeasurementUnit.Ems => new(this.ParamId, add, Constants.EmSize * multi, abs),
                    _ => new(this.ParamId, add, multi, abs),
                };
            }
        }
    }
}