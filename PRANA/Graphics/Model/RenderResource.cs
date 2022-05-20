namespace PRANA;

public abstract class RenderResource : IDisposable
{
    public string Id { get; protected set; }

    protected RenderResource(string id)
    {
        Id = id;
    }

    protected virtual void Free()
    {
    }

    ~RenderResource()
    {
        Free();
        throw new Exception("RenderResource Leak");
    }

    public void Dispose()
    {
        Free();
        GC.SuppressFinalize(this);
    }
}
