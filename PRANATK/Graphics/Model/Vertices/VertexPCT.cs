using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PRANA;

// Vertex containing Position, Color, and Texture UV Data.
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPCT
{
    public float X;

    public float Y;

    public float Z;

    public uint Col;

    public float Tx;

    public float Ty;

    public static readonly VertexLayout VertexLayout;

    private static readonly int _stride;

    public static int Stride => _stride;

    public VertexPCT(float x, float y, float z, uint abgr, float tx = 0f, float ty = 0f)
    {
        X = x;
        Y = y;
        Z = z;
        Col = abgr;
        Tx = tx;
        Ty = ty;
    }

    static VertexPCT()
    {
        VertexLayout = new VertexLayout();
        VertexLayout.Begin();
        VertexLayout.Add(VertexAttribute.Position, VertexAttributeType.Float, 3, false, false);
        VertexLayout.Add(VertexAttribute.Color0, VertexAttributeType.UInt8, 4, true, false);
        VertexLayout.Add(VertexAttribute.Texture0, VertexAttributeType.Float, 2, false, false);
        VertexLayout.End();

        _stride = Unsafe.SizeOf<VertexPCT>();
    }

    public override string ToString()
    {
        return $"{X},{Y},{Z},{Col},{Tx},{Ty}";
    }
}
