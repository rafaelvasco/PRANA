using System.Diagnostics;
using PRANA.Common;

namespace PRANA;

public class Font : Asset
{
    public Texture2D Texture => _texture;

    public Glyph[] Glyphs { get; }

    public float LineSpacing { get; set; }

    public float Spacing { get; set; }

    public char? DefaultCharacter
    {
        get => _defaultChar;
        set
        {
            // Get the default glyph index here once.
            if (value.HasValue)
            {
                if (!TryGetGlyphIndex(value.Value, out _defaultGlyphIndex))
                    throw new ArgumentException("Character not present in Font.");
            }
            else
            {
                _defaultGlyphIndex = -1;
            }

            _defaultChar = value;
        }
    }

    private readonly Texture2D _texture;

    private readonly CharRegion[] _regions;

    private char? _defaultChar;

    private int _defaultGlyphIndex = -1;

    internal Font(Texture2D texture, FontData fontData)
    {
        _texture = texture;

        LineSpacing = 20;

        Spacing = 0;

        Glyphs = new Glyph[fontData.Chars.Length];

       
        var texRects = fontData.GlyphRects;
        var chars = fontData.Chars;
        var offsets = fontData.GlyphOffsets;
        var advances = fontData.GlyphAdvances;

        var regions = new Stack<CharRegion>();

        for (var i = 0; i < chars.Length; ++i)
        {
            Glyphs[i] = new Glyph(
                chars[i],
                texRects[i],
                offsets[i].X,
                offsets[i].Y,
                advances[i]
            );

            if (regions.Count == 0 || chars[i] > regions.Peek().End + 1)
            {
                // New Region

                regions.Push(new CharRegion(chars[i], i));
            }
            else if (chars[i] == regions.Peek().End + 1)
            {
                // Add char in current region

                var currentRegion = regions.Pop();

                currentRegion.End++;
                regions.Push(currentRegion);
            }
            else
            {
                throw new InvalidOperationException(
                    "Invalid TextureFont. Character map must be in ascending order.");
            }
        }

        _regions = regions.ToArray();

        Array.Reverse(_regions);

        DefaultCharacter = '?';
    }

    //public Vector2 MeasureString(string text)
    //{
    //    var source = new CharSource(text);
    //    MeasureString(ref source, out var size);
    //    return size;
    //}

    //public Vector2 MeasureString(StringBuilder text)
    //{
    //    var source = new CharSource(text);
    //    MeasureString(ref source, out var size);
    //    return size;
    //}

    //internal unsafe void MeasureString(ref CharSource text, out Vector2 size)
    //{
    //    if (text.Length == 0)
    //    {
    //        size = Vector2.Zero;
    //        return;
    //    }

    //    var width = 0.0f;

    //    var maxGlyphHeight = 0;

    //    var offset = Vector2.Zero;

    //    var firstGlyphOfLine = true;

    //    var lines = 1;

    //    fixed (Glyph* pGlyphs = Glyphs)
    //    {
    //        for (var i = 0; i < text.Length; ++i)
    //        {
    //            var c = text[i];

    //            switch (c)
    //            {
    //                case '\r':
    //                    continue;
    //                case '\n':
    //                    lines += 1;

    //                    offset.X = 0;
    //                    offset.Y += LineSpacing;
    //                    firstGlyphOfLine = true;
    //                    continue;
    //            }

    //            var currentGlyphIndex = GetGlyphIndexOrDefault(c);

    //            Debug.Assert(currentGlyphIndex >= 0 && currentGlyphIndex < Glyphs.Length,
    //                "currentGlyphIndex was outside array bounds.");

    //            var pCurrentGlyph = pGlyphs + currentGlyphIndex;

    //            if (firstGlyphOfLine)
    //            {
    //                offset.X = Calc.Max(pCurrentGlyph->BearingX, 0);
    //                firstGlyphOfLine = false;
    //            }
    //            else
    //            {
    //                offset.X += Spacing + pCurrentGlyph->BearingX;
    //            }

    //            offset.X += pCurrentGlyph->TextureRect.Width;

    //            var proposedWidth = offset.X;

    //            if (i < text.Length - 1 && text[i + 1] != '\n')
    //                proposedWidth += Calc.Max(pCurrentGlyph->BearingX, 0);

    //            if (proposedWidth > width) width = proposedWidth;

    //            offset.X += pCurrentGlyph->BearingX;

    //        }

    //        size.X = width;
    //        size.Y = maxGlyphHeight * lines + (LineSpacing - maxGlyphHeight) * (lines - 1);
    //    }
    //}

   public int GetGlyphIndexOrDefault(char c)
    {
        if (TryGetGlyphIndex(c, out var glyphIdx)) return glyphIdx;

        if (_defaultGlyphIndex == -1) throw new Exception("Text contains unresolvable characters");

        return _defaultGlyphIndex;
    }

    private unsafe bool TryGetGlyphIndex(char c, out int index)
    {
        // Do a binary search on char regions

        fixed (CharRegion* pRegions = _regions)
        {
            var regionIdx = -1;
            var left = 0;
            var right = _regions.Length - 1;

            while (left <= right)
            {
                var mid = (left + right) / 2;

                Debug.Assert(mid >= 0 && mid < _regions.Length, "Index was outside of array bounds");

                if (pRegions[mid].End < c)
                {
                    left = mid + 1;
                }
                else if (pRegions[mid].Start > c)
                {
                    right = mid - 1;
                }
                else
                {
                    regionIdx = mid;
                    break;
                }
            }

            if (regionIdx == -1)
            {
                index = -1;
                return false;
            }

            index = pRegions[regionIdx].StartIndex + (c - pRegions[regionIdx].Start);
        }

        return true;
    }


}