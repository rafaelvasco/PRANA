namespace PRANA;

public interface IVertexStream
{
    int VertexCount { get;}
    int IndexCount { get;}

    void Submit();
    void Submit(int vertexCount, int indexCount);
}
