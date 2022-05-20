using System.Text.Json.Serialization;

namespace PRANA.Common;

public abstract class BaseAssetManifestInfo
{
    [JsonPropertyName("id")]
    public string Id { get;set;}
}

public class ImageManifestInfo : BaseAssetManifestInfo
{
    [JsonPropertyName("path")]
    public string Path { get; set; }

}

public class ShaderManifestInfo : BaseAssetManifestInfo
{
    [JsonPropertyName("vs_path")]
    public string VsPath { get;set; }

    [JsonPropertyName("fs_path")]
    public string FsPath { get;set; }
}

public class FontManifestInfo : BaseAssetManifestInfo
{
    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("char_ranges")]
    public string[] CharRanges { get; set; }

    [JsonPropertyName(("filtered"))]
    public bool Filtered { get; set; }
}

public class AssetsManifest
{
    [JsonPropertyName("images")]
    public Dictionary<string, ImageManifestInfo> Images { get; set;}

    [JsonPropertyName("shaders")]
    public Dictionary<string, ShaderManifestInfo> Shaders { get;set; }

    [JsonPropertyName("fonts")]
    public Dictionary<string, FontManifestInfo> Fonts { get; set; }
}
