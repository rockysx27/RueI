namespace RueI.API.Parsing;

using System;
using System.Collections.Generic;
using System.Text;
using NorthwoodLib.Pools;

public static class Parser
{
    public static ParsedData Parse(string text)
    {
        StringBuilder sb = StringBuilderPool.Shared.Rent(text.Length);

        bool _ = false;

        float lineHeight = 40.665f;
        float offset = 0;

        void AddNewline()
        {
            sb.Append($"<line-height={lineHeight}>\n<line-height=0>"); // prevent overflows from happening
            offset += lineHeight;
        }

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '<')
            {
                ReadOnlySpan<char> span = text[++i..];

                if (span.StartsWith("br"))
                {

                }

                continue;
            }
            else if (text[i] == '\n')
            {
                AddNewline();
            }
        }
    }

    private static void AddNewline(StringBuilder sb, ref float offset)
    {

    }
}