using PRANA.Foundation.BGFX;

namespace PRANA;

public class DynamicVertexBuffer
{
    internal Bgfx.DynamicVertexBufferHandle Handle { get; }

    internal DynamicVertexBuffer(Bgfx.DynamicVertexBufferHandle handle)
    {
        Handle = handle;
    }

    public void Update<T>(int startVertex, Span<T> vertices)
    {
        Graphics.UpdateDynamicVertexBuffer(this, startVertex, vertices);
    }
}
