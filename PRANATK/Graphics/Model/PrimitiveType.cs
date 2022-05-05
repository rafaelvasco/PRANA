using PRANA.Foundation;

namespace PRANA;

public enum PrimitiveType : ulong
{
    Triangles = 0x0,
    TriangleStrip = Bgfx.StateFlags.PtTristrip,
    Lines = Bgfx.StateFlags.PtLines,
    LineStrip = Bgfx.StateFlags.PtLinestrip,
    Points = Bgfx.StateFlags.PtPoints
}
