using System.Text.Json;
using BinaryPack;
using PRANA.Common;

namespace PRANA;


internal static partial class AssetLoader
{
    private const string EmbeddedAssetsNamespace = "PRANA.Content.BaseAssets";

    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true
    };

    public static GameSettings LoadGameSettings()
    {
        try
        {
            if (File.Exists(ContentProperties.GameSettingsFile))
            {
                var jsonFile = File.ReadAllText(ContentProperties.GameSettingsFile);

                GameSettings settings = JsonSerializer.Deserialize<GameSettings>(jsonFile, serializerOptions);

                return settings;
            }
            else
            {
                var settings = GameSettings.CreateDefault();

                using var stream = File.OpenWrite(ContentProperties.GameSettingsFile);

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
            var jsonFile = File.ReadAllText(Path.Combine(ContentProperties.AssetsFolder, ContentProperties.AssetsManifestFile));

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
        T asset;

        if (assetId == null)
        {
            throw new ArgumentNullException(nameof(assetId));
        }

        if (typeof(T) == typeof(Texture2D))
        {
            asset = LoadTexture(assetId) as T;
        }
        else if (typeof(T) == typeof(Shader))
        {
            asset = LoadShader(assetId) as T;
        }
        else if (typeof(T) == typeof(Font))
        {
            asset = LoadFont(assetId) as T;
        }
        else
        {
            throw new ArgumentException("Invalid asset type: ", nameof(T));
        }

        Content.RegisterAsset(assetId, asset);

        return asset;
    }

    private static T LoadStream<T>(Stream stream) where T : struct
    {
        var data = BinaryConverter.Deserialize<T>(stream);

        return data;
    }

    public static T LoadEmbedded<T>(string assetId) where T : Asset
    {
        if (assetId == null)
        {
            throw new ArgumentNullException(nameof(assetId));
        }

        T asset;

        if (typeof(T) == typeof(Texture2D))
        {
            asset = LoadTextureEmbedded(assetId) as T;
        }
        else if (typeof(T) == typeof(Shader))
        {
            asset = LoadShaderEmbedded(assetId) as T;
        }
        else if (typeof(T) == typeof(Font))
        {
            asset = LoadFontEmbedded(assetId) as T;
        }
        else
        {
            throw new ArgumentException("Invalid asset type: ", nameof(T));
        }

        Content.RegisterAsset(assetId, asset);

        return asset;

    }
}
