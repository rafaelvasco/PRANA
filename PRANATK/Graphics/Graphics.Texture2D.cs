using PRANA.Foundation;

namespace PRANA;

public static unsafe partial class Graphics
{
    internal static Texture2D CreateTexture2D(Pixmap pixmap, bool tiled, TextureFilter filter)
    {
        var samplerFlags = CalculateSamplerFlags(tiled, filter);

        var handle = Bgfx.CreateTexture2D((ushort)pixmap.Width, (ushort)pixmap.Height, false, 0, Bgfx.TextureFormat.BGRA8, (ulong)samplerFlags, Bgfx.GetMemoryBufferReference<byte>(pixmap.Data));

        var texture = new Texture2D(handle, pixmap, samplerFlags);

        return texture;
    }

    internal static void UpdateTexture2D(Texture2D texture, Pixmap pixmap, int targetX = 0, int targetY = 0, int targetW = 0, int targetH = 0)
    {
        var data = Bgfx.GetMemoryBufferReference<byte>(pixmap.Data);

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