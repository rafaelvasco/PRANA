using PRANA.Common;

namespace PRANA;

public readonly struct Glyph
{
    public readonly char Character;

    public readonly Rectangle TextureRect;

    public readonly float OffsetX;

    public readonly float OffsetY;

    public readonly float Advance;


    public static readonly Glyph Empty = new();

    public Glyph(
        char ch, 
        Rectangle textureRect, 
        float offsetX, 
        float offsetY, 
        float advance)
    {
        Character = ch;
        TextureRect = textureRect;
        OffsetX = offsetX;
        OffsetY = offsetY;
        Advance = advance;
    }
}