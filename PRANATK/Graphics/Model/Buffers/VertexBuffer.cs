using PRANA.Foundation;

namespace PRANA;

public class VertexBuffer : RenderResource
{
    internal Bgfx.VertexBufferHandle Handle { get; }

    internal VertexBuffer(string id, Bgfx.VertexBufferHandle  handle) : base(id)
    {
        Handle = handle;
    }

    protected override void Free()
    {
        Graphics.DestroyVertexBuffer(this);
    }
}
