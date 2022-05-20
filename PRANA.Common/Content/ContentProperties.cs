using System.Text;

namespace PRANA.Common;

public static class ContentProperties
{
    public const string AssetsFolder = "Content";
    public const string GameSettingsFile = "settings.json";
    public const string AssetsManifestFile = "assets.json";
    public const string BinaryExt = ".pnb";

    public static Dictionary<GraphicsBackend, string> ShaderAppendStrings =
        new()
        {
            {
                GraphicsBackend.Direct3D11, "_D3D"
            },
            {
                GraphicsBackend.Direct3D12, "_D3D12"
            },
            {
                GraphicsBackend.OpenGL, "_GL"
            },
            {
                GraphicsBackend.Metal, "_MT"
            },
            {
                GraphicsBackend.Vulkan, "_VK"
            },
        };


    public static string GetAssetBinaryPath<T>(string assetsFolder, T assetManifest, string fileNameAppend = null) where T : BaseAssetManifestInfo
    {
        var fileName = new StringBuilder(assetManifest.Id);

        if (fileNameAppend != null)
        {
            fileName.Append($"{fileNameAppend}");
        }

        fileName.Append(BinaryExt);

        if (typeof(T) == typeof(ImageManifestInfo))
        {
            return Path.Combine(assetsFolder, Path.GetDirectoryName(((assetManifest as ImageManifestInfo)!).Path)!,
                fileName.ToString());
        }

        if (typeof(T) == typeof(ShaderManifestInfo))
        {
            return Path.Combine(assetsFolder, Path.GetDirectoryName(((assetManifest as ShaderManifestInfo)!).VsPath)!,
                fileName.ToString());
        }

        if (typeof(T) == typeof(FontManifestInfo))
        {
            return Path.Combine(assetsFolder, Path.GetDirectoryName(((assetManifest as FontManifestInfo)!).Path)!,
                fileName.ToString());
        }

        throw new ArgumentException("Invalid passed type: ", nameof(T));
    }
}
