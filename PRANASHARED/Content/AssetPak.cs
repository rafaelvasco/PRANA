namespace PRANA;

public class AssetPak
{
    public string Name { get; set; }

    public Dictionary<string, ImageData> Images { get; set; }

    public Dictionary<string, ShaderData> Shaders { get; set; }

    public int TotalAssetCount { get; set; }

    public AssetPak(string name)
    {
        Name = name;
        Images = new Dictionary<string, ImageData>();
        Shaders = new Dictionary<string, ShaderData>();
        TotalAssetCount = 0;
    }
}
