using PRANA.Foundation;

namespace PRANA;

internal static partial class AssetBuilder
{
    public static ImageData BuildImage(ImageManifestInfo imageManifest, string assetsFolder)
    {
        using var stream = File.OpenRead(Path.Combine(assetsFolder, imageManifest.Path));

        var stbImage = Stb.ImageResult.FromStream(stream, Stb.ColorComponents.RedGreenBlueAlpha);

        var data = new ImageData()
        {
            Data = stbImage.Data,
            Id = imageManifest.Path,
            Width = stbImage.Width,
            Height = stbImage.Height,
            Datetime = DateTime.UtcNow.ToString("O")
        };

        return data;
    }
}