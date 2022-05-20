using System.Runtime.CompilerServices;
using PRANA.Common;
using PRANA.Foundation.BGFX;

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
        get => !SamplerFlags.HasFlag(Bgfx.SamplerFlags.UClamp) && !SamplerFlags.HasFlag(Bgfx.SamplerFlags.VClamp);
        set => SamplerFlags = Graphics.CalculateSamplerFlags(value, Filter);
    }

    public TextureFilter Filter
    {
        get => (SamplerFlags.HasFlag(Bgfx.SamplerFlags.Point)) ? TextureFilter.NearestNeighbor : TextureFilter.Linear;
        set => SamplerFlags = Graphics.CalculateSamplerFlags(Tiled, value);
    }

    public static Texture2D Create(string id, Pixmap pixmap, bool tiled = false, TextureFilter filter = TextureFilter.NearestNeighbor)
    {
        var texture = Graphics.CreateTexture2D(pixmap, tiled, filter);
        
        Content.RegisterAsset(id, texture);

        return texture;
    }

    public static Texture2D Create(string id, int width, int height, bool tiled = false, TextureFilter filter = TextureFilter.NearestNeighbor)
    {
        var texture = Graphics.CreateTexture2D(width, height, tiled, filter);
        
        Content.RegisterAsset(id, texture);

        return texture;
    }

    public static Texture2D Create(string id, IntPtr pixels, int width, int height, bool tiled = false, TextureFilter filter = TextureFilter.NearestNeighbor)
    {
        var texture = Graphics.CreateTexture2D(pixels, width, height, tiled, filter);
        
        Content.RegisterAsset(id, texture);

        return texture;
    }

    internal Texture2D(Bgfx.TextureHandle handle, int width, int height, Bgfx.SamplerFlags flags)
    {
        Handle = handle;
        Width = width;
        Height = height;
        SamplerFlags = flags;
    }

    public void SetPixels(Pixmap pixmap, Rectangle target = default)
    {
        Graphics.UpdateTexture2D(this, pixmap, target.X, target.Y, target.Width, target.Height);
    }

    public void SetPixels(IntPtr pixelDataPtr, int byteLength)
    {
        Graphics.UpdateTexture2D(this, pixelDataPtr, byteLength);
    }

    public unsafe void SetPixels(byte[] pixels)
    {
        Graphics.UpdateTexture2D(this, (IntPtr)Unsafe.AsPointer(ref pixels[0]), pixels.Length);
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