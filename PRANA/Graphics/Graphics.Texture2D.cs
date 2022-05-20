using PRANA.Common;
using PRANA.Foundation.BGFX;

namespace PRANA;

public static unsafe partial class Graphics
{
    internal static Texture2D CreateTexture2D(Pixmap pixmap, bool tiled, TextureFilter filter)
    {
        var samplerFlags = CalculateSamplerFlags(tiled, filter);

        var handle = Bgfx.CreateTexture2D((ushort)pixmap.Width, (ushort)pixmap.Height, false, 1, Bgfx.TextureFormat.BGRA8, (ulong)samplerFlags, Bgfx.AllocGraphicsMemoryBuffer(pixmap.Pixels, pixmap.SizeBytes));

        var texture = new Texture2D(handle, pixmap.Width, pixmap.Height, samplerFlags);

        return texture;
    }

    internal static Texture2D CreateTexture2D(IntPtr pixels, int width, int height, bool tiled, TextureFilter filter)
    {
        var samplerFlags = CalculateSamplerFlags(tiled, filter);

        var handle = Bgfx.CreateTexture2D((ushort)width, (ushort)height, false, 1, Bgfx.TextureFormat.BGRA8, (ulong)samplerFlags, Bgfx.AllocGraphicsMemoryBuffer(pixels, width * height * 4));

        var texture = new Texture2D(handle, width, height, samplerFlags);

        return texture;
    }
    
    internal static Texture2D CreateTexture2D(int width, int height, bool tiled, TextureFilter filter)
    {
        var samplerFlags = CalculateSamplerFlags(tiled, filter);

        var handle = Bgfx.CreateTexture2D((ushort)width, (ushort)height, false, 1, Bgfx.TextureFormat.BGRA8, (ulong)samplerFlags, null);

        var texture = new Texture2D(handle, width, height, samplerFlags);

        return texture;
    }

    internal static void UpdateTexture2D(Texture2D texture, Pixmap pixmap, int targetX = 0, int targetY = 0, int targetW = 0, int targetH = 0)
    {
        var data = Bgfx.GetMemoryBufferReference<byte>(pixmap.Pixels, pixmap.SizeBytes);

        if (targetW == 0)
        {
            targetW = texture.Width;
        }

        if (targetH == 0)
        {
            targetH = texture.Height;
        }

        Bgfx.UpdateTexture2D(texture.Handle, 0, 0, (ushort)targetX, (ushort)targetY, (ushort)targetW, (ushort)targetH, data, (ushort)pixmap.Stride);
    }
    
    internal static void UpdateTexture2D(Texture2D texture, IntPtr pixelData, int bytesLength, int targetX = 0, int targetY = 0, int targetW = 0, int targetH = 0)
    {
        var data = Bgfx.AllocGraphicsMemoryBuffer(pixelData, bytesLength);

        if (targetW == 0)
        {
            targetW = texture.Width;
        }

        if (targetH == 0)
        {
            targetH = texture.Height;
        }

        Bgfx.UpdateTexture2D(texture.Handle, 0, 0, (ushort)targetX, (ushort)targetY, (ushort)targetW, (ushort)targetH, data, ushort.MaxValue);
    }

    internal static void DisposeTexture2D(Texture2D texture2D)
    {
        Bgfx.DestroyTexture(texture2D.Handle);
    }

    internal static Bgfx.SamplerFlags CalculateSamplerFlags(bool tiled, TextureFilter filter)
    {
        var samplerFlags = Bgfx.SamplerFlags.None;

        if (!tiled) samplerFlags = Bgfx.SamplerFlags.UClamp | Bgfx.SamplerFlags.VClamp;

        switch (filter)
        {
            case TextureFilter.NearestNeighbor:
                samplerFlags |= Bgfx.SamplerFlags.Point;
                break;
        }

        return samplerFlags;
    }
}