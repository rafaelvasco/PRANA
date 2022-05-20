using System.Reflection;
using System.Text;
using BinaryPack;
using PRANA.Common;

namespace PRANA;

internal static partial class AssetLoader
{
    private static Shader LoadShader(string assetId)
    {
        var shaderData = LoadShaderData(assetId);

        return LoadShader(shaderData);
    }

    private static ShaderData LoadShaderData(string assetId)
    {
        var assetManifest = Content.GetShaderManifest(assetId);

        var shaderBinPath = ContentProperties.GetAssetBinaryPath(ContentProperties.AssetsFolder, assetManifest);

        if (File.Exists(shaderBinPath))
        {
            using var stream = File.OpenRead(shaderBinPath);
            var data = BinaryConverter.Deserialize<ShaderData>(stream);
            return data;

        }

        throw new ApplicationException($"Asset {assetId} is not compiled.");
    }

    private static Shader LoadShader(ShaderData shaderData)
    {
        var shader = Graphics.CreateShader(shaderData.VertexShader, shaderData.FragmentShader, shaderData.Samplers,
            shaderData.Params);

        shader.Id = shaderData.Id;

        return shader;
    }

    private static Shader LoadShaderEmbedded(string assetId)
    {
        var path = new StringBuilder();

        var fileName = new StringBuilder(assetId);

        fileName.Append(ContentProperties.ShaderAppendStrings[Graphics.GraphicsBackend]);

        path.Append(EmbeddedAssetsNamespace);
        path.Append(".Shaders.");
        path.Append(assetId);
        path.Append('.');
        path.Append(fileName);
        path.Append(ContentProperties.BinaryExt);

        using var fileStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(path.ToString());

        if (fileStream == null)
        {
            throw new ApplicationException($"Could not load embedded asset: {assetId}");
        }

        var shaderData = LoadStream<ShaderData>(fileStream);

        return LoadShader(shaderData);
    }
}