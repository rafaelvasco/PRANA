using BinaryPack;
using PRANA.Common;

namespace PRANA;


internal static partial class AssetBuilder
{
    private static void BuildAndExportShader(ShaderManifestInfo shaderManifest, string assetsFolder, GraphicsBackend graphicsBackend)
    {
        Console.WriteLine($"Building shader {shaderManifest.Id} for backend {graphicsBackend}...");

        var shaderData = BuildShader(shaderManifest, assetsFolder, graphicsBackend);

        var assetBinPath = ContentProperties.GetAssetBinaryPath(assetsFolder, shaderManifest,  fileNameAppend: ContentProperties.ShaderAppendStrings[graphicsBackend]);

        using var stream = File.OpenWrite(assetBinPath);

        BinaryConverter.Serialize(shaderData, stream);

        Console.WriteLine($"Shader {shaderManifest.Id} for backend {graphicsBackend} built successfuly.");
    }

    private static ShaderData BuildShader(ShaderManifestInfo shaderManifest, string assetsFolder, GraphicsBackend graphicsBackend)
    {
        var vsFullPath = Path.Combine(assetsFolder, shaderManifest.VsPath);

        var fsFullPath = Path.Combine(assetsFolder , shaderManifest.FsPath);

        var compileResult = ShaderCompiler.Compile(vsFullPath, fsFullPath, graphicsBackend);

        var data = new ShaderData()
        {
            FragmentShader = compileResult.FsBytes,
            VertexShader = compileResult.VsBytes,
            Samplers = compileResult.Samplers,
            Params = compileResult.Params,
            Id = shaderManifest.Id,
            Datetime = DateTime.UtcNow.ToString("O")
        };

        return data;
    }
}