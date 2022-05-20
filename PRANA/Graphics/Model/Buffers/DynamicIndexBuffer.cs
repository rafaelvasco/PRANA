using PRANA.Foundation.BGFX;

namespace PRANA;

public class DynamicIndexBuffer
{
    internal Bgfx.DynamicIndexBufferHandle Handle { get; }

    internal DynamicIndexBuffer(Bgfx.DynamicIndexBufferHandle handle)
    {
        Handle = handle;
    }

    public void Update<T>(int startIndex, Span<T> indices)
    {
        Graphics.UpdateDynamicIndexBuffer(this, startIndex, indices);
    }
}
