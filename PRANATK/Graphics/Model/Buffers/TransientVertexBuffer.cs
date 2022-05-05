using PRANA.Foundation;

namespace PRANA;

public struct TransientVertexBuffer
{
    internal Bgfx.TransientVertexBuffer Handle;

    internal TransientVertexBuffer(Bgfx.TransientVertexBuffer handle)
    {
        Handle = handle;
    }
}
