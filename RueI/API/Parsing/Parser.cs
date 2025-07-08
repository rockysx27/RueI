namespace RueI.API.Parsing;

extern alias mscorlib;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using RueI.API.Elements;
using RueI.API.Elements.Parameters;
using RueI.API.Parsing.Enums;
using RueI.API.Parsing.Modifications;
using RueI.API.Parsing.Structs;
using RueI.Utils;
using RueI.Utils.Collections;
using RueI.Utils.Enums;
using RueI.Utils.Extensions;

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
    private static readonly IReadOnlyList<ContentParameter> CurrentParameters = null!; // safe, since we only access inside Parse (which sets this)
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
        ("zwj", '\u00AD'),
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

        noparseParsesEscapeSeq = element.NoparseSettings.HasFlagFast(Elements.Enums.NoparseSettings.ParsesEscapeSequences);
        noparseParsesFormat = element.NoparseSettings.HasFlagFast(Elements.Enums.NoparseSettings.ParsesFormatItems);

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

        Modifications.Clear();
    }

    private static void HandleChar(char ch)
    {
        if (TryParseFormatItem(out int _))
        {
            return;
        }

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
        // keep track of count to ensure that our total size is less than Constants.MaxTagSize
        int startPos = position;

        int Count() => position - startPos + 1; // + 1 to include >

        //// int start = ++position;

        Trie<RichTextTag>.RadixNode? node = TagTrie.Root;

        while (TryGetNext(out char ch))
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
                            if (tag == RichTextTag.Noparse) // if we have something like <noparse=...>, inside ... noparse is technically on
                            {
                                noparse = true;
                            }

                            while (TryGetNext(out ch))
                            {
                                if (Count() > Constants.MaxTagLength)
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
                                default:
                                    return true;
                            }

                            return true;
                    }
                }
                else
                {
                    if (ch != '=')
                    {
                        // we've landed on a valid tag that takes a parameter,
                        // but the next char isn't =
                        // handle it since it could be something like a linebreak
                        HandleChar(ch);

                        return false;
                    }

                    int oldPos = position;

                    if (TryParseMeasurements(Count(), out MeasurementInfo info))
                    {
                        AnimatableFloat value = default;

                        switch (tag)
                        {
                            case RichTextTag.LineHeight:
                                lineHeight = value = info.ToAnimatableFloat(Constants.DefaultLineHeight);
                                break;
                            case RichTextTag.Size:
                                float add = info.AddType != MeasurementInfo.AdditionType.Default && info.Unit == MeasurementUnit.Pixels ? Constants.EmSize : 0;
                                value = info.ToAnimatableFloat(Constants.EmSize, add);

                                SizeStack.Push(value);

                                break;
                            case RichTextTag.VOffset:
                                // TODO: support voffset
                                return false;
                        }

                        string tagName = text[(startPos + 1)..oldPos];
                        int count = Count();

                        if (value.IsAnimated)
                        {
                            Modifications.Add(new AnimatedTagModification(startPos, count, tagName, in value)); // TODO: check for off by one errors
                        }
                        else
                        {
                            Modifications.Add(new TagModification(startPos, count, tagName, value.AddendOrValue)); // TODO: check for off by one errors
                        }

                        return true;
                    }

                    position = oldPos;

                    return false;
                }
            }

            if ((node = node[TagHelpers.ToLowercaseFast(ch)]) == null)
            {
                HandleChar(ch);

                return true;
            }

            position++;
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
                    Modifications.Add(new CharModification(position, '{', true));
                    break;
                case '}':
                    Modifications.Add(new CharModification(position, '}', true));
                    break;
                default:
                    Unsafe.SkipInit(out num);
                    return false;
            }

            num = -1;
            return true;
        }

        // TODO: handle singular { or }
        if (cur == '{')
        {
            const int MaxNumLength = 5;

            if (MoveNext())
            {
                if (text[position] == '{')
                {
                    Modifications.Add(new NobreakModification(position, 2));

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
                            Modifications.Add(new InvalidFormatItemModification(position, length + 1)); // TODO: check for potential off by one errors

                            // doesn't really matter what we do here
                            Unsafe.SkipInit(out num);
                            return false;
                        }

                        // since we load the parameters in reverse order, we get the opposite index here
                        int index = CurrentParameters.Count - result;

                        // TODO: add skip for to tag
                        // TODO: opposite index
                        Modifications.Add(new FormatItemModification(position, length + 1, index)); // TODO: check for potential off by one errors
                        num = index;

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
            Modifications.Add(new NobreakModification(position, 2));
            num = -1;

            return true;
        }

        Unsafe.SkipInit(out num);
        return false;
    }

    private static bool TryParseMeasurements(int count, out MeasurementInfo info)
    {
        // TODO: support weird leading tag shit
        // TODO: break if too many spaces, support otehr weird space shit
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
            if (TryGetNext(out ch) && ++count <= Constants.MaxTagLength) // TODO: check
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
                _ when TagHelpers.IsDigitFast(ch) => MeasurementInfo.AdditionType.Default,
                _ => MeasurementInfo.AdditionType.Absolute,
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
                    if (paramId == -1)
                    {
                        float value = decimalPoint == -1
                            ? TagHelpers.FromIntegerAndDecimal(CharBuffer[..numDigits], ReadOnlySpan<char>.Empty)
                            : TagHelpers.FromIntegerAndDecimal(CharBuffer[..decimalPoint], CharBuffer[decimalPoint..numDigits]);

                        if (value > Constants.MaxValueSize)
                        {
                            info = default;

                            return false;
                        }

                        info = new()
                        {
                            Unit = unit == NotSet ? MeasurementUnit.Pixels : unit,
                            AddType = type,
                            Value = value,
                        };
                    }
                    else
                    {
                        if (CurrentParameters[paramId] is not AnimatedParameter animated
                            || animated.Format != null // no format is allowed for tags
                            || animated.RoundToInt
                            || animated.Value.Any(x => x.value > Constants.MaxValueSize / 2))
                        {
                            info = default;
                            return false;
                        }

                        info = new()
                        {
                            Unit = unit == NotSet ? MeasurementUnit.Pixels : unit,
                            AddType = type,
                            Parameter = animated,
                            Value = 0,
                        };
                    }

                    return true;
                default:
                    if (unit == NotSet && type != MeasurementInfo.AdditionType.Absolute)
                    {
                        switch (ch)
                        {
                            case 'e':
                                unit = MeasurementUnit.Pixels;
                                break;
                            case '%':
                                unit = MeasurementUnit.Percentage;
                                break;
                            case ' ': // if there is no leading values, space breaks (no idea why)
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
                            if (paramId != -1 || unit != NotSet || decimalPoint != -1 || numDigits != -1 || comma)
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
            // additional backslash to ensure the escape sequence isn't handled
            // even if we have something <noparse>\\n</noparse>, we add two backslashes so the \n is
            // never parsed
            Modifications.Add(new CharModification(position, '\\', false)); // TODO: check for potential off by one errors

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
                    Modifications.Add(new CharModification(position, '\\', false)); // TODO: check for potential off by one errors
                    ch = '\\';
                    return true;
                case 'u':
                    // newPos is currently at |u, move to u|
                    const int UnicodeLiteralSize = 4;

                    if (++newPos + (UnicodeLiteralSize - 1) < text.Length)
                    {
                        if (int.TryParse(text.AsSpan(newPos, UnicodeLiteralSize), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value))
                        {
                            // TODO: check
                            position = newPos + UnicodeLiteralSize - 1;
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
        if (noparse)
        {
            Modifications.Add(new NoparseModification(position - 1));
        }
        else
        {
            Modifications.Add(new CloseNoparseModification(position - 1));
        }
    }

    private static void AddLinebreak()
    {
        if (!lineHeight.IsInvalid)
        {
            Offset.Add(in lineHeight);
        }
        else if (SizeStack.TryPeek(out AnimatableFloat value))
        {
            const float SizeToLineHieght = Constants.DefaultLineHeight / Constants.EmSize;

            Offset.Add(value with { Multiplier = value.Multiplier * SizeToLineHieght });
        }
        else
        {
            Offset.Add(Constants.DefaultLineHeight);
        }
    }

    private struct MeasurementInfo
    {
        public MeasurementUnit Unit;

        public AdditionType AddType;

        public float Value;

        public AnimatedParameter? Parameter;

        internal enum AdditionType
        {
            Default,
            Absolute,
            Additive,
            SubtractiveOrNegative,
        }

        public readonly AnimatableFloat ToAnimatableFloat(float pointSize, float add = 0)
        {
            float multi = this.AddType == AdditionType.SubtractiveOrNegative ? -1f : 1f;

            if (add == 0 && this.Unit != MeasurementUnit.Pixels)
            {
                add = 0; // TODO: check this
            }

            if (this.Parameter == null)
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
                    MeasurementUnit.Percentage => new(this.Parameter, add, pointSize / 100 * multi, abs),
                    MeasurementUnit.Ems => new(this.Parameter, add, Constants.EmSize * multi, abs),
                    _ => new(this.Parameter, add, multi, abs),
                };
            }
        }
    }
}