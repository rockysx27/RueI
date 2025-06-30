namespace RueI.API.Parsing;

using System;
using System.Text;

using NorthwoodLib.Pools;

using RueI.API.Elements;
using RueI.Utils;
using RueI.Utils.Enums;
using RueI.Utils.Extensions;

public static class Parser
{
    public static ParsedData Parse(string text, Element element)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        ParserContext context = new();

        for (int i = 0; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '<':
                    ReadOnlySpan<char> span = text[++i..];

                    if (TryParseTag(span, out TagInfo info))
                    {
                        i += info.Length - 1; // TODO: check this

                        if ()
                    }

                    continue;
                case '\n':
                    AddNewline();
                    break;

            }

            if (text[i] == '}')
            {
                ReadOnlySpan<char> span = text[++i..];

                TryParseTag(span, out TagInfo _);

                continue;
            }
            else if (text[i] == '\n')
            {
                AddNewline();
            }
        }
    }

    private static bool TryParseTag(ReadOnlySpan<char> span, out TagInfo info)
    {
        var name = span[..16].TakeWhile(TagHelpers.IsTagNameChar); // limit to first 16 characters

        int pos = name.Length;
        if (pos != span.Length)
        {
            char ch = span[pos];

            if (ch == '>')
            {
                info = new()
                {
                    TagName = name,
                    Length = pos + 2, // we do + 2 to include both the < and the >
                };

                return true;
            }
            else if (ch == '=')
            {
                pos++;

                ReadOnlySpan<char> tagValue = span.Terminated('>');
                if (tagValue != ReadOnlySpan<char>.Empty && TryParseNumber(tagValue, out MeasurementInfo measurement))
                {
                    pos += tagValue.Length + 1;

                    // pos doesn't include the < (but DOES include the >)
                    if (pos < Constants.MaxTagLength)
                    {
                        info = new()
                        {
                            TagName = name,
                            Measurements = measurement,
                            Length = pos,
                        };

                        return true;
                    }
                }
            }
        }

        info = default;
        return false;
    }

    private static bool TryParseNumber(ReadOnlySpan<char> span, out MeasurementInfo info)
    {
        // TODO: support empty
        if (span.Length == 0)
        {
            info = default;
            return false;
        }

        int numDecimals = -1; // -1 = no decimal place encountered (not yet adding characters to decimal)
        int value = 0; // by default, if we have no numbers (e.g. <line-height=>), the value is 0
        MeasurementUnit unit = MeasurementUnit.Pixels;
        for (int i = 0; i < span.Length; i++)
        {
            char c = span[i];
            switch (c)
            {
                case '-' when i == 0:
                    value = -0;
                    break;

                // the way tmp works is that upon encountering a unit character, it immediately starts ignoring
                // the rest of the characters. thus we break when we encounter one
                case 'e':
                    unit = MeasurementUnit.Ems;
                    goto END_LOOP;
                case '%':
                    unit = MeasurementUnit.Percentage;
                    goto END_LOOP;
                case 'p':
                    goto END_LOOP; // default is already pixels, doesn't do anything
                case '.' when numDecimals == -1:
                    numDecimals = 0;
                    break;
                default:
                    if (char.IsDigit(c))
                    {
                        value *= 10;
                        value += (int)(c - '0'); // convert e.g. '5' -> 5

                        if (numDecimals > -1)
                        {
                            numDecimals++;
                        }
                    }

                    break;
            }
        }

    END_LOOP: // break out of the loop

        float floatValue = ((float)value) / UnityEngine.Mathf.Pow(value, Math.Min(numDecimals, 0));

        info = new()
        {
            Unit = unit,
            Value = floatValue,
        };

        return true;
    }

    private char ParseChar(ref ReadOnlySpan<char> span, bool noParse)
    {
        if (noParse)
        {
            if 
        }
        else
        {

        }
    }

    private struct MeasurementInfo
    {
        public MeasurementUnit Unit;

        public float Value;
    }

    private ref struct TagInfo
    {
        public ReadOnlySpan<char> TagName;

        public MeasurementInfo? Measurements;

        public int Length;
    }
}