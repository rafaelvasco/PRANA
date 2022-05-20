using System.Text.Json;
using PRANA.Common;

namespace PRANA;

internal static partial class AssetBuilder
{
    private static AssetsManifest LoadAssetsManifest(string rootPath)
    {
        try
        {
            var jsonFile = File.ReadAllText(Path.Combine(rootPath, ContentProperties.AssetsManifestFile));

            AssetsManifest manifest = JsonSerializer.Deserialize<AssetsManifest>(jsonFile);

            return manifest;
        }
        catch (Exception e)
        {
            throw new ApplicationException($"Could not load assets manifest file: {e.Message}");
        }
    }

    public static void BuildAssets(string assetsFolder)
    {
        Console.WriteLine($"Hey, I'll be building all assets on folder {assetsFolder} :");

        var manifest = LoadAssetsManifest(assetsFolder);

        if (manifest.Images != null)
        {
            foreach (var (_, imageManifest) in manifest.Images)
            {
                BuildAndExportImage(imageManifest, assetsFolder);
            }
        }

        if (manifest.Shaders != null)
        {
            foreach (var (_, shaderManifest) in manifest.Shaders)
            {
                BuildAndExportShader(shaderManifest, assetsFolder, GraphicsBackend.Direct3D11);
                BuildAndExportShader(shaderManifest, assetsFolder, GraphicsBackend.OpenGL);
            }
        }

        if (manifest.Fonts != null)
        {
            foreach (var (_, fontManifest) in manifest.Fonts)
            {
                BuildAndExportFont(fontManifest, assetsFolder);
            }
        }

        Console.WriteLine("Done. Bye!");
    }

}