using System.Runtime.InteropServices;

namespace PRANA;

public class VertexStream : RenderResource, IVertexStream
{
    public bool Static { get; }

    public int VertexCount => _vertexIndex;
    public int IndexCount => 0;

    private VertexBuffer _vertexBuffer;
    private int _vertexIndex;

    private VertexPCT[] _vertices;
    private readonly VertexLayout _layout;


    public VertexStream(string id, VertexLayout layout, bool isStatic = false) : base(id)
    {
        _layout = layout;
        _vertices = new VertexPCT[2048 * 4];
        Static = isStatic;

        Graphics.RegisterRenderResource(this);
    }


    public void Begin()
    {
        _vertexIndex = 0;

        if (Static)
        {
            if (_vertexBuffer != null)
            {
                Graphics.DestroyVertexBuffer(_vertexBuffer);
                _vertexBuffer = null;
            }
        }
    }

    protected override void Free()
    {
        if (_vertexBuffer is { Handle: { Valid: true } })
        {
            Graphics.DestroyVertexBuffer(_vertexBuffer);
        }
    }

    public unsafe void PushTriangle(VertexPCT vertex1, VertexPCT vertex2, VertexPCT vertex3)
    {
        IncreaseBuffersIfNeeded(3);

        fixed (VertexPCT* p = &MemoryMarshal.GetArrayDataReference(_vertices))
        {
            int index = _vertexIndex;

            *(p + index) = vertex1;
            *(p + index + 1) = vertex2;
            *(p + index + 2) = vertex3;
        }

        unchecked
        {
            _vertexIndex += 3;
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
            *(p + index + 3) = quad.TopLeft; 
            *(p + index + 2) = quad.BottomRight;
            *(p + index + 3) = quad.BottomLeft;
        }

        unchecked
        {
            _vertexIndex += 6;
        }

        
    }

    public void End()
    {
        if (Static)
        {
            _vertexBuffer = Graphics.CreateVertexBuffer(new Span<VertexPCT>(_vertices, 0, _vertexIndex), _layout);
        }
    }

    private void IncreaseBuffersIfNeeded(int delta)
    {
        if (_vertexIndex + delta > _vertices.Length)
        {
            Array.Resize(ref _vertices, _vertices.Length * 2);
        }
    }

    
    public void Submit()
    {
        Submit(VertexCount, IndexCount);
    }

    public void Submit(int vertexCount, int indexCount)
    {
        if (Static)
        {
            Graphics.SetVertexBuffer(_vertexBuffer, 0, vertexCount);
        }
        else
        {
            var verticesSpan = new Span<VertexPCT>(_vertices, 0, vertexCount);

            var transientVbo = Graphics.CreateTransientVertexBuffer(verticesSpan, _layout);

            Graphics.SetTransientVertexBuffer(transientVbo, vertexCount);
        }
    }
}