using PRANA.Foundation;

namespace PRANA;

public class DynamicVertexBuffer : RenderResource
{
    internal Bgfx.DynamicVertexBufferHandle Handle { get; }

    internal DynamicVertexBuffer(string id, Bgfx.DynamicVertexBufferHandle handle) : base(id)
    {
        Handle = handle;
    }

    public void Update(int startVertex, Span<VertexPCT> vertices)
    {
        Graphics.UpdateDynamicVertexBuffer(this, startVertex, vertices);
    }

    protected override void Free()
    {
        Graphics.DestroyDynamicVertexBuffer(this);
    }
}
