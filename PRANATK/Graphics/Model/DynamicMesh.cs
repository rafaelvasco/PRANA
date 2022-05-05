using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PRANA;

public unsafe class DynamicMesh : RenderResource, IMesh
{
    internal DynamicIndexBuffer DynamicIndexBuffer => _dynamicIndexBuffer;

    internal VertexLayout Layout => _layout;

    public int VertexCount => _vertexCount;

    public int IndexCount => _indexCount;

    private DynamicIndexBuffer _dynamicIndexBuffer;

    private VertexPCT[] _vertices;
    private readonly VertexLayout _layout;

    private int _vertexCount;
    private int _indexCount;


    public DynamicMesh(
        string id,
        int maxVertices,
        VertexLayout layout) : base(id)
    {

        _vertices = new VertexPCT[maxVertices];
        _layout = layout;

    }

    public void SetIndices(int maxQuads)
    {
        int countIndices = maxQuads * 6;

        _dynamicIndexBuffer ??= Graphics.CreateDynamicIndexBuffer(Id + "DynIndexBuffer", countIndices);

        var indices = new ushort[countIndices];

        var index = 0;

        for (int i = 0; i < maxQuads; i++, index += 6)
        {
            indices[index + 0] = (ushort)(i * 4 + 0);
            indices[index + 1] = (ushort)(i * 4 + 1);
            indices[index + 2] = (ushort)(i * 4 + 2);

            indices[index + 3] = (ushort)(i * 4 + 0);
            indices[index + 4] = (ushort)(i * 4 + 2);
            indices[index + 5] = (ushort)(i * 4 + 3);
        }

        Graphics.UpdateDynamicIndexBuffer(_dynamicIndexBuffer, 0, indices);
    }

    public void PushTriangle(VertexPCT vertex1, VertexPCT vertex2, VertexPCT vertex3)
    {
        IncreaseBuffersIfNeeded(3);

        fixed (VertexPCT* p = &MemoryMarshal.GetArrayDataReference(_vertices))
        {
            int index = _vertexCount;

            *(p + index) = vertex1;
            *(p + index + 1) = vertex2;
            *(p + index + 2) = vertex3;
        }

        _vertexCount += 3;
    }

    public void PushQuad(ref Quad quad)
    {
        IncreaseBuffersIfNeeded(4);

        fixed (VertexPCT* p = &MemoryMarshal.GetArrayDataReference(_vertices))
        {
            int index = _vertexCount;

            *(p + index) = quad.TopLeft;
            *(p + index + 1) = quad.TopRight;
            *(p + index + 2) = quad.BottomRight;
            *(p + index + 3) = quad.BottomLeft;
        }

        _vertexCount += 4;
        _indexCount += 6;
    }

    public void Reset()
    {
        _vertexCount = 0;
        _indexCount = 0;
    }

    private void IncreaseBuffersIfNeeded(int delta)
    {
        if (_vertexCount + delta > _vertices.Length)
        {
            Array.Resize(ref _vertices, _vertices.Length * 2);
        }

        if (_dynamicIndexBuffer != null)
        {
            int newMaxQuads = _vertices.Length / 4;
            SetIndices(newMaxQuads);
        }
    }

    void IMesh.Submit()
    {
        InternalSubmit(_vertexCount, _indexCount);
    }

    void IMesh.Submit(int vertexCount, int indexCount)
    {
        InternalSubmit(vertexCount, indexCount);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InternalSubmit(int vertexCount, int indexCount)
    {
        if (_dynamicIndexBuffer != null)
        {
            Graphics.SetDynamicIndexBuffer(_dynamicIndexBuffer, 0, indexCount);
        }

        var verticesSpan = new Span<VertexPCT>(_vertices, 0, vertexCount);

        var transientVbo = Graphics.CreateTransientVertexBuffer(verticesSpan, _layout);

        Graphics.SetTransientVertexBuffer(transientVbo, vertexCount);
    }

    protected override void Free()
    {
        if (_dynamicIndexBuffer != null)
        {
            Graphics.DestroyDynamicIndexBuffer(_dynamicIndexBuffer);
        }
    }

    
}
