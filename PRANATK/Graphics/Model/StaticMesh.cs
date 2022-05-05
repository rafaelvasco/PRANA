using System.Runtime.CompilerServices;

namespace PRANA;

public class StaticMesh : RenderResource, IMesh
{
    private IndexBuffer _indexBuffer;
    private VertexBuffer _vertexBuffer;
    private int _numIndices;
    private int _numVertices;

    public int VertexCount => _numVertices;

    public int IndexCount => _numIndices;

    public StaticMesh(string id) : base(id)
    {
    }

    public void SetIndices(ushort[] indices)
    {
        if (_indexBuffer != null)
        {
            throw new ApplicationException("Can only set StaticMesh data once.");
        }

        _numIndices = indices.Length;

        _indexBuffer = Graphics.CreateIndexBuffer($"{Id}_indexBuffer", indices);

    }

    public void SetVertices(VertexPCT[] vertices, VertexLayout layout)
    {
        _vertexBuffer = Graphics.CreateVertexBuffer($"{Id}_vertexBuffer", vertices, layout);

        _numVertices = vertices.Length;
    }

    protected override void Free()
    {
        if (_indexBuffer != null)
        {
            Graphics.DestroyIndexBuffer(_indexBuffer);
        }

        if (_vertexBuffer != null)
        {
            Graphics.DestroyVertexBuffer(_vertexBuffer);
        }
    }

    void IMesh.Submit()
    {
        InternalSubmit(_numVertices, _numIndices);
    }


    void IMesh.Submit(int vertexCount, int indexCount)
    {
        InternalSubmit(vertexCount, indexCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InternalSubmit(int vertexCount, int indexCount)
    {
        if (_vertexBuffer == null)
        {
            return;
        }

        if (_indexBuffer != null)
        {
            Graphics.SetIndexBuffer(_indexBuffer, 0, indexCount);
        }

        Graphics.SetVertexBuffer(_vertexBuffer, 0, vertexCount);
    }
}
