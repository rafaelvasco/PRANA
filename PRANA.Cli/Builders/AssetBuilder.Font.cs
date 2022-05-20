using System.Runtime.CompilerServices;
using BinaryPack;
using PRANA.Common;
using PRANA.Common.SDL;
using static PRANA.Common.SDL.SDL_ttf;

namespace PRANA;

internal struct GlyphInfo
{
    public int X;
    public int Y;
    public int Width;
    public int Height;
    public int OffsetX;
    public int OffsetY;
    public int Advance;
}

internal readonly struct CharRange
{
    private static readonly CharRange BasicLatin = new(0x0020, 0x007F);
    private static readonly CharRange Latin1Supplement = new(0x00A0, 0x00FF);
    private static readonly CharRange LatinExtendedA = new(0x0100, 0x017F);
    private static readonly CharRange LatinExtendedB = new(0x0180, 0x024F);
    private static readonly CharRange Cyrillic = new(0x0400, 0x04FF);
    private static readonly CharRange CyrillicSupplement = new(0x0500, 0x052F);
    private static readonly CharRange Hiragana = new(0x3040, 0x309F);
    private static readonly CharRange Katakana = new(0x30A0, 0x30FF);
    private static readonly CharRange Greek = new(0x0370, 0x03FF);
    private static readonly CharRange CjkSymbolsAndPunctuation = new(0x3000, 0x303F);
    private static readonly CharRange CjkUnifiedIdeographs = new(0x4e00, 0x9fff);
    private static readonly CharRange HangulCompatibilityJamo = new(0x3130, 0x318f);
    private static readonly CharRange HangulSyllables = new(0xac00, 0xd7af);

    private static readonly Dictionary<string, CharRange> Map = new()
    {
        { "BasicLatin", BasicLatin },
        { "Latin1Supplement", Latin1Supplement },
        { "LatinExtendedA", LatinExtendedA },
        { "LatinExtendedB", LatinExtendedB },
        { "Cyrillic", Cyrillic },
        { "CyrillicSupplement", CyrillicSupplement },
        { "Hiragana", Hiragana },
        { "Katakana", Katakana },
        { "Greek", Greek },
        { "CjkSymbolsAndPunctuation", CjkSymbolsAndPunctuation },
        { "CjkUnifiedIdeographs", CjkUnifiedIdeographs },
        { "HangulCompatibilityJamo", HangulCompatibilityJamo },
        { "HangulSyllables", HangulSyllables }
    };

    public static CharRange GetFromKey(string key)
    {
        if (Map.TryGetValue(key, out var charRange)) return charRange;

        throw new Exception($"Invalid CharRange: {key}");
    }

    public int Start { get; }

    public int End { get; }

    private CharRange(int start, int end)
    {
        Start = start;
        End = end;
    }
}

internal static partial class AssetBuilder
{
    private static readonly Dictionary<int, int> FontSizeTextSizeMap = new()
    {
        { 25, 256 },
        { 50, 512 },
        { 100, 1024 },
        { 200, 2048 }
    };

    private const int MaxPermittedTexSize = 8192;

    private static int GetOptimalTextureSize(FontManifestInfo fontManifest)
    {
        var glyphSize = fontManifest.Size;
        var totalRanges = fontManifest.CharRanges.Length;

        foreach (var key in FontSizeTextSizeMap.Keys)
            if (glyphSize <= key)
            {
                var size = FontSizeTextSizeMap[key] * totalRanges;

                size = Calc.Min(size, MaxPermittedTexSize);

                return size;
            }

        return 4096;
    }

    private static void BuildAndExportFont(FontManifestInfo fontManifest, string assetsFolder)
    {
        Console.WriteLine($"Building font {fontManifest.Id}...");

        var fontData = BuildFont(fontManifest, assetsFolder);

        var assetBinPath = ContentProperties.GetAssetBinaryPath(assetsFolder, fontManifest);

        using var streamAsset = File.OpenWrite(assetBinPath);

        BinaryConverter.Serialize(fontData, streamAsset);

        Console.WriteLine($"Font {fontManifest.Id} built successfuly.");
    }

    private static unsafe FontData BuildFont(FontManifestInfo fontManifest, string assetsFolder)
    {
        if (TTF_Init() < 0)
        {
            throw new ApplicationException("Could not initialize SDL_ttf");
        }

        var sheetSize = GetOptimalTextureSize(fontManifest);

        var glyphDictionary = new Dictionary<char, GlyphInfo>();

        var fontSheetImageData = new ImageData($"{fontManifest.Id}_Texture", new byte[sheetSize * sheetSize * 4],
            sheetSize, sheetSize);

        var glyphRenderColor = new SDL.SDL_Color()
        {
            r = 255,
            g = 255,
            b = 255,
            a = 255
        };

        int offsetX = 0;
        int offsetY = 0;

        var fontPath = fontManifest.Path;

        var fontSize = fontManifest.Size;

        var fontFileDataPath = Path.Combine(assetsFolder, fontPath);

        var font = TTF_OpenFont(fontFileDataPath, fontSize);

        var lineSpacing = TTF_FontLineSkip(font);

        if (font == IntPtr.Zero)
        {
            throw new ApplicationException($"Could not load font: {fontManifest.Path}");
        }

        foreach (var charRangeKey in fontManifest.CharRanges)
        {
            var charRange = CharRange.GetFromKey(charRangeKey);

            for (uint ch = (uint)charRange.Start; ch < charRange.End; ++ch)
            {
                if (TTF_GlyphIsProvided(font, (ushort)ch) != 0)
                {
                    SDL.SDL_Surface* glyphSurface;

                    if (fontManifest.Filtered)
                    {
                        glyphSurface =
                            (SDL.SDL_Surface*)TTF_RenderGlyph_Blended(font, (ushort)ch, glyphRenderColor);
                    }
                    else
                    {
                        glyphSurface = (SDL.SDL_Surface*)TTF_RenderGlyph_Solid(font, (ushort)ch, glyphRenderColor);
                    }

                    if (glyphSurface != null)
                    {
                        glyphSurface = (SDL.SDL_Surface*)SDL.SDL_ConvertSurfaceFormat((IntPtr)glyphSurface,
                            SDL.SDL_PIXELFORMAT_ARGB8888, 0);

                        if (offsetX + glyphSurface->w >= sheetSize)
                        {
                            offsetY += lineSpacing;
                            offsetX = 0;
                        }

                        var dataLength = glyphSurface->h * glyphSurface->pitch;

                        #pragma warning disable CA2014
                        var bytes = stackalloc byte[dataLength];
                        #pragma warning restore CA2014

                        Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref bytes[0]), (void*)glyphSurface->pixels,
                            (uint)dataLength);

                        Blitter.Begin(fontSheetImageData.Data, sheetSize, sheetSize);
                        Blitter.Blit(new Span<byte>(bytes, dataLength), glyphSurface->w, glyphSurface->h, offsetX,
                            offsetY);
                        Blitter.End();

                        _ = TTF_GlyphMetrics(
                            font,
                            (ushort)ch,
                            out int minX, out _, out int minY, out _,
                            out int advance);

                        var glyphInfo = new GlyphInfo()
                        {
                            X = offsetX,
                            Y = offsetY,
                            Width = glyphSurface->w,
                            Height = glyphSurface->h,
                            OffsetX = minX,
                            OffsetY = minY,
                            Advance = advance
                        };

                        glyphDictionary.Add((char)ch, glyphInfo);

                        offsetX += glyphSurface->w + 1;

                        SDL.SDL_FreeSurface((IntPtr)glyphSurface);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Could not render glyph: {(char)ch}");
                    }
                }
            }
        }

        TTF_CloseFont(font);

        var orderedGlyphKeys = glyphDictionary.Keys.OrderBy(g => g);

        var index = 0;

        var charCount = glyphDictionary.Count;

        var chars = new char[charCount];
        var glyphRects = new Rectangle[charCount];
        var glyphOffsets = new Vector2[charCount];
        var glyphAdvances = new int[charCount];

        foreach (var charKey in orderedGlyphKeys)
        {
            var glyph = glyphDictionary[charKey];
            var glyphRect = new Rectangle(glyph.X, glyph.Y, glyph.Width, glyph.Height);

            chars[index] = charKey;
            glyphRects[index] = glyphRect;

            glyphOffsets[index] = new Vector2(glyph.OffsetX, glyph.OffsetY);
            glyphAdvances[index] = glyph.Advance;

            ++index;
        }

        TTF_Quit();

        return new FontData(
            fontManifest.Id,
            fontSheetImageData,
            chars,
            glyphRects,
            glyphOffsets,
            glyphAdvances);
    }
}