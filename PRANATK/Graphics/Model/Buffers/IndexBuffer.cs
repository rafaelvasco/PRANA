using PRANA.Foundation;

namespace PRANA;

public class IndexBuffer : RenderResource
{
    internal Bgfx.IndexBufferHandle Handle { get; }

    internal IndexBuffer(string id, Bgfx.IndexBufferHandle handle) : base(id)
    {
        Handle = handle;
    }

    protected override void Free()
    {
        Graphics.DestroyIndexBuffer(this);
    }
}
