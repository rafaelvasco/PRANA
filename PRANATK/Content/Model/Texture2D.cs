using PRANA.Foundation;

namespace PRANA;

public enum TextureFilter
{
    NearestNeighbor,
    Linear
}

public class Texture2D : Asset, IEquatable<Texture2D>
{
    internal Bgfx.TextureHandle Handle { get; }

    internal Bgfx.SamplerFlags SamplerFlags { get; private set; }

    public int Width { get; }

    public int Height { get; }

    public const int PixelSizeInBytes = sizeof(byte) * 4;

    public bool Tiled
    {
        get => (SamplerFlags & Bgfx.SamplerFlags.UClamp) == 0 && (SamplerFlags & Bgfx.SamplerFlags.VClamp) == 0;
        set => SamplerFlags = Graphics.CalculateSamplerFlags(value, Filter);
    }

    public TextureFilter Filter
    {
        get => (SamplerFlags & Bgfx.SamplerFlags.Point) == 0 ? TextureFilter.NearestNeighbor : TextureFilter.Linear;
        set => SamplerFlags = Graphics.CalculateSamplerFlags(Tiled, value);
    }

    public static Texture2D Create(string id, int width, int height, bool tiled = false, TextureFilter filter = TextureFilter.NearestNeighbor)
    {
        var pixmap = new Pixmap(width, height);

        return Create(id, pixmap, tiled, filter);
    }

    public static Texture2D Create(string id, Pixmap pixmap, bool tiled = false,
        TextureFilter filter = TextureFilter.NearestNeighbor)
    {
        var texture = Graphics.CreateTexture2D(pixmap, tiled, filter);

        Content.RegisterAsset(id, texture);

        return texture;
    }

    internal Texture2D(Bgfx.TextureHandle handle, Pixmap pixmap, Bgfx.SamplerFlags flags)
    {
        Handle = handle;
        Width = pixmap.Width;
        Height = pixmap.Height;
        SamplerFlags = flags;
    }

    public void SetPixels(Pixmap pixmap, Rectangle target = default)
    {
        Graphics.UpdateTexture2D(this, pixmap, target.X, target.Y, target.Width, target.Height);
    }

    protected override void FreeUnmanaged()
    {
        if (Handle.Valid)
        {
            Graphics.DisposeTexture2D(this);
        }
    }

    public bool Equals(Texture2D other)
    {
        return other != null && Handle.idx == other.Handle.idx;
    }

    public override bool Equals(object obj)
    {
        return obj != null && Equals(obj as Texture2D);
    }

    public override int GetHashCode()
    {
        return Handle.idx.GetHashCode();
    }
}