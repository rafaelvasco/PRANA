namespace PRANA.Common;

public readonly struct FontData
{
    public string Id { get; init; }

    public ImageData FontSheet { get; init; }

    public char[] Chars { get; init; }

    public Rectangle[] GlyphRects { get; init; }

    public Vector2[] GlyphOffsets { get; init; }

    public int[] GlyphAdvances { get; init; }
    
    public FontData(
        string id,
        ImageData fontSheet,
        char[] chars,
        Rectangle[] glyphRects,
        Vector2[] glyphOffsets,
        int[] glyphAdvances
    )
    {
        Id = id;
        FontSheet = fontSheet;
        Chars = chars;
        GlyphRects = glyphRects;
        GlyphOffsets = glyphOffsets;
        GlyphAdvances = glyphAdvances;
    }
}