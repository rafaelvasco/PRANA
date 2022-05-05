using System.Text.Json;

namespace PRANA;


internal static partial class AssetLoader
{
    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true
    };

    public static GameSettings LoadGameSettings()
    {
        try
        {
            if (File.Exists(ContentGlobals.GameSettingsFile))
            {
                var jsonFile = File.ReadAllText(ContentGlobals.GameSettingsFile);

                GameSettings settings = JsonSerializer.Deserialize<GameSettings>(jsonFile, serializerOptions);

                return settings;
            }
            else
            {
                var settings = GameSettings.CreateDefault();

                using var stream = File.OpenWrite(ContentGlobals.GameSettingsFile);

                JsonSerializer.Serialize(stream, settings, serializerOptions);

                return settings;
            }
        }
        catch (Exception e)
        {
            throw new ApplicationException($"Could not load game settings: {e.Message}");
        }
    }

    public static AssetsManifest LoadAssetsManifest()
    {
        try
        {
            var jsonFile = File.ReadAllText(Path.Combine(ContentGlobals.AssetsFolder, ContentGlobals.AssetsManifestFile));

            AssetsManifest manifest = JsonSerializer.Deserialize<AssetsManifest>(jsonFile);

            return manifest;
        }
        catch (Exception e)
        {
            throw new ApplicationException($"Could not load assets manifest file: {e.Message}");
        }
    }

    public static T Load<T>(string assetId) where T : Asset
    {
        if (assetId == null)
        {
            throw new ArgumentNullException(nameof(assetId));
        }

        if (typeof(T) == typeof(Texture2D))
        {
            return LoadTexture(assetId) as T;
        }

        if (typeof(T) == typeof(Shader))
        {
            return LoadShader(assetId) as T;
        }

        throw new ArgumentException("Invalid asset type: ", nameof(T));
    }

}
