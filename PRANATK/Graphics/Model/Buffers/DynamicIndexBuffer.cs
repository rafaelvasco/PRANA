using PRANA.Foundation;

namespace PRANA;

public class DynamicIndexBuffer : RenderResource
{
    internal Bgfx.DynamicIndexBufferHandle Handle { get; }

    internal DynamicIndexBuffer(string id, Bgfx.DynamicIndexBufferHandle handle) : base(id)
    {
        Handle = handle;
    }

    public void Update(int startIndex, Span<ushort> indices)
    {
        Graphics.UpdateDynamicIndexBuffer(this, startIndex, indices);
    }

    protected override void Free()
    {
        Graphics.DestroyDynamicIndexBuffer(this);
    }
}
