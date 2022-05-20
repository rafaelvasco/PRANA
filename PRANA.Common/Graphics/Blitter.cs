using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static PRANA.Common.SDL.SDL;

namespace PRANA.Common;

public static unsafe class Blitter
{
    private const int AuxBufferSize = 1024 * 1024 * 4;
    private const int AuxBufferStackSize = 2;
    private static IntPtr _pixels;
    private static int _pixelsSizeInBytes;
    private static Pixmap _currentPixmap;
    private static readonly byte[][] AuxBuffers = new byte[AuxBufferStackSize][];
    private static int _auxBuffersIdx;
    private static int _surfaceW;
    private static int _surfaceH;
    private static bool _ready;
    private static Rectangle _clipRect;
    private static Color _mDrawColor = Color.White;

    public static void Begin(Pixmap pixmap)
    {
        if (_ready)
        {
            throw new Exception("Blitter: Dangling Begin Call");
        }

        _currentPixmap = pixmap;

        _pixelsSizeInBytes = pixmap.SizeBytes;

        if (SDL_MUSTLOCK(pixmap.Surface))
        {
            if (SDL_LockSurface(pixmap.Surface) != 0)
            {
                throw new ApplicationException("Could not lock Surface");
            }
        }

        SetDrawState(pixmap.Pixels, pixmap.Width, pixmap.Height);
    }

    public static void Begin(byte[] pixels, int surfaceWidth, int surfaceHeight)
    {
        if (_ready)
        {
            throw new Exception("Blitter: Dangling Begin Call");
        }

        _pixelsSizeInBytes = pixels.Length;

        SetDrawState((IntPtr)Unsafe.AsPointer(ref pixels[0]), surfaceWidth, surfaceHeight);
    }


    private static void SetDrawState(IntPtr pixels, int surfaceWidth, int surfaceHeight)
    {
        _pixels = pixels;
        _surfaceW = surfaceWidth;
        _surfaceH = surfaceHeight;
        _mDrawColor = Color.White;
        _clipRect = new Rectangle(0, 0, _surfaceW, _surfaceH);
        _ready = true;
    }
    public static void End()
    {
        _pixels = IntPtr.Zero;
        _surfaceW = 0;
        _surfaceH = 0;
        _ready = false;
        _clipRect = Rectangle.Empty;

        if (_currentPixmap != null)
        {
            if (SDL_LockSurface(_currentPixmap.Surface) != 0)
            {
                SDL_UnlockSurface(_currentPixmap.Surface);
            }
            _currentPixmap = null;
        }
    }

    public static void SetColor(Color color)
    {
        _mDrawColor = color;
    }

    public static void Clip(Rectangle rect)
    {
        _clipRect = rect;
        if (_clipRect.IsEmpty)
        {
            _clipRect = new Rectangle(0, 0, _surfaceW, _surfaceH);
        }
        else if (!(new Rectangle(0, 0, _surfaceW, _surfaceH)).Contains(_clipRect))
        {
            _clipRect = new Rectangle(0, 0, _surfaceW, _surfaceH);
        }
    }

    public static void Clip(int x = 0, int y = 0, int w = 0, int h = 0)
    {
        Clip(new Rectangle(x, y, w, h));
    }

    public static void PixelSet(int x, int y)
    {
        if (!_ready)
        {
            return;
        }

        if (!_clipRect.Contains(x, y))
        {
            return;
        }

        ref var col = ref _mDrawColor;

        byte r = col.R;
        byte g = col.G;
        byte b = col.B;
        byte a = col.A;

        byte* ptr = (byte*)_pixels;
        
        byte* ptr_idx = ptr + (x + y * _surfaceW) * 4;

        *(ptr_idx) = b;
        *(ptr_idx + 1) = g;
        *(ptr_idx + 2) = r;
        *(ptr_idx + 3) = a;
    }

    public static Color? PixelGet(int x, int y)
    {
        if (!_ready)
        {
            return null;
        }
        
        byte* ptr = (byte*)_pixels;

        byte* ptr_idx = ptr + (x + y * _surfaceW) * 4;

        return new Color(
            *(ptr_idx + 2),
            *(ptr_idx + 1),
            *(ptr_idx),
            *(ptr_idx + 3)
        );
    }

    public static void Clear()
    {
        if (!_ready)
        {
            return;
        }

        byte* ptr = (byte*)_pixels;

        var len = _pixelsSizeInBytes - 4;
        for (int i = 0; i <= len; i += 4)
        {
            *(ptr + i) = 0;
            *(ptr + i + 1) = 0;
            *(ptr + i + 2) = 0;
            *(ptr + i + 3) = 0;
        }
    }

    public static void Fill()
    {
        if (!_ready)
        {
            return;
        }

        ref var col = ref _mDrawColor;

        byte r = col.R;
        byte g = col.G;
        byte b = col.B;
        byte a = col.A;

        byte* ptr = (byte*)_pixels;

        var len = _pixelsSizeInBytes - 4;
        for (int i = 0; i <= len; i += 4)
        {
            *(ptr + i) = b;
            *(ptr + i + 1) = g;
            *(ptr + i + 2) = r;
            *(ptr + i + 3) = a;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HLine(int sx, int ex, int y)
    {
        if (!_ready)
        {
            return;
        }

        var minX = _clipRect.Left;
        var maxX = _clipRect.Right;

        if (y < _clipRect.Top || y > _clipRect.Bottom)
        {
            return;
        }

        if (sx < minX && ex < minX)
        {
            return;
        }

        if (sx > maxX && ex > maxX)
        {
            return;
        }

        if (ex < sx)
        {
            (sx, ex) = (ex, sx);
        }

        ref var col = ref _mDrawColor;

        byte r = col.R;
        byte g = col.G;
        byte b = col.B;
        byte a = col.A;

        int sw = _surfaceW;
        
        byte* ptr = (byte*)_pixels;

        for (int x = sx; x < ex; ++x)
        {
            byte* ptr_idx = ptr + (x + y * sw) * 4;

            *(ptr_idx) = b;
            *(ptr_idx + 1) = g;
            *(ptr_idx + 2) = r;
            *(ptr_idx + 3) = a;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void VLine(int sy, int ey, int x)
    {
        if (!_ready)
        {
            return;
        }

        if (x < _clipRect.Left || x > _clipRect.Right)
        {
            return;
        }

        var minY = _clipRect.Top;
        var maxY = _clipRect.Bottom;
        if (sy < minY && ey < minY) return;
        if (sy > maxY && ey > maxY) return;

        if (ey < sy)
        {
            (sy, ey) = (ey, sy);
        }

        ref var col = ref _mDrawColor;

        byte r = col.R;
        byte g = col.G;
        byte b = col.B;
        byte a = col.A;

        int sw = _surfaceW;
        
        byte* ptr = (byte*)_pixels;

        for (int y = sy; y < ey; ++y)
        {
            if (y < _clipRect.Top || y > _clipRect.Bottom)
            {
                continue;
            }

            byte* ptr_idx = ptr + (x + y * sw) * 4;

            *(ptr_idx) = b;
            *(ptr_idx + 1) = g;
            *(ptr_idx + 2) = r;
            *(ptr_idx + 3) = a;
        }
    }

    public static void FillRect(int x, int y, int w, int h)
    {
        if (!_ready)
        {
            return;
        }

        int sw = _surfaceW;

        ref var col = ref _mDrawColor;

        byte r = col.R;
        byte g = col.G;
        byte b = col.B;
        byte a = col.A;

        int left = Math.Max(x, _clipRect.Left);
        int right = Math.Min(x + w, _clipRect.Right);
        int top = Math.Max(y, _clipRect.Top);
        int bottom = Math.Min(y + h, _clipRect.Bottom);
        
        byte* ptr = (byte*)_pixels;

        for (int px = left; px < right; ++px)
        {
            for (int py = top; py < bottom; ++py)
            {
                byte* ptr_idx = ptr + (px + py * sw) * 4;

                *(ptr_idx) = b;
                *(ptr_idx + 1) = g;
                *(ptr_idx + 2) = r;
                *(ptr_idx + 3) = a;
            }
        }
    }

    public static void DrawRect(int x, int y, int w, int h, int lineSize = 1)
    {
        if (!_ready)
        {
            return;
        }

        if (lineSize < 1)
        {
            lineSize = 1;
        }

        if (lineSize == 1)
        {
            HLine(x - 1, x + w, y - 1); // Top
            HLine(x - 1, x + w, y + h); // Down
            VLine(y, y + h, x - 1); // Left
            VLine(y - 1, y + h + 1, x + w); // Right
        }
        else
        {
            FillRect(x - lineSize, y - lineSize, w + lineSize, lineSize); // Top
            FillRect(x, y + h, w + lineSize, lineSize); // Down
            FillRect(x - lineSize, y, lineSize, h + lineSize); // Left
            FillRect(x + w, y - lineSize, lineSize, h + lineSize); // Right
        }
    }

    public static void DrawLine(int x0, int y0, int x1, int y1, int lineSize = 1)
    {
        if (!_ready)
        {
            return;
        }

        void OnePxLine()
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            while (true)
            {
                PixelSet(x0, y0);
                if (x0 == x1 && y0 == y1) break;
                var e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        void ThickLine()
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            float ed = (float)(dx + dy == 0 ? 1 : Math.Sqrt((float)dx * dx + (float)dy * dy));

            for (lineSize = (lineSize + 1) / 2; ;)
            {
                PixelSet(x0, y0);
                var e2 = err; var x2 = x0;
                if (2 * e2 >= -dx)
                {
                    int y2;
                    for (e2 += dy, y2 = y0; e2 < ed * lineSize && (y1 != y2 || dx > dy); e2 += dx)
                        PixelSet(x0, y2);
                    if (x0 == x1) break;
                    e2 = err; err -= dy; x0 += sx;
                }
                if (2 * e2 <= dy)
                {
                    for (e2 = dx - e2; e2 < ed * lineSize && (x1 != x2 || dx < dy); e2 += dy)
                        PixelSet(x2 += sx, y0);
                    if (y0 == y1) break;
                    err += dx; y0 += sy;
                }
            }
        }

        if (lineSize == 1)
        {
            OnePxLine();
            return;
        }

        ThickLine();
    }

    public static void DrawCircle(int centerX, int centerY, int radius)
    {
        if (!_ready)
        {
            return;
        }

        if (radius > 0)
        {
            int x = -radius, y = 0, err = 2 - 2 * radius;
            do
            {
                PixelSet(centerX - x, centerY + y);
                PixelSet(centerX - y, centerY - x);
                PixelSet(centerX + x, centerY - y);
                PixelSet(centerX + y, centerY + x);

                radius = err;
                if (radius <= y) err += ++y * 2 + 1;
                if (radius > x || err > y) err += ++x * 2 + 1;
            } while (x < 0);
        }
        else
        {
            PixelSet(centerX, centerY);
        }
    }

    public static void FillCircle(int centerX, int centerY, int radius)
    {
        if (!_ready)
        {
            return;
        }

        if (radius < 0 || centerX < -radius || centerY < -radius || centerX - _clipRect.Width > radius || centerY - _clipRect.Height > radius)
        {
            return;
        }

        if (radius > 0)
        {
            int x0 = 0;
            int y0 = radius;
            int d = 3 - 2 * radius;

            while (y0 >= x0)
            {
                HLine(centerX - y0, centerX + y0, centerY - x0);

                if (x0 > 0)
                {
                    HLine(centerX - y0, centerX + y0, centerY + x0);
                }

                if (d < 0)
                {
                    d += 4 * x0++ + 6;
                }
                else
                {
                    if (x0 != y0)
                    {
                        HLine(centerX - x0, centerX + x0, centerY - y0);
                        HLine(centerX - x0, centerX + x0, centerY + y0);
                    }
                    d += 4 * (x0++ - y0--) + 10;
                }
            }
        }
        else
        {
            PixelSet(centerX, centerY);
        }
    }

    public static void DrawTriangle(int x1, int y1, int x2, int y2, int x3, int y3, int lineSize = 1)
    {
        DrawLine(x1, y1, x2, y2, lineSize);
        DrawLine(x2, y2, x3, y3, lineSize);
        DrawLine(x3, y3, x1, y1, lineSize);
    }

    public static void ColorAdd(byte r, byte g, byte b, byte a)
    {
        if (!_ready)
        {
            return;
        }

        byte* ptr = (byte*)_pixels;

        for (int i = 0; i < _pixelsSizeInBytes / 4; ++i)
        {
            byte* ptr_idx = ptr + (i * 4);

            var sb = (*(ptr_idx) + b);
            var sg = (*(ptr_idx + 1) + g);
            var sr = (*(ptr_idx + 2) + r);
            var sa = (*(ptr_idx + 3) + a);

            *(ptr_idx) = (byte)Calc.Clamp(sb, 0, 255);
            *(ptr_idx + 1) = (byte)Calc.Clamp(sg, 0, 255);
            *(ptr_idx + 2) = (byte)Calc.Clamp(sr, 0, 255);
            *(ptr_idx + 3) = (byte)Calc.Clamp(sa, 0, 255);
        }
    }

    public static void ColorMult(float r, float g, float b, float a)
    {
        if (!_ready)
        {
            return;
        }

        byte* ptr = (byte*)_pixels;

        for (int i = 0; i < _pixelsSizeInBytes / 4; ++i)
        {
            byte* ptr_idx = ptr + (i * 4);

            if (*(ptr_idx + 3) == 0)
            {
                continue;
            }

            var sb = (*(ptr_idx) * b);
            var sg = (*(ptr_idx + 1) * g);
            var sr = (*(ptr_idx + 2) * r);
            var sa = (*(ptr_idx + 3) * a);

            *(ptr_idx) = (byte)Calc.Clamp(sb, 0, 255);
            *(ptr_idx + 1) = (byte)Calc.Clamp(sg, 0, 255);
            *(ptr_idx + 2) = (byte)Calc.Clamp(sr, 0, 255);
            *(ptr_idx + 3) = (byte)Calc.Clamp(sa, 0, 255);
        }
    }

    public static void PixelShift(int shiftX, int shiftY)
    {
        if (!_ready)
        {
            return;
        }

        Span<byte> copy = GetCopy(_pixelsSizeInBytes);

        int sw = _surfaceW;
        int sh = _surfaceH;

        
        byte* ptr = (byte*)_pixels;
        
        fixed (byte* copy_ptr = &MemoryMarshal.GetReference(copy))
        {
            for (int x = 0; x < sw; ++x)
            {
                var px = x - shiftX + sw;
                while (px < 0)
                {
                    px += sw;
                }
                px %= sw;
                for (int y = 0; y < sh; ++y)
                {
                    var py = y - shiftY + sh;
                    while (py < 0)
                    {
                        py += sh;
                    }
                    py %= sh;

                    int old_idx = (px + py * sw) * 4;
                    int new_idx = (x + y * sw) * 4;

                    byte* ptr_idx = ptr + new_idx;
                    byte* copy_idx = copy_ptr + old_idx;

                    *(ptr_idx) = *(copy_idx);
                    *(ptr_idx + 1) = *(copy_idx + 1);
                    *(ptr_idx + 2) = *(copy_idx + 2);
                    *(ptr_idx + 3) = *(copy_idx + 3);
                }
            }
        }
    }

    public static void Blit(
        Span<byte> pastePixels,
        int pastePixelsW,
        int pastePixelsH,
        int x,
        int y,
        Rectangle region = default,
        int w = 0,
        int h = 0,
        bool flip = false
    )
    {
        if (!_ready)
        {
            return;
        }

        if (region.IsEmpty)
        {
            region = new Rectangle(0, 0, pastePixelsW, pastePixelsH);
        }

        if (w == 0)
        {
            w = region.Width;
        }

        if (h == 0)
        {
            h = region.Height;
        }

        float factor_w = (float)w / region.Width;
        float factor_h = (float)h / region.Height;

        var min_x = Math.Max(x, _clipRect.Left);
        var min_y = Math.Max(y, _clipRect.Top);
        var max_x = Math.Min(x + w, _clipRect.Right);
        var max_y = Math.Min(y + h, _clipRect.Bottom);

        int sw = _surfaceW;

        ref var col = ref _mDrawColor;
        
        byte* ptr = (byte*)_pixels;

        fixed (byte* paste = &MemoryMarshal.GetReference(pastePixels))
        {
            if (!flip)
            {
                for (int px = min_x; px < max_x; ++px)
                {
                    for (int py = min_y; py < max_y; ++py)
                    {
                        byte* src_idx = paste + (region.X + (int)((px - x) / factor_w) + (region.Y + (int)((py - y) / factor_h)) * pastePixelsW) * 4;

                        if (*(src_idx + 3) == 0)
                        {
                            continue;
                        }


                        byte* ptr_idx = ptr + (px + py * sw) * 4;

                        if (col.Abgr == 1)
                        {
                            *(ptr_idx) = *(src_idx);
                            *(ptr_idx + 1) = *(src_idx + 1);
                            *(ptr_idx + 2) = *(src_idx + 2);
                            *(ptr_idx + 3) = *(src_idx + 3);
                        }
                        else
                        {
                            var sbf = (*(src_idx)) / 255.0f;
                            var sgf = (*(src_idx + 1)) / 255.0f;
                            var srf = (*(src_idx + 2)) / 255.0f;
                            var saf = (*(src_idx + 3)) / 255.0f;

                            var bf = col.Bf;
                            var gf = col.Gf;
                            var rf = col.Rf;
                            var af = col.Af;

                            *(ptr_idx) = ((byte)(sbf * bf * 255.0f));
                            *(ptr_idx + 1) = ((byte)(sgf * gf * 255.0f));
                            *(ptr_idx + 2) = ((byte)(srf * rf * 255.0f));
                            *(ptr_idx + 3) = ((byte)(saf * af * 255.0f));
                        }
                    }
                }
            }
            else
            {
                var start_pix_x = region.Right - 1;

                for (int px = min_x; px < max_x; ++px)
                {
                    for (int py = min_y; py < max_y; ++py)
                    {
                        byte* src_idx = paste + (start_pix_x + (int)((px - x) / factor_w) + (region.Y + (int)((py - y) / factor_h)) * pastePixelsW) * 4;

                        if (*(src_idx + 3) == 0)
                        {
                            continue;
                        }


                        byte* ptr_idx = ptr + (px + py * sw) * 4;

                        if (col.Abgr == 1)
                        {
                            *(ptr_idx) = *(src_idx);
                            *(ptr_idx + 1) = *(src_idx + 1);
                            *(ptr_idx + 2) = *(src_idx + 2);
                            *(ptr_idx + 3) = *(src_idx + 3);
                        }
                        else
                        {
                            var sbf = (*(src_idx)) / 255.0f;
                            var sgf = (*(src_idx + 1)) / 255.0f;
                            var srf = (*(src_idx + 2)) / 255.0f;
                            var saf = (*(src_idx + 3)) / 255.0f;

                            var bf = col.Bf;
                            var gf = col.Gf;
                            var rf = col.Rf;
                            var af = col.Af;

                            *(ptr_idx) = ((byte)(sbf * bf * 255.0f));
                            *(ptr_idx + 1) = ((byte)(sgf * gf * 255.0f));
                            *(ptr_idx + 2) = ((byte)(srf * rf * 255.0f));
                            *(ptr_idx + 3) = ((byte)(saf * af * 255.0f));
                        }
                    }
                }
            }
        }
    }

    public static void Blit(
        Pixmap pixmap,
        int x,
        int y,
        Rectangle region = default,
        int w = 0,
        int h = 0
    )
    {
        SDL_Rect srcRect = new SDL_Rect()
        {
            x = region.X,
            y = region.Y,
            w = region.Width > 0 ? region.Width : pixmap.Width,
            h = region.Height > 0 ? region.Height : pixmap.Height
        };

        SDL_Rect dstRect = new SDL_Rect()
        {
            x = x,
            y = y,
            w = w > 0 ? w : pixmap.Width,
            h = h > 0 ? h : pixmap.Height
        };

        if (SDL_BlitSurface(pixmap.Pixels, ref srcRect, _pixels, ref dstRect) != 0)
        {
            throw new ApplicationException("Could not blit pixmap");
        }
        
    }


    // ==========================================================
    // FILTERS
    // ==========================================================

    public static void DropShadow(int offsetX, int offsetY, Color color)
    {
        if (!_ready)
        {
            return;
        }

        byte r = color.R;
        byte g = color.G;
        byte b = color.B;
        byte a = color.A;

        Span<byte> copy = GetCopy(_pixelsSizeInBytes);

        PixelShift(offsetX, offsetY);
        ColorAdd(255, 255, 255, 0);

        ColorMult(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);

        Blit(copy, _surfaceW, _surfaceH, 0, 0);
    }

    private static Span<byte> GetCopy(int length)
    {
        if (length > AuxBufferSize)
        {
            throw new Exception($"Blitter GetCopy: Overflow. Length {length} is bigger than max {AuxBufferSize}");
        }

        AuxBuffers[_auxBuffersIdx] ??= new byte[AuxBufferSize];
        
        Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref AuxBuffers[_auxBuffersIdx]), (void*)_pixels, (uint)length);

        var result = new Span<byte>(AuxBuffers[_auxBuffersIdx], 0, length);

        _auxBuffersIdx++;

        if (_auxBuffersIdx > AuxBufferStackSize - 1)
        {
            _auxBuffersIdx = 0;
        }

        return result;
    }
}