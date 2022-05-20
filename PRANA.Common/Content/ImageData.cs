namespace PRANA.Common;

public readonly struct ImageData
{
    public string Id { get; init; }

    public byte[] Data { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }

    public ImageData(
        string id,
        byte[] data,
        int width,
        int height
    )
    {
        Id = id;
        Data = data;
        Width = width;
        Height = height;
    }
}

