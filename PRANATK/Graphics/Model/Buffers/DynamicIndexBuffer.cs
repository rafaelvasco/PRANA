using PRANA.Foundation;

namespace PRANA;

public class DynamicIndexBuffer
{
    internal Bgfx.DynamicIndexBufferHandle Handle { get; }

    internal DynamicIndexBuffer(Bgfx.DynamicIndexBufferHandle handle)
    {
        Handle = handle;
    }

    public void Update(int startIndex, Span<ushort> indices)
    {
        Graphics.UpdateDynamicIndexBuffer(this, startIndex, indices);
    }
}
