using PRANA.Common;

namespace PRANA;

public struct RenderView
{
    public Matrix Transform;
    public Matrix ProjectionMatrix;
    public Color ClearColor;
    public Rectangle ViewRect;
    public ushort Order { get; }

    internal RenderView(ushort order)
    {
        Order = order;
        Transform = Matrix.Identity;
        ProjectionMatrix = Matrix.Identity;
        ClearColor = Color.Blue;
        ViewRect = Rectangle.Empty;
    }

}
