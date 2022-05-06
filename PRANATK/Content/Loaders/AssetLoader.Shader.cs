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

    public static Shader LoadShader(Stream stream, string shaderId)
    {
        var shaderData = LoadStream<ShaderData>(stream);

        return LoadShader(shaderData);
    }

    public static Shader LoadShader(string assetId)
    {
        var shaderData = LoadShaderData(assetId);

        return LoadShader(shaderData);
    }

    public static Shader LoadShader(ShaderData shaderData)
    {
        var shader = Graphics.CreateShader(shaderData.VertexShader, shaderData.FragmentShader, shaderData.Samplers,
            shaderData.Params);

        return shader;
    }
}