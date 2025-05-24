namespace RueI.Utils;

using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using NorthwoodLib.Pools;
using RueI.Utils.Enums;

/// <summary>
/// Provides extensions for adding rich text tags to <see cref="StringBuilder"/>s.
/// </summary>
/// <example>
/// This example demonstrates simple usage of the <see cref="HintBuilding"/> methods. Note that you do not
/// need a custom <see cref="StringBuilder"/> to use the methods.
/// <code>
/// <include file="Documentation.xml" path="doc/members/member[@name='RueI.Utils.HintBuilding']/example"/>
/// </code>
/// </example>
public static class HintBuilding
{
    private const string EMS = "ems";
    private const string PERCENT = "%";

    /// <summary>
    /// Represents all of the options for the alignment of a string of text.
    /// </summary>
    public enum AlignStyle
    {
        /// <summary>
        /// Indicates that the text should be left-aligned.
        /// </summary>
        Left,

        /// <summary>
        /// Indicates that the text should be center-aligned.
        /// </summary>
        Center,

        /// <summary>
        /// Indicates that the text should be right-aligned.
        /// </summary>
        Right,

        /// <summary>
        /// Indicates that every line should be stretched to fill the display area, excluding the last line.
        /// </summary>
        Justified,

        /// <summary>
        /// Indicates that every line should be stretched to fill the display area, including the last line.
        /// </summary>
        Flush,
    }

    /// <summary>
    /// Represents the case style of text.
    /// </summary>
    public enum CaseStyle
    {
        /// <summary>
        /// Indicates that all text will be in uppercase, but lowercase characters will be slightly smaller.
        /// </summary>
        Smallcaps,

        /// <summary>
        /// Indicates that all text will be in lowercase.
        /// </summary>
        Lowercase,

        /// <summary>
        /// Indicates that all text will be in uppercase.
        /// </summary>
        Uppercase,
    }

    /// <summary>
    /// Converts a <see cref="Color"/> to a hex code string.
    /// </summary>
    /// <param name="color">The <see cref="Color"/> to convert.</param>
    /// <returns>The color as a hex code string.</returns>
    public static string ConvertToHex(Color color)
    {
        string alphaInclude = color.A switch
        {
            255 => string.Empty,
            _ => color.A.ToString("X2"),
        };

        return $"#{color.R:X2}{color.G:X2}{color.B:X2}{alphaInclude}";
    }

    /// <summary>
    /// Converts a <see cref="UnityEngine.Color"/> to a system <see cref="Color"/>.
    /// </summary>
    /// <param name="unityColor">The <see cref="UnityEngine.Color"/> to convert.</param>
    /// <returns>The converted <see cref="Color"/>.</returns>
    public static Color UnityToSystemColor(UnityEngine.Color unityColor)
    {
        return Color.FromArgb((int)(255 * unityColor.a), (int)(255 * unityColor.r), (int)(255 * unityColor.g), (int)(255 * unityColor.b));
    }

    /// <summary>
    /// Creates a <see langword="string"/> that has additional tags to preserve the true case of the string.
    /// </summary>
    /// <param name="str">The <see langword="string"/> to add the tags to.</param>
    /// <returns>The <see langword="string"/> with additional tags that preserve casing.</returns>
    /// <remarks>
    /// <inheritdoc cref="AppendPreservedLowercase(StringBuilder, string)" path="/remarks"/>
    /// <br/>
    /// If you are appending to an existing <see cref="StringBuilder"/>, use the <see cref="AppendPreservedLowercase(StringBuilder, string)"/>
    /// method, as it is faster.
    /// </remarks>
    public static string PreserveLowercase(string str)
    {
        StringBuilder sb = StringBuilderPool.Shared.Rent(str.Length).AppendPreservedLowercase(str);

        return StringBuilderPool.Shared.ToStringReturn(sb);
    }

    /// <summary>
    /// Sanitizes a <see langword="string"/> so that it has no tags by wrapping it in noparse and sanitizing any close noparses.
    /// </summary>
    /// <param name="str">The <see langword="string"/> to sanitize.</param>
    /// <returns>The sanitized <see langword="string"/> with additional tags that preserve casing.</returns>
    /// <remarks>
    /// <inheritdoc cref="AppendPreservedLowercase(StringBuilder, string)" path="/remarks"/>
    /// <br/>
    /// If you are appending to an existing <see cref="StringBuilder"/>, use the <see cref="AppendPreservedLowercase(StringBuilder, string)"/>
    /// method, as it is faster.
    /// </remarks>
    public static string Sanitize(string str)
    {
        return "<noparse>" + str.Replace("<noparse>", "<nopa‌rse>") + "</noparse>"; // zero width non join
    }

    /// <summary>
    /// Appends a <see langword="string"/> that has additional tags to preserve the true case of the string.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="str">The <see langword="string"/> to add, with preserved case. the tags to.</param>
    /// <returns>The same <see langword="string"/> with additional tags that preserve casing.</returns>
    /// <remarks>
    /// By default, lowercase text appear as smallcaps. This method ensures lowercase characters are truly lowercase.
    /// </remarks>
    public static StringBuilder AppendPreservedLowercase(this StringBuilder sb, string str)
    {
        bool lastLowercase = false;

        foreach (char c in str)
        {
            if (char.IsLower(c))
            {
                if (!lastLowercase)
                {
                    sb.Append("<lowercase>");
                }

                lastLowercase = true;
            }
            else if (char.IsUpper(c) && lastLowercase) // ignore characters that don't change based on case to avoid unnecessary </lowercase> and <lowercase>
            {
                sb.Append("</lowercase>");

                lastLowercase = false;
            }

            sb.Append(c);
        }

        return sb;
    }

    /// <summary>
    /// Adds a linebreak to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder AddLinebreak(this StringBuilder sb) => sb.Append('\n');

    /// <summary>
    /// Adds an alignment tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="align">The <see cref="AlignStyle"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetAlignment(this StringBuilder sb, AlignStyle align)
    {
        string alignment = align switch
        {
            AlignStyle.Left => "left",
            AlignStyle.Right => "right",
            AlignStyle.Justified => "justified",
            AlignStyle.Flush => "flush",
            _ => "center",
        };

        return sb.Append($"<align={alignment}>");
    }

    /// <summary>
    /// Adds a size tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="size">The size to include in the size tag.</param>
    /// <param name="unit">The measurement unit of the size tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetSize(this StringBuilder sb, float size, MeasurementUnit unit = MeasurementUnit.Pixels)
    {
        return sb.AddMeasurementTag("cspace", size, unit);
    }

    /// <summary>
    /// Adds a size tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="size">The size to include in the character spacing tag.</param>
    /// <param name="unit">The measurement unit of the character space tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetCharacterSpace(this StringBuilder sb, float size, MeasurementUnit unit = MeasurementUnit.Pixels)
    {
        return sb.AddMeasurementTag("cspace", size, unit);
    }

    /// <summary>
    /// Adds a line-height tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="size">The line height to include in the line-height tag.</param>
    /// <param name="unit">The measurement unit of the line-height tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetLineHeight(this StringBuilder sb, float size, MeasurementUnit unit = MeasurementUnit.Pixels)
    {
        return sb.AddMeasurementTag("line-height", size, unit);
    }

    /// <summary>
    /// Adds an indent tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="indent">The indent size to include in the indent tag.</param>
    /// <param name="unit">The measurement unit of the indent tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetIndent(this StringBuilder sb, float indent, MeasurementUnit unit = MeasurementUnit.Pixels)
    {
        return sb.AddMeasurementTag("indent", indent, unit);
    }

    /// <summary>
    /// Adds a monospace tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="spacing">The size of the spacing.</param>
    /// <param name="unit">The measurement unit of the monospacing tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetMonospace(this StringBuilder sb, float spacing, MeasurementUnit unit = MeasurementUnit.Pixels)
    {
        return sb.AddMeasurementTag("mspace", spacing, unit);
    }

    /// <summary>
    /// Adds an margins tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="margins">The size of the margins.</param>
    /// <param name="unit">The measurement unit of the margins tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetMargins(this StringBuilder sb, float margins, MeasurementUnit unit = MeasurementUnit.Pixels)
    {
        return sb.AddMeasurementTag("margins", margins, unit);
    }

    /// <summary>
    /// Adds a pos tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="pos">The size of the pos tag.</param>
    /// <param name="unit">The measurement unit of the pos tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetHorizontalPos(this StringBuilder sb, float pos, MeasurementUnit unit = MeasurementUnit.Pixels)
    {
        return sb.AddMeasurementTag("pos", pos, unit);
    }

    /// <summary>
    /// Adds a voffset tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="offset">The size of the voffset tag.</param>
    /// <param name="unit">The measurement unit of the voffset tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetVOffset(this StringBuilder sb, float offset, MeasurementUnit unit = MeasurementUnit.Pixels)
    {
        return sb.AddMeasurementTag("voffset", offset, unit);
    }

    /// <summary>
    /// Adds a width tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="offset">The size of the new width.</param>
    /// <param name="unit">The measurement style of the width tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetWidth(this StringBuilder sb, float offset, MeasurementUnit unit = MeasurementUnit.Pixels)
    {
        return sb.AddMeasurementTag("space", offset, unit);
    }

    /// <summary>
    /// Adds a color tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="color">The color to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetColor(this StringBuilder sb, Color color)
    {
        return sb.Append($"<color={ConvertToHex(color)}>");
    }

    /// <summary>
    /// Adds a color tag to a <see cref="StringBuilder"/> using a <see cref="UnityEngine.Color"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="color">The <see cref="UnityEngine.Color"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetColor(this StringBuilder sb, UnityEngine.Color color) => sb.SetColor(UnityToSystemColor(color));

    /// <summary>
    /// Adds a color tag to a <see cref="StringBuilder"/> from RGBA values.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="r">The red (0-255) in the color.</param>
    /// <param name="g">The green (0-255) in the color.</param>
    /// <param name="b">The blue (0-255) in the color.</param>
    /// <param name="alpha">The optional alpha (0-255) of the color.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetColor(this StringBuilder sb, int r, int g, int b, int alpha = 255)
    {
        Color color = Color.FromArgb(alpha, r, g, b);
        return sb.SetColor(color);
    }

    /// <summary>
    /// Adds a mark tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="color">The color to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetMark(this StringBuilder sb, Color color)
    {
        return sb.Append($"<mark={ConvertToHex(color)}>");
    }

    /// <summary>
    /// Adds a mark tag to a <see cref="StringBuilder"/> using a <see cref="UnityEngine.Color"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="color">The <see cref="UnityEngine.Color"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetMark(this StringBuilder sb, UnityEngine.Color color) => sb.SetColor(UnityToSystemColor(color));

    /// <summary>
    /// Adds a mark tag to a <see cref="StringBuilder"/> from RGBA values.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="r">The red (0-255) in the color.</param>
    /// <param name="g">The green (0-255) in the color.</param>
    /// <param name="b">The blue (0-255) in the color.</param>
    /// <param name="alpha">The alpha (0-255) of the color.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetMark(this StringBuilder sb, int r, int g, int b, int alpha)
    {
        Color color = Color.FromArgb(alpha, r, g, b);
        return sb.SetMark(color);
    }

    /// <summary>
    /// Adds an alpha tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="alpha">The alpha (0-255) of the color.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetAlpha(this StringBuilder sb, int alpha) => sb.Append($"<alpha={alpha:X2}>");

    /// <summary>
    /// Adds a bold tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetBold(this StringBuilder sb) => sb.Append("<b>");

    /// <summary>
    /// Adds an italics tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetItalics(this StringBuilder sb) => sb.Append("<i>");

    /// <summary>
    /// Adds a strikethrough tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetStrikethrough(this StringBuilder sb) => sb.Append("<s>");

    /// <summary>
    /// Adds an underline tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetUnderline(this StringBuilder sb) => sb.Append("<u>");

    /// <summary>
    /// Adds a horizontal scale tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="scale">The scale size to include in the scale tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetScale(this StringBuilder sb, float scale) => sb.Append($"<scale={scale}>");

    /// <summary>
    /// Adds a case tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="caseStyle">The case to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetCase(this StringBuilder sb, CaseStyle caseStyle)
    {
        string format = caseStyle switch
        {
            CaseStyle.Uppercase => "allcaps", // slightly smaller than "uppercase"
            CaseStyle.Lowercase => "lowercase",
            _ => "smallcaps",
        };

        return sb.Append($"<{format}>");
    }

    /// <summary>
    /// Adds a nobreak tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetNobreak(this StringBuilder sb) => sb.Append("<nobr>");

    /// <summary>
    /// Adds a noparse tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetNoparse(this StringBuilder sb) => sb.Append("<noparse>");

    /// <summary>
    /// Adds a rotation tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="rotation">The rotation (-180 to 180) of the tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetRotation(this StringBuilder sb, int rotation) => sb.Append($"<rotate=\"{rotation}\">");

    /// <summary>
    /// Adds a subscript tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetSubscript(this StringBuilder sb) => sb.Append("<sub>");

    /// <summary>
    /// Adds a superscript tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder SetSuperscript(this StringBuilder sb) => sb.Append("<sup>");

    /// <summary>
    /// Adds a space tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="space">The amount of space to add.</param>
    /// <param name="unit">The measurement unit of the pos tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder AddSpace(this StringBuilder sb, float space, MeasurementUnit unit = MeasurementUnit.Pixels)
    {
        return sb.AddMeasurementTag("space", space, unit);
    }

    /// <summary>
    /// Adds a sprite tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="index">The index (0-20) of the sprite tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder AddSprite(this StringBuilder sb, int index) => sb.Append($"<sprite={index}>");

    /// <summary>
    /// Adds a sprite tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="index">The index (0-20) of the sprite tag.</param>
    /// <param name="color">The color of the sprite tag.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder AddSprite(this StringBuilder sb, int index, Color color) => sb.Append($"<sprite index={index} color={ConvertToHex(color)}>");

    /// <summary>
    /// Adds a closing color tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseColor(this StringBuilder sb) => sb.Append("</color>");

    /// <summary>
    /// Adds a closing align tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseAlign(this StringBuilder sb) => sb.Append("</align>");

    /// <summary>
    /// Adds a closing alpha tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseAlpha(this StringBuilder sb) => sb.Append("</alpha>");

    /// <summary>
    /// Adds a closing size tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseSize(this StringBuilder sb) => sb.Append("</size>");

    /// <summary>
    /// Adds a closing line-height tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseLineHeight(this StringBuilder sb) => sb.Append("</line-height>");

    /// <summary>
    /// Adds a closing bold tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseBold(this StringBuilder sb) => sb.Append("</b>");

    /// <summary>
    /// Adds a closing italics tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseItalics(this StringBuilder sb) => sb.Append("</i>");

    /// <summary>
    /// Adds a closing strikethrough tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseStrikethrough(this StringBuilder sb) => sb.Append("</s>");

    /// <summary>
    /// Adds a closing underline tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseUnderline(this StringBuilder sb) => sb.Append("</u>");

    /// <summary>
    /// Adds a closing indent tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseIndent(this StringBuilder sb) => sb.Append("</indent>");

    /// <summary>
    /// Adds a closing scale tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseScale(this StringBuilder sb) => sb.Append("</scale>");

    /// <summary>
    /// Adds a closing monospace tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseMonospace(this StringBuilder sb) => sb.Append("</mspace>");

    /// <summary>
    /// Adds a closing subscript tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseSubscript(this StringBuilder sb) => sb.Append("</sub>");

    /// <summary>
    /// Adds a closing superscript tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseSuperscript(this StringBuilder sb) => sb.Append("</sup>");

    /// <summary>
    /// Adds a closing rotation tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseRotation(this StringBuilder sb) => sb.Append("</rotate>");

    /// <summary>
    /// Adds a closing margins tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseMargins(this StringBuilder sb) => sb.Append("</margins>");

    /// <summary>
    /// Adds a closing mark tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseMark(this StringBuilder sb) => sb.Append("</mark>");

    /// <summary>
    /// Adds a closing nobreak tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseNobreak(this StringBuilder sb) => sb.Append("</nobr>");

    /// <summary>
    /// Adds a closing noparse tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseNoparse(this StringBuilder sb) => sb.Append("</noparse>");

    /// <summary>
    /// Adds a closing cspace tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseCharacterSpace(this StringBuilder sb) => sb.Append("</cspace>");

    /// <summary>
    /// Adds a closing voffset tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseVOffset(this StringBuilder sb) => sb.Append("</voffset>");

    /// <summary>
    /// Adds a closing pos tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseHorizontalPos(this StringBuilder sb) => sb.Append("</pos>");

    /// <summary>
    /// Adds a closing width tag to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <returns>A reference to the original <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CloseWidth(this StringBuilder sb) => sb.Append("</width>");

    private static StringBuilder AddMeasurementTag(this StringBuilder sb, string tag, float value, MeasurementUnit unit)
    {
        string format = unit switch
        {
            MeasurementUnit.Percentage => PERCENT,
            MeasurementUnit.Ems => EMS,
            _ => string.Empty, // efault is p
        };

        return sb.Append('<')
          .Append(tag)
          .Append('=')
          .AppendFormat(format, "0.###"); // up to 3 degrees of precision
    }
}