using PRANA.Foundation;

namespace PRANA;

public class DynamicVertexBuffer
{
    internal Bgfx.DynamicVertexBufferHandle Handle { get; }

    internal DynamicVertexBuffer(Bgfx.DynamicVertexBufferHandle handle)
    {
        Handle = handle;
    }

    public void Update(int startVertex, Span<VertexPCT> vertices)
    {
        Graphics.UpdateDynamicVertexBuffer(this, startVertex, vertices);
    }
}
