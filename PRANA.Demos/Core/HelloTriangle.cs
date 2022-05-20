using PRANA;
using PRANA.Common;

namespace PRANADEMOS;

public class HelloTriangle : Scene
{
    private VertexStream triangle;
    private RenderView _view;
    private RenderState _state;


    public override void Load()
    {
        _view = Graphics.CreateDefaultView();

        _state = RenderState.Default;

        triangle = new VertexStream("triangle", VertexPCT.VertexLayout, isStatic: true);

        triangle.Begin();

        triangle.PushTriangle(
            new VertexPCT(0f, Game.WindowSize.Height, 0f, Color.Red),
            new VertexPCT(Game.WindowSize.Width, Game.WindowSize.Height, 0f, Color.Green),
            new VertexPCT(Game.WindowSize.Width/2f, 0f, 0f, Color.Blue)
        );

        triangle.End();

        Graphics.MSAALevel = MSAALevel.Four;
    }

    public override void Unload()
    {
    }

    public override void Update(GameTime time)
    {
        if (Input.KeyPressed(Key.Escape))
        {
            Game.Exit();
        }
    }

    public override void Draw(GameTime time)
    {
        Graphics.ApplyRenderView(_view);
        Graphics.ApplyRenderState(_state);

        Graphics.Submit(triangle);
    }
}