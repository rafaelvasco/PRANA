using BinaryPack;

namespace PRANA;

internal static partial class AssetLoader
{
    public static ImageData LoadImageData(string assetId)
    {
        var assetManifest = Content.GetImageManifest(assetId);

        var imageDataBinPath = ContentGlobals.GetAssetBinaryPath(ContentGlobals.AssetsFolder, assetManifest);

        if (File.Exists(imageDataBinPath))
        {
            using var binStream = File.OpenRead(imageDataBinPath);
            var data = BinaryConverter.Deserialize<ImageData>(binStream);
            return data;
        }

        throw new ApplicationException($"Asset {assetId} is not compiled.");
    }

    public static Texture2D LoadTexture(string assetId)
    {
        var imageData = LoadImageData(assetId);

        var pixmap = new Pixmap(imageData.Data, imageData.Width, imageData.Height);

        return Graphics.CreateTexture2D(pixmap, false, TextureFilter.NearestNeighbor);
    }
}
