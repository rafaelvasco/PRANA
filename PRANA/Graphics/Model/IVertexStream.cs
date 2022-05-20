namespace PRANA;

public interface IVertexStream
{
    int VertexCount { get;}

    void Submit();

    void Submit(int startingVertexIndex, int vertexCount, int startingIndiceIndex, int indexCount);
}
