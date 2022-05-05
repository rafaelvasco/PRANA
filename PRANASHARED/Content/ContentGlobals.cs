namespace PRANA;

internal static class ContentGlobals
{
    public const string AssetsFolder = "Content";
    public const string GameSettingsFile = "settings.json";
    public const string AssetsManifestFile = "assets.json";
    public const string BinaryExt = ".pnb";

    public static string GetAssetBinaryPath<T>(string assetsFolder, T assetManifest) where T : BaseAssetManifestInfo
    {
        if (typeof(T) == typeof(ImageManifestInfo))
        {
            return Path.Combine(assetsFolder, Path.GetDirectoryName(((assetManifest as ImageManifestInfo)!).Path)!,
                assetManifest.Id + BinaryExt);
        }

        if (typeof(T) == typeof(ShaderManifestInfo))
        {
            return Path.Combine(assetsFolder, Path.GetDirectoryName(((assetManifest as ShaderManifestInfo)!).VsPath)!,
                assetManifest.Id + BinaryExt);
        }

        throw new ArgumentException("Invalid passed type: ", nameof(T));
    }
}
