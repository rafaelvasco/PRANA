using PRANA.Common;

namespace PRANA;

public struct RenderView
{
    private static ushort _staticId;

    public Matrix Transform;
    public Matrix ProjectionMatrix;
    public Color ClearColor;
    public Rectangle ViewRect;

    internal ushort Id { get; }

    public RenderView()
    {
        Id = _staticId++;
        Transform = Matrix.Identity;
        ProjectionMatrix = Matrix.Identity;
        ClearColor = Color.Blue;
        ViewRect = Rectangle.Empty;
    }

}
