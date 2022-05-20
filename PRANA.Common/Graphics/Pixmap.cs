using System.Runtime.CompilerServices;
using PRANA.Common.STB;
using static PRANA.Common.SDL.SDL;

namespace PRANA.Common;

public unsafe class Pixmap : IDisposable
{
    public IntPtr Pixels => ((SDL_Surface*)_surface)->pixels;
    
    internal IntPtr Surface => _surface;
    
    public int Width { get; }

    public int Height { get; }

    public int SizeBytes { get; }

    public int Stride => Width * 4;

    private readonly IntPtr _surface;
    
    private bool _disposedValue;


    public Pixmap(byte[] srcData, int width, int height)
    {
        Width = width;
        Height = height;
        SizeBytes = srcData.Length;
        _surface = SDL_CreateRGBSurfaceWithFormatFrom((IntPtr)Unsafe.AsPointer(ref srcData[0]), width, height, 32, width * 4,
            SDL_PIXELFORMAT_BGRA8888);
    }

    public Pixmap(int width, int height)
    {
        Width = width;
        Height = height;
        SizeBytes = width * height * 4;
        _surface = SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL_PIXELFORMAT_BGRA8888);
    }
    
    ~Pixmap()
    {
        InternalFree(disposing: false);
        throw new Exception("Pixmap Leak");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        InternalFree(disposing: true);
    }

    public void SaveToFile(string path)
    {
        using var stream = File.OpenWrite(path);

        var convertedSurface = (SDL_Surface*)SDL_ConvertSurfaceFormat(_surface, SDL_PIXELFORMAT_RGBA8888, 0);

        var image_writer = new ImageWriter();
        image_writer.WritePng((void*)convertedSurface->pixels, Width, Height, ColorComponents.RedGreenBlueAlpha, stream);
    }

    private void InternalFree(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            SDL_FreeSurface(_surface);

            _disposedValue = true;
        }
    }
}