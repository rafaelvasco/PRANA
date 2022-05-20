using BinaryPack;
using PRANA.Common;
using PRANA.Common.STB;

namespace PRANA;

internal static partial class AssetBuilder
{
    private static void BuildAndExportImage(ImageManifestInfo imageManifest, string assetsFolder)
    {
        Console.WriteLine($"Building image {imageManifest.Id}...");

        var imageData = BuildImage(imageManifest, assetsFolder);

        var assetBinPath = ContentProperties.GetAssetBinaryPath(assetsFolder, imageManifest);

        using var stream = File.OpenWrite(assetBinPath);

        BinaryConverter.Serialize(imageData, stream);
    }

    private static ImageData BuildImage(ImageManifestInfo imageManifest, string assetsFolder)
    {
        using var stream = File.OpenRead(Path.Combine(assetsFolder, imageManifest.Path));

        var stbImage = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        var data = new ImageData(
            imageManifest.Id,
            stbImage.Data,
            stbImage.Width,
            stbImage.Height
        );

        return data;
    }
}