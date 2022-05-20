using System.Reflection;
using System.Text;
using PRANA.Common;

namespace PRANA;

internal static partial class AssetLoader
{
    private static Texture2D LoadTexture(string assetId)
    {
        var imageData = LoadImageData(assetId);

        return LoadTexture(imageData);
    }

    private static ImageData LoadImageData(string assetId)
    {
        var assetManifest = Content.GetImageManifest(assetId);

        var imageDataBinPath = ContentProperties.GetAssetBinaryPath(ContentProperties.AssetsFolder, assetManifest);

        if (File.Exists(imageDataBinPath))
        {
            using var binStream = File.OpenRead(imageDataBinPath);
            return LoadStream<ImageData>(binStream);
        }

        throw new ApplicationException($"Asset {assetId} is not compiled.");
    }

    private static Texture2D LoadTexture(ImageData imageData)
    {
        using var pixmap = new Pixmap(imageData.Data, imageData.Width, imageData.Height);

        var texture = Graphics.CreateTexture2D(pixmap, false, TextureFilter.NearestNeighbor);
        texture.Id = imageData.Id;

        return texture;
    }

    private static Texture2D LoadTextureEmbedded(string assetId)
    {
        var path = new StringBuilder();

        path.Append(EmbeddedAssetsNamespace);
        path.Append(".Images.");
        path.Append(assetId);
        path.Append(ContentProperties.BinaryExt);

        using var fileStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(path.ToString());

        if (fileStream == null)
        {
            throw new ApplicationException($"Could not load embedded asset: {assetId}");
        }

        var imageData = LoadStream<ImageData>(fileStream);

        return LoadTexture(imageData);
    }
}
