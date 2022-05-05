namespace PRANA;

public struct RenderView
{
    private static ushort _staticId = 0;

    public Transform ViewMatrix => _viewMatrix;
    public Transform ProjectionMatrix => _projMatrix;
    public Color ClearColor => _clearColor;
    public Rectangle ViewRect => _viewRect;

    internal ushort Id { get; }


    internal Transform _viewMatrix;
    internal Transform _projMatrix;
    internal Color _clearColor;
    internal Rectangle _viewRect;

    public RenderView()
    {
        Id = _staticId++;
        _viewMatrix = Transform.Identity;
        _projMatrix = Transform.Identity;
        _clearColor = Color.Blue;
        _viewRect = Rectangle.Empty;
    }

    public void SetViewport(int x, int y, int w, int h)
    {
        _viewRect = new Rectangle(x, y, w, h);
    }

    public void SetBackColor(Color color)
    {
        _clearColor = color;
    }

    public void SetTransform(Transform matrix)
    {
        _viewMatrix = matrix;
    }

    public void SetProjection(Transform matrix)
    {
        _projMatrix = matrix;
    }
}
