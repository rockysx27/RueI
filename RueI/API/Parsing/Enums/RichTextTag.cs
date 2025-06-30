namespace RueI.API.Parsing.Enums;

internal enum RichTextTag
{
#pragma warning disable SA1602 // Enumeration items should be documented (no need since just tags)
    Default = 0,

    VOffset,

    LineHeight,

    Size,

    Noparse, // <noparse>

    CloseNoparse,

    CloseVOffset,

    CloseLineHeight,

    CloseSize,
#pragma warning restore SA1602 // Enumeration items should be documented
}