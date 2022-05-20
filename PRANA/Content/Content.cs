using PRANA.Common;

namespace PRANA;

public static class Content
{
    private static Dictionary<string, Asset> _assets;

    private static AssetsManifest Manifest { get; set; }

    internal static ImageManifestInfo GetImageManifest(string assetId)
    {
        if (Manifest.Images.TryGetValue(assetId, out var imageManifest))
        {
            return imageManifest;
        }

        throw new ApplicationException($"Could not fetch manifest for asset {assetId}");
    }

    internal static ShaderManifestInfo GetShaderManifest(string assetId)
    {
        if (Manifest.Shaders.TryGetValue(assetId, out var shaderManifest))
        {
            return shaderManifest;
        }

        throw new ApplicationException($"Could not fetch manifest for asset {assetId}");
    }

    internal static FontManifestInfo GetFontManifest(string assetId)
    {
        if (Manifest.Fonts.TryGetValue(assetId, out var fontManifest))
        {
            return fontManifest;
        }

        throw new ApplicationException($"Could not fetch manifest for asset {assetId}");
    }

    internal static void Init()
    {
        _assets = new Dictionary<string, Asset>();

        Manifest = AssetLoader.
            LoadAssetsManifest();
    }

    public static T Get<T>(string assetId) where T : Asset
    {
        if (_assets == null)
        {
            throw new ApplicationException("Trying to call Get before Content manager is initialized.");
        }

        if (_assets.TryGetValue(assetId, out var foundAsset))
        {
            if (foundAsset is T value)
            {
                return value;
            }
        }

        var asset = AssetLoader.Load<T>(assetId);

        return asset;
    }

    internal static void RegisterAsset(string id, Asset asset)
    {
        asset.Id = id;
        _assets.Add(id, asset);
    }

    public static void Free(Asset asset)
    {
        if (_assets == null)
        {
            throw new ApplicationException("Trying to free resource before Content Manager is initialized.");
        }

        if (_assets.TryGetValue(asset.Id, out _))
        {
            asset.Dispose();
            _assets.Remove(asset.Id);
        }
    }

    internal static GameSettings GetGameSettings()
    {
        return AssetLoader.LoadGameSettings();
    }

    internal static void Free()
    {
        Console.WriteLine("Freeing Content...");

        if (_assets == null)
        {
            throw new ApplicationException("Trying to free resources before Content Manager is initialized.");
        }

        foreach (var (_, asset) in _assets)
        {
            Console.WriteLine($"Freeing {asset.Id} of type {asset.GetType()}");

            asset.Dispose();
        }

        _assets.Clear();
    }
}
