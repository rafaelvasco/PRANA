namespace PRANA;

public interface IMesh
{
    int VertexCount { get;}
    int IndexCount { get;}

    void Submit();
    void Submit(int vertexCount, int indexCount);
}
