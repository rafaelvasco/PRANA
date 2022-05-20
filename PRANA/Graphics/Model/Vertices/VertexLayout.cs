using System.Runtime.CompilerServices;
using PRANA.Foundation.BGFX;

namespace PRANA;

public unsafe struct VertexLayout
{
    internal Bgfx.VertexLayout Handle;

    public void Begin()
    {
        Bgfx.VertexLayoutBegin((Bgfx.VertexLayout*)Unsafe.AsPointer(ref Handle), Bgfx.GetRendererType());
    }

    public void Add(VertexAttribute attribute, VertexAttributeType type, int num, bool normalized, bool asInt)
    {
        Bgfx.VertexLayoutAdd((Bgfx.VertexLayout*)Unsafe.AsPointer(ref Handle), (Bgfx.Attrib)attribute, (byte)num, (Bgfx.AttribType)type, normalized, asInt);
    }

    public void End()
    {
        Bgfx.VertexLayoutEnd((Bgfx.VertexLayout*)Unsafe.AsPointer(ref Handle));
    }
}
