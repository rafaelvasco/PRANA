using PRANA.Foundation.BGFX;

namespace PRANA;

public class IndexBuffer
{
    internal Bgfx.IndexBufferHandle Handle { get; }

    internal IndexBuffer(Bgfx.IndexBufferHandle handle)
    {
        Handle = handle;
    }
}
