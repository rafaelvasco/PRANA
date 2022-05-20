using System.Runtime.InteropServices;
using PRANA.Common;

namespace PRANA;

public struct Quad
{
    public VertexPCT TopLeft;
    public VertexPCT TopRight;
    public VertexPCT BottomRight;
    public VertexPCT BottomLeft;

    public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Quad));

    public float Width => Calc.Abs(TopRight.X - TopLeft.X);
    public float Height => Calc.Abs(BottomRight.Y - TopRight.Y);

    public Quad(
        float x1, float y1, float z1, Color color1, float t1x, float t1y,
        float x2, float y2, float z2, Color color2, float t2x, float t2y,
        float x3, float y3, float z3, Color color3, float t3x, float t3y,
        float x4, float y4, float z4, Color color4, float t4x, float t4y
    )
    {
        TopLeft = new VertexPCT(x1, y1, z1, color1, t1x, t1y);
        TopRight = new VertexPCT(x2, y2, z2, color2, t2x, t2y);
        BottomRight = new VertexPCT(x3, y3, z3, color3, t3x, t3y);
        BottomLeft = new VertexPCT(x4, y4, z4, color4, t4x, t4y);
    }

    public Quad(RectangleF rect)
    {
        TopLeft = default;
        TopRight = default;
        BottomRight = default;
        BottomLeft = default;

        TopLeft.X = rect.Left;
        TopLeft.Y = rect.Top;
        TopLeft.Col = 0xFFFFFFFF;
        TopRight.X = rect.Right;
        TopRight.Y = rect.Top;
        TopRight.Col = 0xFFFFFFFF;
        BottomRight.X = rect.Right;
        BottomRight.Y = rect.Bottom;
        BottomRight.Col = 0xFFFFFFFF;
        BottomLeft.X = rect.Left;
        BottomLeft.Y = rect.Bottom;
        BottomLeft.Col = 0xFFFFFFFF;
    }

    public Quad(Texture2D texture, RectangleF srcRect = default)
    {
        TopLeft = default;
        TopRight = default;
        BottomRight = default;
        BottomLeft = default;

        if (texture == null) return;

        SetRegion(texture, srcRect);

        float w = texture.Width;
        float h = texture.Height;

        if (!srcRect.IsEmpty)
        {
            w = srcRect.Width;
            h = srcRect.Height;
        }

        TopLeft.X = 0;
        TopLeft.Y = 0;
        TopLeft.Col = 0xFFFFFFFF;

        TopRight.X = w;
        TopRight.Y = 0;
        TopRight.Col = 0xFFFFFFFF;

        BottomRight.X = w;
        BottomRight.Y = h;
        BottomRight.Col = 0xFFFFFFFF;

        BottomLeft.X = 0;
        BottomLeft.Y = h;
        BottomLeft.Col = 0xFFFFFFFF;
    }

    public void SetColor(Color color)
    {
        TopLeft.Col = color;
        TopRight.Col = color;
        BottomRight.Col = color;
        BottomLeft.Col = color;
    }

    public void SetColors(Color colorTopLeft, Color colorTopRight, Color colorBottomLeft, Color colorBottomRight)
    {
        TopLeft.Col = colorTopLeft;
        TopRight.Col = colorTopRight;
        BottomRight.Col = colorBottomRight;
        BottomLeft.Col = colorBottomLeft;
    }

    public void SetZ(float depth)
    {
        TopLeft.Z = depth;
        TopRight.Z = depth;
        BottomRight.Z = depth;
        BottomLeft.Z = depth;
    }

    public void SetXY(float x, float y, float originX, float originY)
    {
        SetXYWH(x, y, Width, Height, originX, originY);
    }

    public void SetXYWH(float x, float y, float w, float h, float originX, float originY)
    {
        var x0 = x - w * originX;
        var y0 = y - h * originY;
        var x1 = x0 + w;
        var y1 = y0 + h;

        TopLeft.X = x0;
        TopLeft.Y = y0;

        TopRight.X = x1;
        TopRight.Y = y0;

        BottomRight.X = x1;
        BottomRight.Y = y1;

        BottomLeft.X = x0;
        BottomLeft.Y = y1;
    }

    public void SetXYWHR(float x, float y, float w, float h, float originX, float originY, float sin, float cos)
    {
        var dx = w * originX;
        var dy = h * originY;

        TopLeft.X = x + dx * cos - dy * sin;
        TopLeft.Y = y + dx * sin + dy * cos;

        TopRight.X = x + (dx + w) * cos - dy * sin;
        TopRight.Y = y + (dx + w) * sin + dy * cos;
        
        BottomLeft.X = x + dx * cos - (dy + h) * sin;
        BottomLeft.Y = y + dx * sin + (dy + h) * cos;

        BottomRight.X = x + (dx + w) * cos - (dy + h) * sin;
        BottomRight.Y = y + (dx + w) * sin + (dy + h) * cos;
    }

    public void SetRegion(Texture2D texture, RectangleF region)
    {
        float ax, ay, bx, by;

        if (region.IsEmpty)
        {
            ax = 0;
            ay = 0;
            bx = 1;
            by = 1;
        }
        else
        {
            float inv_tex_w = 1.0f / texture.Width;
            float inv_tex_h = 1.0f / texture.Height;

            ax = region.Left * inv_tex_w;
            ay = region.Top * inv_tex_h;
            bx = region.Right * inv_tex_w;
            by = region.Bottom * inv_tex_h;
        }

        TopLeft.Tx = ax;
        TopLeft.Ty = ay;
        TopRight.Tx = bx;
        TopRight.Ty = ay;
        BottomRight.Tx = bx;
        BottomRight.Ty = by;
        BottomLeft.Tx = ax;
        BottomLeft.Ty = by;
    }

    public override string ToString()
    {
        return $"{TopLeft};{TopRight};{BottomRight};{BottomLeft}";
    }

    public RectangleF GetRegionRect(Texture2D texture)
    {
        return new RectangleF(TopLeft.Tx * texture.Width, TopLeft.Ty * texture.Height, BottomRight.X * texture.Width, BottomRight.Y * texture.Height);
    }
}
