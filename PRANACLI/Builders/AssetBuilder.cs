using System.Text.Json;
using BinaryPack;

namespace PRANA;

internal static partial class AssetBuilder
{
    private static AssetsManifest LoadAssetsManifest(string rootPath)
    {
        try
        {
            var jsonFile = File.ReadAllText(Path.Combine(rootPath, ContentGlobals.AssetsManifestFile));

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
            foreach (var (imageId, imageManifest) in manifest.Images)
            {
                Console.WriteLine($"Building image {imageId}...");

                var imageData = BuildImage(imageManifest, assetsFolder);

                var assetBinPath = ContentGlobals.GetAssetBinaryPath(assetsFolder, imageManifest);

                using var stream = File.OpenWrite(assetBinPath);

                BinaryConverter.Serialize(imageData, stream);

                Console.WriteLine($"Image {imageId} built successfuly.");
            }
        }

        if (manifest.Shaders != null)
        {
            foreach (var (shaderId, shaderManifest) in manifest.Shaders)
            {
                Console.WriteLine($"Building shader {shaderId}...");

                var shaderData = BuildShader(shaderManifest, assetsFolder);

                var assetBinPath = ContentGlobals.GetAssetBinaryPath(assetsFolder, shaderManifest);

                using var stream = File.OpenWrite(assetBinPath);

                BinaryConverter.Serialize(shaderData, stream);

                Console.WriteLine($"Shader {shaderId} built successfuly.");
            }
        }

        Console.WriteLine("Done. Bye!");
    }

}