using PRANA.Foundation.BGFX;

namespace PRANA;

public struct TransientIndexBuffer
{
    internal Bgfx.TransientIndexBuffer Handle;

    internal TransientIndexBuffer(Bgfx.TransientIndexBuffer handle)
    {
        Handle = handle;
    }
}
