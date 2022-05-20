﻿using PRANA.Foundation.BGFX;

namespace PRANA;

public class VertexBuffer
{
    internal Bgfx.VertexBufferHandle Handle { get; }

    internal VertexBuffer(Bgfx.VertexBufferHandle  handle)
    {
        Handle = handle;
    }
}