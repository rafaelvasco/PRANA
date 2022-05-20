using System.Runtime.InteropServices;
using PRANA.Common;

namespace PRANA;

public class QuadVertexStream : RenderResource, IVertexStream
{
    public int VertexCount => _vertexIndex;

    private DynamicIndexBuffer _dynamicIndexBuffer;

    private int _indiceIndex;
    private int _vertexIndex;

    private VertexPCT[] _vertices;
    private readonly VertexLayout _layout;


    public QuadVertexStream(string id, VertexLayout layout, int maxQuads = 2048) : base(id)
    {
        if (!Calc.IsPowerOfTwo(maxQuads))
        {
            maxQuads = Calc.NextPowerOfTwo(maxQuads);
        }

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
            _dynamicIndexBuffer = Graphics.CreateDynamicIndexBuffer<ushort>(indices);
        }
        else
        {
            Graphics.UpdateDynamicIndexBuffer<ushort>(_dynamicIndexBuffer, 0, indices);
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
        Submit(0, _vertexIndex, 0, _indiceIndex);
    }

    public void Submit(int startingVertexIndex, int vertexCount, int startingIndiceIndex, int indexCount)
    {
        Graphics.SetDynamicIndexBuffer(_dynamicIndexBuffer, startingIndiceIndex, indexCount > 0 ? indexCount : _indiceIndex);

        var verticesSpan = new Span<VertexPCT>(_vertices, startingVertexIndex, vertexCount > 0 ? vertexCount : _vertexIndex);

        var transientVbo = Graphics.CreateTransientVertexBuffer(verticesSpan, _layout, VertexPCT.Stride);

        Graphics.SetTransientVertexBuffer(transientVbo, vertexCount);
    }
}