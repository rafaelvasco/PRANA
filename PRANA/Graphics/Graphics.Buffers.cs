﻿using System.Runtime.CompilerServices;
using PRANA.Foundation.BGFX;

namespace PRANA;

public static unsafe partial class Graphics
{
    public static VertexBuffer CreateVertexBuffer<T>(Span<T> vertices, VertexLayout layout)
    {
        var memory = Bgfx.GetMemoryBufferReference(vertices);
        var handle = Bgfx.CreateVertexBuffer(memory, &layout.Handle, (ushort)Bgfx.BufferFlags.None);

        var vertexBuffer = new VertexBuffer(handle);

        return vertexBuffer;
    }

    public static void DestroyVertexBuffer(VertexBuffer buffer)
    {
        if (buffer.Handle.Valid)
        {
            Bgfx.DestroyVertexBuffer(buffer.Handle);
        }
    }

    public static IndexBuffer CreateIndexBuffer<T>(Span<T> indices)
    {
        var memory = Bgfx.GetMemoryBufferReference(indices);
        var handle = Bgfx.CreateIndexBuffer(memory, (ushort)Bgfx.BufferFlags.None);

        var indexBuffer = new IndexBuffer(handle);

        return indexBuffer;
    }

    public static void DestroyIndexBuffer(IndexBuffer buffer)
    {
        if (buffer.Handle.Valid)
        {
            Bgfx.DestroyIndexBuffer(buffer.Handle);
        }
    }

    public static DynamicVertexBuffer CreateDynamicVertexBuffer(int vertexCount, VertexLayout layout)
    {
        var handle = Bgfx.CreateDynamicVertexBuffer((uint)vertexCount, &layout.Handle, (ushort)Bgfx.BufferFlags.None);

        var buffer = new DynamicVertexBuffer(handle);

        return buffer;
    }

    public static DynamicVertexBuffer CreateDynamicVertexBuffer<T>(Span<T> vertices, VertexLayout layout)
    {
        var memory = Bgfx.GetMemoryBufferReference(vertices);

        var handle = Bgfx.CreateDynamicVertexBufferMem(memory, &layout.Handle, (ushort)Bgfx.BufferFlags.None);

        var buffer = new DynamicVertexBuffer(handle);

        return buffer;
    }

    public static void UpdateDynamicVertexBuffer<T>(DynamicVertexBuffer buffer, int startVertex, Span<T> vertices)
    {
        var memory = Bgfx.GetMemoryBufferReference(vertices);

        Bgfx.UpdateDynamicVertexBuffer(buffer.Handle, (uint)startVertex, memory);
    }

    public static void DestroyDynamicVertexBuffer(DynamicVertexBuffer buffer)
    {
        if (buffer.Handle.Valid)
        {
            Bgfx.DestroyDynamicVertexBuffer(buffer.Handle);
        }
    }

    public static DynamicIndexBuffer CreateDynamicIndexBuffer(int indexCount)
    {
        var handle = Bgfx.CreateDynamicIndexBuffer((uint)indexCount, (ushort)Bgfx.BufferFlags.AllowResize);

        var buffer = new DynamicIndexBuffer(handle);

        return buffer;
    }

    public static DynamicIndexBuffer CreateDynamicIndexBuffer<T>(Span<T> indices)
    {
        var memory = Bgfx.GetMemoryBufferReference(indices);

        var handle = Bgfx.CreateDynamicIndexBufferMem(memory, (ushort)Bgfx.BufferFlags.None);

        var buffer = new DynamicIndexBuffer(handle);

        return buffer;
    }

    public static void UpdateDynamicIndexBuffer<T>(DynamicIndexBuffer buffer, int startIndex, Span<T> indices)
    {
        var memory = Bgfx.GetMemoryBufferReference(indices);

        Bgfx.UpdateDynamicIndexBuffer(buffer.Handle, (uint)startIndex, memory);
    }

    public static void DestroyDynamicIndexBuffer(DynamicIndexBuffer buffer)
    {
        if (buffer.Handle.Valid)
        {
            Bgfx.DestroyDynamicIndexBuffer(buffer.Handle);
        }
    }

    public static TransientVertexBuffer CreateTransientVertexBuffer<T>(Span<T> vertices, VertexLayout layout, int vertexByteSize)
    {
        var handle = new Bgfx.TransientVertexBuffer();

        Bgfx.AllocTransientVertexBuffer(&handle, (uint)vertices.Length, &layout.Handle);

        var dataSize = vertices.Length * vertexByteSize;

        Unsafe.CopyBlockUnaligned(handle.data, Unsafe.AsPointer(ref vertices[0]), (uint)dataSize);

        return new TransientVertexBuffer(handle);
    }

    public static bool AllocateTransientBuffers(int numVertices, VertexLayout vertexLayout, int numIndices, out TransientVertexBuffer tvb, out TransientIndexBuffer tib)
    {
        Bgfx.TransientVertexBuffer bgfxTvb;

        Bgfx.TransientIndexBuffer bgfxTib;

        var couldAllocate =
            Bgfx.AllocTransientBuffers(&bgfxTvb, &vertexLayout.Handle, (uint)numVertices, &bgfxTib, (uint)numIndices, false);

        if (!couldAllocate)
        {
            tvb = new TransientVertexBuffer(bgfxTvb);
            tib = new TransientIndexBuffer(bgfxTib);
            return false;
        }
        
        tvb = new TransientVertexBuffer(bgfxTvb);

        tib = new TransientIndexBuffer(bgfxTib);

        return true;
    }

    public static void SetIndexBuffer(IndexBuffer buffer, int firstIndex, int numIndices)
    {
        Bgfx.SetIndexBuffer(buffer.Handle, (uint)firstIndex, (uint)numIndices);
    }

    public static void SetDynamicIndexBuffer(DynamicIndexBuffer buffer, int firstIndex, int numIndices)
    {
        Bgfx.SetDynamicIndexBuffer(buffer.Handle, (uint)firstIndex, (uint)numIndices);
    }

    public static void SetVertexBuffer(VertexBuffer buffer, int firstIndex, int numVertices)
    {
        Bgfx.SetVertexBuffer(0, buffer.Handle, (uint)firstIndex, (uint)numVertices);
    }

    public static void SetDynamicVertexBuffer(DynamicVertexBuffer buffer, int firstIndex, int numVertices)
    {
        Bgfx.SetDynamicVertexBuffer(0, buffer.Handle, (uint)firstIndex, (uint)numVertices);
    }

    public static void SetTransientVertexBuffer(TransientVertexBuffer buffer, int numVertices)
    {
        Bgfx.SetTransientVertexBuffer(0, &buffer.Handle, 0, (uint)numVertices);
    }
}