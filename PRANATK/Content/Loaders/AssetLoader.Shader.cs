using BinaryPack;

namespace PRANA;

internal static partial class AssetLoader
{
    public static ShaderData LoadShaderData(string assetId)
    {
        var assetManifest = Content.GetShaderManifest(assetId);

        var shaderBinPath = ContentGlobals.GetAssetBinaryPath(ContentGlobals.AssetsFolder, assetManifest);

        if (File.Exists(shaderBinPath))
        {
            using var stream = File.OpenRead(shaderBinPath);
            var data = BinaryConverter.Deserialize<ShaderData>(stream);
            return data;

        }

        throw new ApplicationException($"Asset {assetId} is not compiled.");
    }

    public static Shader LoadShader(string assetId)
    {
        var shaderData = LoadShaderData(assetId);

        var shader = Graphics.CreateShader(assetId, shaderData.VertexShader, shaderData.FragmentShader, shaderData.Samplers,
            shaderData.Params);

        return shader;
    }
}