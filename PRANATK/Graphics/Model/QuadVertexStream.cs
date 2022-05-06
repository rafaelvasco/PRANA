using System.Runtime.InteropServices;

namespace PRANA;

public class QuadVertexStream : RenderResource, IVertexStream
{
    public int VertexCount => _vertexIndex;
    public int IndexCount => _indiceIndex;

    private DynamicIndexBuffer _dynamicIndexBuffer;

    private int _indiceIndex;
    private int _vertexIndex;

    private VertexPCT[] _vertices;
    private readonly VertexLayout _layout;


    public QuadVertexStream(string id, VertexLayout layout, int maxQuads = 2048) : base(id)
    {
        _layout = layout;
        _vertices = new VertexPCT[maxQuads * 4];

        BuildIndices(maxQuads);

        Graphics.RegisterRenderResource(this);
    }

    private void BuildIndices(int maxQuads)
    {
        int countIndices = maxQuads * 6;

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

        if (_dynamicIndexBuffer == null)
        {
            _dynamicIndexBuffer = Graphics.CreateDynamicIndexBuffer(indices);
        }
        else
        {
            Graphics.UpdateDynamicIndexBuffer(_dynamicIndexBuffer, 0, indices);
        }
    }


    public void Reset()
    {
        _indiceIndex = 0;
        _vertexIndex = 0;
    }

    protected override void Free()
    {
        if (_dynamicIndexBuffer is { Handle: { Valid: true } })
        {
            Graphics.DestroyDynamicIndexBuffer(_dynamicIndexBuffer);
        }
    }

    public unsafe void PushQuad(ref Quad quad)
    {
        IncreaseBuffersIfNeeded(4);

        fixed (VertexPCT* p = &MemoryMarshal.GetArrayDataReference(_vertices))
        {
            int index = _vertexIndex;

            *(p + index) = quad.TopLeft;
            *(p + index + 1) = quad.TopRight;
            *(p + index + 2) = quad.BottomRight;
            *(p + index + 3) = quad.BottomLeft; 
        }

        unchecked
        {
            _vertexIndex += 4;
            _indiceIndex += 6;
        }
    }

    private void IncreaseBuffersIfNeeded(int delta)
    {
        if (_vertexIndex + delta > _vertices.Length)
        {
            Array.Resize(ref _vertices, _vertices.Length * 2);

            BuildIndices(maxQuads: _vertices.Length / 4);
        }
    }

    
    public void Submit()
    {
        Submit(VertexCount, IndexCount);
    }

    public void Submit(int vertexCount, int indexCount)
    {
        Graphics.SetDynamicIndexBuffer(_dynamicIndexBuffer, 0, indexCount);

        var verticesSpan = new Span<VertexPCT>(_vertices, 0, vertexCount);

        var transientVbo = Graphics.CreateTransientVertexBuffer(verticesSpan, _layout);

        Graphics.SetTransientVertexBuffer(transientVbo, vertexCount);
    }
}