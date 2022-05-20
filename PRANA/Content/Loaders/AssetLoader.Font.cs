using System.Reflection;
using System.Text;
using PRANA.Common;

namespace PRANA;

internal static partial class AssetLoader
{
    private static Font LoadFont(string assetId)
    {
        var fontData = LoadFontData(assetId);

        return LoadFont(fontData);
    }

    private static FontData LoadFontData(string assetId)
    {
        var assetManifest = Content.GetFontManifest(assetId);

        var fontDataBinPath = ContentProperties.GetAssetBinaryPath(ContentProperties.AssetsFolder, assetManifest);

        if (File.Exists(fontDataBinPath))
        {
            using var binStream = File.OpenRead(fontDataBinPath);
            return LoadStream<FontData>(binStream);
        }

        throw new ApplicationException($"Asset {assetId} is not compiled.");
    }

    private static Font LoadFont(FontData fontData)
    {
        using var fontSheet = new Pixmap(fontData.FontSheet.Data, fontData.FontSheet.Width, fontData.FontSheet.Height);

        var texture = Graphics.CreateTexture2D(fontSheet, false, TextureFilter.NearestNeighbor);

        var font = new Font(texture, fontData);

        font.Id = fontData.Id;
        font.Texture.Id = fontData.FontSheet.Id;

        return font;
    }

    private static Font LoadFontEmbedded(string assetId)
    {
        var path = new StringBuilder();

        path.Append(EmbeddedAssetsNamespace);
        path.Append(".Fonts.");
        path.Append(assetId);
        path.Append(ContentProperties.BinaryExt);

        using var fileStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(path.ToString());

        if (fileStream == null)
        {
            throw new ApplicationException($"Could not load embedded asset: {assetId}");
        }

        var fontData = LoadStream<FontData>(fileStream);

        return LoadFont(fontData);
    }
}