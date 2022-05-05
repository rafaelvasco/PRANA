using System.Text.Json.Serialization;

namespace PRANA;

internal abstract class BaseAssetManifestInfo
{
    [JsonPropertyName("id")]
    public string Id { get;set;}
}

internal class ImageManifestInfo : BaseAssetManifestInfo
{
    [JsonPropertyName("path")]
    public string Path { get; set; }

}

internal class ShaderManifestInfo : BaseAssetManifestInfo
{
    [JsonPropertyName("vs_path")]
    public string VsPath { get;set; }

    [JsonPropertyName("fs_path")]
    public string FsPath { get;set; }
}

internal class AssetsManifest
{
    [JsonPropertyName("images")]
    public Dictionary<string, ImageManifestInfo> Images { get; set;}

    [JsonPropertyName("shaders")]
    public Dictionary<string, ShaderManifestInfo> Shaders { get;set; }
}
